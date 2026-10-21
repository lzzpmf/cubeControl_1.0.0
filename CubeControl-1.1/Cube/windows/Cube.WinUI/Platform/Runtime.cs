global using CommandManager = Cube.WinUI.Platform.CommandManager;
using Microsoft.UI.Dispatching;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
namespace Cube.WinUI.Platform;
public static class CommandManager
{
    public static event EventHandler? RequerySuggested;
    public static void InvalidateRequerySuggested() => RequerySuggested?.Invoke(null, EventArgs.Empty);
}
public enum ControlAppearance { Primary, Secondary, Success, Caution, Danger, Transparent }
public enum InfoBarSeverity { Informational, Warning, Error, Success }
public enum SymbolRegular { Pause24, Play24, Speaker224, SpeakerMute24 }
public enum DispatcherPriority { Normal }
public sealed record DispatchOperation<T>(Task<T> Task);
public sealed class Dispatcher
{
    private readonly DispatcherQueue queue = DispatcherQueue.GetForCurrentThread();
    public static Dispatcher CurrentDispatcher => new();
    public bool HasShutdownStarted { get; private set; }
    public Dispatcher() => queue.ShutdownStarting += (_, _) => HasShutdownStarted = true;
    public bool CheckAccess() => queue.HasThreadAccess;
    public Task BeginInvoke(Action action) => InvokeAsync(() => { action(); return true; }, DispatcherPriority.Normal, default).Task;
    public DispatchOperation<T> InvokeAsync<T>(Func<T> action, DispatcherPriority priority, CancellationToken token)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!queue.TryEnqueue(() => { try { token.ThrowIfCancellationRequested(); completion.TrySetResult(action()); } catch (OperationCanceledException) { completion.TrySetCanceled(token); } catch (Exception ex) { completion.TrySetException(ex); } }))
            completion.TrySetException(new InvalidOperationException("A janela foi encerrada."));
        return new(completion.Task);
    }
}
public sealed class DispatcherTimer
{
    private readonly DispatcherQueueTimer timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
    public DispatcherTimer() { timer.Tick += (_, _) => Tick?.Invoke(this, EventArgs.Empty); }
    public event EventHandler? Tick;
    public TimeSpan Interval { get => timer.Interval; set => timer.Interval = value; }
    public bool IsEnabled => timer.IsRunning;
    public void Start() => timer.Start();
    public void Stop() => timer.Stop();
}
public sealed class UiApplication
{
    public static UiApplication Current { get; } = new();
    public Dispatcher Dispatcher { get; } = Dispatcher.CurrentDispatcher;
}
public sealed record PropertyGroupDescription(string Name);
public sealed class ListCollectionView : ObservableCollection<object>
{
    private readonly IList source;
    public Predicate<object>? Filter { get; set; }
    public Collection<PropertyGroupDescription> GroupDescriptions { get; } = [];
    public ListCollectionView(IList source)
    {
        this.source = source;
        if (source is INotifyCollectionChanged observable) observable.CollectionChanged += (_, _) => Refresh();
        Refresh();
    }
    public void Refresh()
    {
        // A Reset would clear native Selector.SelectedItem and write null into the session.
        var desired = source.Cast<object>().Where(value => Filter?.Invoke(value) != false).ToArray();
        var retained = desired.ToHashSet();
        for (int i = Count - 1; i >= 0; i--) if (!retained.Contains(this[i])) RemoveAt(i);
        for (int i = 0; i < desired.Length; i++)
        {
            if (i < Count && Equals(this[i], desired[i])) continue;
            int previous = IndexOf(desired[i]);
            if (previous < 0) Insert(i, desired[i]); else if (previous != i) Move(previous, i);
        }
    }
}
