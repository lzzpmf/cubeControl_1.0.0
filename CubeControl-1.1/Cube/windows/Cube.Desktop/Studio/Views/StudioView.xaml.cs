using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Studio.Controls;
using Cube.Desktop.Studio.ViewModels;

namespace Cube.Desktop.Studio.Views;

/// <summary>View-only input routing for the two fixed practice roles.</summary>
public partial class StudioView : UserControl
{
    private ShellViewModel? _viewModel;
    private Window? _hostWindow;

    public StudioView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => AttachViewModel();
        Loaded += (_, _) => AttachViewModel();
        Unloaded += (_, _) => DetachViewModel();
        IsVisibleChanged += (_, _) => UpdateVisualActivity();
    }

    public bool IsTimelineKeyboardFocus => Timeline.IsKeyboardFocusWithin || GuitarTimeline.IsKeyboardFocusWithin;
    public void FitTimeline() => Timeline.FitProject();
    public void FinishEdits()
    {
        if (_viewModel is null) return;
        if (TempoBox.IsKeyboardFocusWithin) ApplyNumber(TempoBox, _viewModel.SetTempoFromUi);
    }

    private void AttachViewModel()
    {
        if (ReferenceEquals(_viewModel, DataContext)) return;
        DetachViewModel();
        if (DataContext is not ShellViewModel vm) return;
        _viewModel = vm;
        _hostWindow = Window.GetWindow(this);
        if (_hostWindow != null) _hostWindow.StateChanged += HostWindow_StateChanged;
        UpdateVisualActivity();
        vm.PropertyChanged += ViewModel_PropertyChanged;
        RebuildHeaders();
    }

    private void DetachViewModel()
    {
        if (_viewModel is null) return;
        FinishEdits();
        if (_hostWindow != null) _hostWindow.StateChanged -= HostWindow_StateChanged;
        _hostWindow = null;
        _viewModel.SetVisualActivity(false);
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        _viewModel = null;
    }
    private void HostWindow_StateChanged(object? sender, EventArgs e) => UpdateVisualActivity();
    private void UpdateVisualActivity() => _viewModel?.SetVisualActivity(IsVisible && _hostWindow?.WindowState != WindowState.Minimized);

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShellViewModel.Project)) RebuildHeaders();
    }

    private void RebuildHeaders()
    {
        BackingHeader.Content = _viewModel?.Project.Tracks.ElementAtOrDefault(0);
        GuitarHeader.Content = _viewModel?.Project.Tracks.ElementAtOrDefault(1);
    }

    private bool _syncingTimeline;
    private void Timeline_ViewChanged(object? sender, EventArgs e)
    {
        if (_syncingTimeline || sender is not TimelineControl source || GuitarTimeline is null || Timeline is null) return;
        _syncingTimeline = true;
        try { (ReferenceEquals(source, Timeline) ? GuitarTimeline : Timeline).MatchViewport(source); }
        finally { _syncingTimeline = false; }
    }

    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (sender is ContextMenu menu) StudioPopupScale.Apply(menu);
    }
    private void Workspace_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ControlsRow == null || WorkspaceLayout == null || PlaybackDrawer == null) return;
        bool stackedSearch = e.NewSize.Width < 1050;
        bool stackedControls = e.NewSize.Width < 700;
        WorkspaceLayout.ColumnDefinitions[1].Width = stackedSearch ? new GridLength(0) : new GridLength(0.46, GridUnitType.Star);
        Grid.SetColumn(PlaybackDrawer, stackedSearch ? 0 : 1);
        Grid.SetRow(PlaybackDrawer, stackedSearch ? 2 : 0);
        Grid.SetRowSpan(PlaybackDrawer, stackedSearch ? 1 : 2);
        PlaybackDrawer.Margin = stackedSearch ? new Thickness(0, 8, 0, 0) : new Thickness(12, 0, 0, 0);
        Grid.SetRowSpan(MetronomePanel, stackedControls ? 1 : 2);
        Grid.SetColumnSpan(MetronomePanel, stackedControls ? 2 : 1);
        MetronomePanel.Margin = stackedControls ? new Thickness(0) : new Thickness(0, 0, 6, 0);
        Grid.SetRow(PitchSpeedPanel, stackedControls ? 1 : 0);
        Grid.SetColumn(PitchSpeedPanel, stackedControls ? 0 : 1);
        Grid.SetColumnSpan(PitchSpeedPanel, stackedControls ? 2 : 1);
        PitchSpeedPanel.Margin = stackedControls ? new Thickness(0, 8, 0, 0) : new Thickness(6, 0, 0, 0);
        Grid.SetRow(TransportBar, stackedControls ? 2 : 1);
        Grid.SetColumn(TransportBar, stackedControls ? 0 : 1);
        Grid.SetColumnSpan(TransportBar, stackedControls ? 2 : 1);
        TransportBar.Margin = stackedControls ? new Thickness(0, 8, 0, 0) : new Thickness(6, 8, 0, 0);
    }

    private static void ApplyNumber(TextBox box, Action<double> apply)
    {
        if (double.TryParse(box.Text.Trim().TrimEnd('%').Replace(',', '.'), NumberStyles.Float,
            CultureInfo.InvariantCulture, out var value) && double.IsFinite(value)) apply(value);
        box.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
    }

    private void NumericKeyDown(TextBox box, Action<double> apply, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { ApplyNumber(box, apply); Timeline.Focus(); e.Handled = true; }
        else if (e.Key == Key.Escape) { box.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget(); Timeline.Focus(); e.Handled = true; }
    }
    private void Tempo_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_viewModel is not { } vm || e.Delta == 0) return;
        ApplyNumber(TempoBox, vm.SetTempoFromUi);
        vm.SetTempoFromUi(vm.MetronomeBpm + e.Delta / 120d);
        e.Handled = true;
    }

    private void Tempo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { if (_viewModel is { } vm) ApplyNumber(TempoBox, vm.SetTempoFromUi); }
    private void Tempo_KeyDown(object sender, KeyEventArgs e) { if (_viewModel is { } vm) NumericKeyDown(TempoBox, vm.SetTempoFromUi, e); }
    private void TempoUp_Click(object sender, RoutedEventArgs e) { if (_viewModel is { } vm) { ApplyNumber(TempoBox, vm.SetTempoFromUi); vm.SetTempoFromUi(vm.MetronomeBpm + 1); } }
    private void TempoDown_Click(object sender, RoutedEventArgs e) { if (_viewModel is { } vm) { ApplyNumber(TempoBox, vm.SetTempoFromUi); vm.SetTempoFromUi(vm.MetronomeBpm - 1); } }

    private void Timeline_SeekRequested(object? sender, TimelineSeekEventArgs e) => _viewModel?.Seek(e.Time);
    private void Timeline_FilesDropped(object? sender, TimelineFilesDroppedEventArgs e) => _viewModel?.ImportPaths(e.Paths);
    private void Timeline_EditRequested(object? sender, TimelineEditEventArgs e)
    {
        if (_viewModel is not { CanEditTimeline: true } vm) return;
        if (e.Kind == TimelineEditKind.Loop) vm.SetLoop(e.Start, e.Start + e.Duration);
        else if (e.Kind == TimelineEditKind.Trim) vm.TrimSelectedAttempt(e.Start, e.Duration);
        else if (e.Kind == TimelineEditKind.Delete && vm.DeleteAttemptCommand.CanExecute(null)) vm.DeleteAttemptCommand.Execute(null);
    }

    private void MetronomeOptions_Click(object sender, RoutedEventArgs e) { if (_viewModel is { } vm) vm.IsMetronomeOptionsOpen = !vm.IsMetronomeOptionsOpen; }
    private void Audio_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is not { } vm) return;
        vm.IsAudioOptionsOpen = !vm.IsAudioOptionsOpen;
        if (vm.IsAudioOptionsOpen && vm.RefreshAudioDevicesCommand.CanExecute(null)) vm.RefreshAudioDevicesCommand.Execute(null);
    }
    private void Playback_MouseEnter(object sender, MouseEventArgs e) { if (sender is FrameworkElement { DataContext: PlaybackResultViewModel result }) _viewModel?.PreviewHover(result, true); }
    private void Playback_MouseLeave(object sender, MouseEventArgs e) { if (sender is FrameworkElement { DataContext: PlaybackResultViewModel result }) _viewModel?.PreviewHover(result, false); }
    private void PlaybackDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: PlaybackResultViewModel result } button)
        {
            var details = new ContextMenu { PlacementTarget = button, Placement = PlacementMode.Bottom };
            details.Items.Add(new MenuItem { Header = new TextBlock { Text = result.Details, TextWrapping = TextWrapping.Wrap, MaxWidth = 280 } });
            details.Opened += (_, _) => StudioPopupScale.Apply(details);
            button.ContextMenu = details;
            details.IsOpen = true;
        }
    }

    private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel is not { } vm) return;
        var source = e.OriginalSource as DependencyObject;
        if (vm.IsAudioOptionsOpen && !Within(source, AudioCard) && !Within(source, AudioTrigger)) vm.IsAudioOptionsOpen = false;
    }

    private static DependencyObject? GetParentNode(DependencyObject node)
    {
        if (node is ComboBoxItem item && ItemsControl.ItemsControlFromItemContainer(item) is { } owner) return owner;
        if (node is Popup popup) return popup.PlacementTarget ?? popup.TemplatedParent;
        return node is Visual or System.Windows.Media.Media3D.Visual3D
            ? VisualTreeHelper.GetParent(node) ?? LogicalTreeHelper.GetParent(node)
            : LogicalTreeHelper.GetParent(node);
    }
    private static bool Within(DependencyObject? node, DependencyObject ancestor)
    {
        while (node is not null) { if (node == ancestor) return true; node = GetParentNode(node); }
        return false;
    }
    private static bool IsTextOrValueInput(DependencyObject? node)
    {
        while (node is not null) { if (node is TextBoxBase or ComboBox or KnobControl or Cube.Desktop.Controls.Knob or Slider) return true; node = GetParentNode(node); }
        return false;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_viewModel is not { } vm || !IsVisible || !IsKeyboardFocusWithin) return;
        if (e.Key == Key.Escape && (vm.IsAudioOptionsOpen || vm.IsMetronomeOptionsOpen))
        {
            FinishEdits(); vm.ClosePopups(); Timeline.Focus(); e.Handled = true; return;
        }
        // Text fields keep their own editing shortcuts, including Ctrl+Z and Space.
        if (IsTextOrValueInput(e.OriginalSource as DependencyObject)) return;
        ICommand? command = (e.Key, Keyboard.Modifiers) switch
        {
            (Key.Space, ModifierKeys.None) => vm.PlayPauseCommand,
            (Key.R, ModifierKeys.None) => vm.RecordCommand,
            (Key.M, ModifierKeys.None) => vm.ToggleMetronomeCommand,
            (Key.L, ModifierKeys.None) => vm.ToggleLoopCommand,
            (Key.A, ModifierKeys.None) => vm.SetLoopStartCommand,
            (Key.B, ModifierKeys.None) => vm.SetLoopEndCommand,
            (Key.Delete, ModifierKeys.None) => vm.DeleteAttemptCommand,
            (Key.I, ModifierKeys.Control) => vm.ImportCommand,
            (Key.Z, ModifierKeys.Control) => vm.UndoCommand,
            (Key.Y, ModifierKeys.Control) => vm.RedoCommand,
            (Key.Z, ModifierKeys.Control | ModifierKeys.Shift) => vm.RedoCommand,
            _ => null
        };
        if (command is null) return;
        if (!e.IsRepeat && command.CanExecute(null)) command.Execute(null);
        e.Handled = true;
    }
}
