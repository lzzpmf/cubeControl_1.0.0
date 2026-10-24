using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Studio.Controls;
using Cube.Desktop.Studio.ViewModels;

namespace Cube.Desktop.Studio.Views;

/// <summary>View-only input routing for the two fixed practice roles.</summary>
public partial class StudioView : UserControl
{
    private ShellViewModel? _viewModel;

    public StudioView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => AttachViewModel();
        Loaded += (_, _) => AttachViewModel();
        Unloaded += (_, _) => DetachViewModel();
    }

    public bool IsTimelineKeyboardFocus => Timeline.IsKeyboardFocusWithin;
    public void FitTimeline() => Timeline.FitProject();
    public void FinishEdits()
    {
        if (_viewModel is null) return;
        if (SpeedBox.IsKeyboardFocusWithin) ApplyNumber(SpeedBox, _viewModel.SetSpeedFromUi);
        if (TempoBox.IsKeyboardFocusWithin) ApplyNumber(TempoBox, _viewModel.SetTempoFromUi);
    }

    private void AttachViewModel()
    {
        if (ReferenceEquals(_viewModel, DataContext)) return;
        DetachViewModel();
        if (DataContext is not ShellViewModel vm) return;
        _viewModel = vm;
        vm.PropertyChanged += ViewModel_PropertyChanged;
        RebuildHeaders();
    }

    private void DetachViewModel()
    {
        if (_viewModel is null) return;
        FinishEdits();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        _viewModel = null;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShellViewModel.Project)) RebuildHeaders();
    }

    private void RebuildHeaders()
    {
        TrackHeaders.Children.Clear();
        TrackHeaders.RowDefinitions.Clear();
        if (_viewModel is null) return;
        foreach (var track in _viewModel.Project.Tracks.Take(2))
        {
            var row = TrackHeaders.RowDefinitions.Count;
            TrackHeaders.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            var header = new ContentControl
            {
                Content = track,
                ContentTemplate = (DataTemplate)FindResource("TrackHeaderTemplate"),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };
            Grid.SetRow(header, row);
            TrackHeaders.Children.Add(header);
        }
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
    private void Tempo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { if (_viewModel is { } vm) ApplyNumber(TempoBox, vm.SetTempoFromUi); }
    private void Speed_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { if (_viewModel is { } vm) ApplyNumber(SpeedBox, vm.SetSpeedFromUi); }
    private void Tempo_KeyDown(object sender, KeyEventArgs e) { if (_viewModel is { } vm) NumericKeyDown(TempoBox, vm.SetTempoFromUi, e); }
    private void Speed_KeyDown(object sender, KeyEventArgs e) { if (_viewModel is { } vm) NumericKeyDown(SpeedBox, vm.SetSpeedFromUi, e); }

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
    private void Audio_Click(object sender, RoutedEventArgs e) { if (_viewModel is { } vm) vm.IsAudioOptionsOpen = !vm.IsAudioOptionsOpen; }
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
        if (vm.IsMetronomeOptionsOpen && !Within(source, MetronomeCard) && !Within(source, MetronomeOptionsTrigger)) vm.IsMetronomeOptionsOpen = false;
        if (vm.IsAudioOptionsOpen && !Within(source, AudioCard) && !Within(source, AudioTrigger)) vm.IsAudioOptionsOpen = false;
    }

    private static DependencyObject? GetParentNode(DependencyObject node) => node is Visual or System.Windows.Media.Media3D.Visual3D
        ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
    private static bool Within(DependencyObject? node, DependencyObject ancestor)
    {
        while (node is not null) { if (node == ancestor) return true; node = GetParentNode(node); }
        return false;
    }
    private static bool IsTextOrValueInput(DependencyObject? node)
    {
        while (node is not null) { if (node is TextBoxBase or ComboBox or KnobControl or Slider) return true; node = GetParentNode(node); }
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
