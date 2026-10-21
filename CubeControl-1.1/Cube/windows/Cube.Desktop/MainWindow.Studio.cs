using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop;

public partial class MainWindow
{
    private void StudioPitchStep(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string step } && int.TryParse(step, out int delta)) BackingTracks.Pitch += delta;
    }
    private void StudioOptionsClick(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu();
        void Add(string label, Action action)
        {
            var item = new MenuItem { Header = LanguageService.T(label) };
            item.Click += (_, _) => action(); menu.Items.Add(item);
        }
        Add("Nova pista", () => BackingTracks.AddEmptyStem.Execute(null));
        Add("Sessões e entrada", () => { BackingSessionsPopup.DataContext = BackingTracks; BackingSessionsPopup.IsOpen = true; });
        Add("Biblioteca de áudio", () => ToggleBackingLibrary(sender, e));
        Add("Adicionar pasta", () => BackingTracks.AddLibraryFolder.Execute(null));
        Add("Parar pré-escuta", () => { BackingTracks.StopYouTubePreview(); _ = BackingTracks.StopLocalPreview(); });
        Add("Desfazer", () => BackingTracks.UndoEdit());
        Add("Refazer", () => BackingTracks.UndoEdit(true));
        menu.PlacementTarget = sender as UIElement; menu.IsOpen = true;
    }
    private async void PreviewLocalFile(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: LocalAudioFile file }) await BackingTracks.PreviewLocalAudio(file);
    }
    private async void InsertLocalFile(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: LocalAudioFile file })
            try { await BackingTracks.InsertLocalAudio(file.Path); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, LanguageService.T("Importar áudio")); }
    }
    private void LocalFileContextOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: LocalAudioFile file } element) return;
        var menu = new ContextMenu();
        foreach (string category in BackingTracks.LocalCategories)
        {
            var item = new MenuItem { Header = LanguageService.T(category), IsCheckable = true, IsChecked = file.Category == category || category == "Todos" && file.Category == "" };
            item.Click += (_, _) => BackingTracks.CategorizeLocalFile(file, category); menu.Items.Add(item);
        }
        element.ContextMenu = menu;
        menu.PlacementTarget = element; menu.IsOpen = true; e.Handled = true;
    }
    private Point? localFileDragOrigin;
    private void LocalFileDragStart(object sender, MouseButtonEventArgs e) => localFileDragOrigin = e.GetPosition(this);
    private void LocalFileDrag(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) { localFileDragOrigin = null; return; }
        if (localFileDragOrigin is not { } origin || sender is not FrameworkElement { DataContext: LocalAudioFile file } element) return;
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null && node != element; node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
            if (node is System.Windows.Controls.Primitives.ButtonBase) return;
        var point = e.GetPosition(this);
        if (Math.Abs(point.X - origin.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(point.Y - origin.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        localFileDragOrigin = null;
        DragDrop.DoDragDrop(element, new DataObject(DataFormats.FileDrop, new[] { file.Path }), DragDropEffects.Copy);
    }
}
