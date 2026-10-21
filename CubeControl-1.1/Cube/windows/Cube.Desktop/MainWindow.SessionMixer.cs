using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Controls;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop;

public partial class MainWindow
{
    private void ToggleBackingLibrary(object sender, RoutedEventArgs e) { BackingLibraryLayout.Visibility = BackingLibraryLayout.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; ApplyBackingLayout(BackingPage.ActualWidth); }
    private void BackingGoToStart(object sender, RoutedEventArgs e) => BackingTracks.Position = 0;
    private void BackingUndo(object sender, RoutedEventArgs e) => BackingTracks.UndoEdit();
    private void BackingRedo(object sender, RoutedEventArgs e) => BackingTracks.UndoEdit(true);
    private void BackingTransportSizeChanged(object sender, SizeChangedEventArgs e) =>
        ConfigureBackingTransport(BackingPage.ActualWidth, BackingPage.ActualHeight);
    private void ConfigureBackingTransport(double width, double height)
    {
        if (BackingWorkspace == null || BackingTransportHost == null) return;
        BackingWorkspace.Height = Math.Max(360, height - BackingTransportHost.ActualHeight);
    }
    private void SessionTrackWidthChanged(object sender, SizeChangedEventArgs e) => BackingTracks.TrackHeaderWidth = 335;
    private void SessionTrackLayoutUpdated(object? sender, EventArgs e)
    {
        if (!BackingStemList.IsVisible || BackingMainWave.Parent is not FrameworkElement rulerHost) return;
        // Use a realized waveform, not the outer ListBox width: its viewport excludes
        // the scrollbar and item chrome. LayoutUpdated also covers scrollbar changes.
        static BackingWaveform? FindWave(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is BackingWaveform wave) return wave;
                if (FindWave(child) is { } nested) return nested;
            }
            return null;
        }
        if (FindWave(BackingStemList) is not { ActualWidth: > 0 } trackWave) return;
        double left = trackWave.TranslatePoint(new Point(), rulerHost).X;
        double right = Math.Max(0, rulerHost.ActualWidth - left - trackWave.ActualWidth);
        var margin = new Thickness(Math.Max(0, left), 0, right, 0);
        if (Math.Abs(BackingMainWave.Margin.Left - margin.Left) > .1 ||
            Math.Abs(BackingMainWave.Margin.Right - margin.Right) > .1)
            BackingMainWave.Margin = margin;
    }
    private void BackingTrackHeightWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) return;
        BackingTracks.Zoom *= e.Delta > 0 ? 1.25 : .8;
        e.Handled = true;
    }
    private void SessionStemContextOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: BackingStem stem } element) return;
        BackingTracks.SelectedStem = stem;
        var menu = new ContextMenu();
        void Add(string label, ICommand command) => menu.Items.Add(new MenuItem { Header = LanguageService.T(label), Command = command });
        Add("Nova pista", BackingTracks.AddEmptyStem);
        Add("Duplicar pista", BackingTracks.DuplicateStem);
        Add("Remover pista", BackingTracks.RemoveStem);
        Add("Localizar áudio da pista", BackingTracks.LocateStem);
        Add("Mover pista para cima", BackingTracks.StemUp);
        Add("Mover pista para baixo", BackingTracks.StemDown);
        element.ContextMenu = menu;
    }
}
