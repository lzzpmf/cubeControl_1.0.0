using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Cube.Desktop.ViewModels;
namespace Cube.Desktop.Views;
public partial class BackingTracksView : UserControl
{
    private BackingTracksViewModel BackingTracks => Model!;
    private Wpf.Ui.Controls.ContentDialogHost DialogHost => Owner!.DialogHost;
    private MainWindow? Owner => Window.GetWindow(this) as MainWindow;
    private BackingTracksViewModel? Model => DataContext as BackingTracksViewModel;
    public BackingTracksView() { Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag); InitializeComponent(); DataContextChanged += (_, _) => AttachModel(); SizeChanged += (_, _) => ApplyPanels(); Loaded += (_, _) => AttachModel(); Unloaded += (_, _) => { if (attached != null) attached.PropertyChanged -= ModelChanged; attached = null; }; }
    private BackingTracksViewModel? attached;
    private void AttachModel()
    {
        if (attached != null) attached.PropertyChanged -= ModelChanged;
        attached = Model;
        if (attached != null) attached.PropertyChanged += ModelChanged;
        ApplyPanels();
    }
    private void ModelChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    { if (e.PropertyName is nameof(BackingTracksViewModel.LibraryOpen) or nameof(BackingTracksViewModel.LibraryWidth)) ApplyPanels(); }
    internal void ApplyPanels()
    {
        if (BackingSideColumn == null) return;
        bool narrow = ActualWidth > 0 && ActualWidth < 1000;
        if (narrow && !wasNarrow) { wasInspectorOpen = InspectorToggle.IsChecked == true; wasNarrow = true; InspectorToggle.IsChecked = false; }
        else if (!narrow && wasNarrow) { wasNarrow = false; InspectorToggle.IsChecked = wasInspectorOpen; }
        wasNarrow = narrow;
        bool inspector = InspectorToggle.IsChecked == true, library = Model?.LibraryOpen == true;
        InspectorColumn.Width = new GridLength(!narrow && inspector ? 270 : 0);
        BackingSideColumn.Width = new GridLength(!narrow && library ? Model!.LibraryWidth : 0);
        Grid.SetColumn(Inspector, narrow ? 1 : 0);
        Grid.SetColumn(BackingLibraryLayout, narrow ? 1 : 2);
        LibrarySplitter.Visibility = !narrow && library ? Visibility.Visible : Visibility.Collapsed;
        Inspector.Visibility = inspector && (!narrow || !library) ? Visibility.Visible : Visibility.Collapsed;
        BackingTracksScroll.Visibility = narrow && (inspector || library) ? Visibility.Collapsed : Visibility.Visible;
    }
    private bool wasNarrow;
    private bool wasInspectorOpen = true;
    private void InspectorChanged(object sender, RoutedEventArgs e)
    { if (Inspector == null) return; ApplyPanels(); }
    private void MixerResize(object sender, DragDeltaEventArgs e) { if (Model != null) Model.MixerHeight -= e.VerticalChange; }
    private void LibraryResize(object sender, DragDeltaEventArgs e) { if (Model != null) Model.LibraryWidth -= e.HorizontalChange; }
    private void RecentProjectsClick(object sender, RoutedEventArgs e)
    {
        if (Model == null) return;
        var menu = new ContextMenu { PlacementTarget = (UIElement)sender };
        foreach (string path in Model.RecentProjects) { var item = new MenuItem { Header = System.IO.Path.GetFileNameWithoutExtension(path), ToolTip = path }; item.Click += async (_, _) => await Model.OpenProjectAsync(path); menu.Items.Add(item); }
        menu.IsOpen = true;
    }
    private void BackingPageSizeChanged(object sender, SizeChangedEventArgs e) => Owner?.BackingPageSizeChanged(sender, e);
}
