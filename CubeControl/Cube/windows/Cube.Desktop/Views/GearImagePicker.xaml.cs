using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using Microsoft.Win32;

namespace Cube.Desktop.Views;

public partial class GearImagePicker : UserControl
{
    public event Action? ApplyRequested;
    public event Action? CancelRequested;
    private readonly EquipmentImageEntry[] catalog;
    private Controls.EquipmentImageGridPanel? imageGrid;
    private void GridLoaded(object sender, RoutedEventArgs e) => imageGrid = (Controls.EquipmentImageGridPanel)sender;
    private static readonly string[] categories = ["Todos", "Amplificadores", "Gabinetes", "Efeitos", "Dispositivos", "Microfones", "Guitarras", "Racks"];
    public string? SelectedPath { get; private set; }
    public GearImagePicker(GearItem target)
    {
        catalog = EquipmentImageEntry.Discover(Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Equipment"),
            Path.Combine(AppContext.BaseDirectory, "Assets", "Images", "Catalog"));
        InitializeComponent();
        Category.ItemsSource = categories.Select(LanguageService.T).ToArray();
        Category.SelectedIndex = target.Kind switch { "amp" => 1, "cab" => 2, "chorus" or "phaser" or "delay" or "reverb" => 3, _ => 0 };
    }
    private void FilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Images == null) return;
        Images.ItemsSource = catalog.Where(i => Category.SelectedIndex == 0 || i.Category == categories[Category.SelectedIndex]).ToArray();
        // A PC selection remains provisional while browsing categories.
        if (SelectedPath != null) Images.SelectedItem = catalog.FirstOrDefault(i => i.Path == SelectedPath);
        Images.ScrollIntoView(Images.SelectedItem ?? Images.Items.Cast<object>().FirstOrDefault());
    }
    private void ImageSelected(object sender, SelectionChangedEventArgs e)
    {
        if (Images.SelectedItem is EquipmentImageEntry item) SelectPath(item.Path);
    }
    public void SelectPath(string path)
    {
        SelectedPath = path; SelectionName.Text = Path.GetFileNameWithoutExtension(path);
        Apply.IsEnabled = true; Error.Visibility = Visibility.Collapsed;
    }
    public void ShowError(string message) { Error.Text = LanguageService.T(message); Error.Visibility = Visibility.Visible; }
    private void ChooseComputer(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = LanguageService.T("Imagens PNG/JPEG|*.png;*.jpg;*.jpeg"), CheckFileExists = true };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
        try { EditorViewModel.ValidateGearImage(dialog.FileName); Images.SelectedItem = null; SelectPath(dialog.FileName); }
        catch (Exception ex) { ShowError(ex.Message); }
    }
    private void ApplyClick(object sender, RoutedEventArgs e) { if (SelectedPath != null) ApplyRequested?.Invoke(); }
    private void CancelClick(object sender, RoutedEventArgs e) => CancelRequested?.Invoke();
    private void NavigateImages(object sender, KeyEventArgs e)
    {
        if (Images.Items.Count == 0 || Keyboard.Modifiers != ModifierKeys.None) return;
        int columns = imageGrid?.Columns ?? 1;
        int index = Images.SelectedIndex;
        int next = e.Key switch
        {
            Key.Left => index - 1, Key.Right => index + 1,
            Key.Up => index - columns, Key.Down => index + columns,
            Key.Home => 0, Key.End => Images.Items.Count - 1, _ => -1
        };
        if (e.Key is not (Key.Left or Key.Right or Key.Up or Key.Down or Key.Home or Key.End)) return;
        if (index < 0 && e.Key != Key.End) next = 0;
        next = Math.Clamp(next, 0, Images.Items.Count - 1);
        Images.SelectedIndex = next; Images.ScrollIntoView(Images.SelectedItem);
        Images.UpdateLayout();
        (Images.ItemContainerGenerator.ContainerFromIndex(next) as ListBoxItem)?.Focus();
        e.Handled = true;
    }
    private void ApplyDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.OriginalSource is DependencyObject source &&
            ItemsControl.ContainerFromElement(Images, source) is ListBoxItem { DataContext: EquipmentImageEntry item })
        { SelectPath(item.Path); e.Handled = true; ApplyRequested?.Invoke(); }
    }
}
