using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using NAudio.CoreAudioApi;
using Windows.Devices.Enumeration;
using Windows.Media.Devices;
namespace Cube.Desktop.ViewModels;
public sealed record BackingOutput(string Id, string Name, bool Available = true);
public sealed partial class BackingTracksViewModel : Observable, IDisposable
{
    private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
    private readonly CancellationTokenSource lifetime = new();
    private DeviceWatcher? watcher;
    private bool disposed, pageVisible, refreshing;
    private double volume = 30;
    private bool muted;
    private string status = "";
    private BackingOutput? selectedOutput;
    public ObservableCollection<BackingOutput> Outputs { get; } = [];
    public Func<Task>? BeforePlayback { get; set; }
    public ICommand RefreshOutputs { get; }
    public BackingTracksViewModel(string dataDirectory)
    {
        InitializeYouTubeDownloads(dataDirectory);
        RefreshOutputs = new UiCommand(() => Start(RefreshDevices()), () => !refreshing && !disposed);
    }
    public bool PageVisible { get => pageVisible; set { if (Set(ref pageVisible, value) && !value) StopYouTubePreview(); } }
    public string Status { get => status; private set => Set(ref status, value); }
    public double Volume { get => volume; set { if (double.IsFinite(value) && Set(ref volume, Math.Clamp(value, 0, 100))) UpdateYouTubePreviewVolume(); } }
    public bool IsMuted { get => muted; set { if (Set(ref muted, value)) UpdateYouTubePreviewVolume(); } }
    public BackingOutput? SelectedOutput { get => selectedOutput; set { if (Set(ref selectedOutput, value)) StopYouTubePreview(); } }
    public Task StopForIrAudition() => StopYouTubePreviewAndWait();
    public async Task Initialize()
    {
        if (disposed || watcher != null) return;
        try
        {
            watcher = DeviceInformation.CreateWatcher(MediaDevice.GetAudioRenderSelector());
            watcher.Added += DeviceChanged; watcher.Removed += DeviceUpdated; watcher.Updated += DeviceUpdated;
            watcher.Start();
            await RefreshDevices();
        }
        catch (Exception ex)
        {
            Log(ex); Status = "Não foi possível listar as saídas de áudio. Tente atualizar saídas.";
        }
    }
    private void DeviceChanged(DeviceWatcher sender, DeviceInformation args) => Dispatch(() => Start(RefreshDevices()));
    private void DeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate args) => Dispatch(() => Start(RefreshDevices()));
    private async Task RefreshDevices()
    {
        if (disposed || refreshing) return;
        refreshing = true; CommandManager.InvalidateRequerySuggested();
        try
        {
            var (devices, defaultId) = await Task.Run(() =>
            {
                using var enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).Select(device =>
                { using (device) return new BackingOutput(device.ID, device.FriendlyName); }).ToArray();
                string? id = null;
                try { using var endpoint = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia); id = endpoint.ID; } catch (Exception ex) { Log(ex); }
                return (devices, id);
            });
            if (disposed) return;
            string? selectedId = SelectedOutput?.Id;
            foreach (var old in Outputs.ToArray()) if (!devices.Any(d => d.Id == old.Id)) Outputs.Remove(old);
            foreach (var device in devices) if (!Outputs.Any(d => d.Id == device.Id)) Outputs.Add(device);
            SelectedOutput = Outputs.FirstOrDefault(d => d.Id == selectedId) ?? Outputs.FirstOrDefault(d => d.Id == defaultId) ?? Outputs.FirstOrDefault();
            Status = Outputs.Count == 0 ? "Nenhuma saída disponível. Conecte uma saída de áudio e atualize a lista." : "";
        }
        catch (Exception ex) { Log(ex); Status = "Não foi possível listar as saídas de áudio. Tente atualizar saídas."; }
        finally { refreshing = false; CommandManager.InvalidateRequerySuggested(); }
    }
    private static void Log(Exception ex) => System.Diagnostics.Debug.WriteLine(ex);
    private async void Start(Task task) { try { await task; } catch (OperationCanceledException) { } catch (Exception ex) { Log(ex); } }
    private void Dispatch(Action action)
    {
        if (!disposed && !dispatcher.HasShutdownStarted) dispatcher.BeginInvoke(new Action(() => { if (!disposed) action(); }));
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true; lifetime.Cancel(); StopYouTubePreview();
        if (watcher != null)
        {
            watcher.Added -= DeviceChanged; watcher.Removed -= DeviceUpdated; watcher.Updated -= DeviceUpdated;
            if (watcher.Status is DeviceWatcherStatus.Started or DeviceWatcherStatus.EnumerationCompleted) watcher.Stop();
        }
        lifetime.Dispose();
    }
}
