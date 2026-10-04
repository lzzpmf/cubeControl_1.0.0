using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Views;

public partial class PhaserEditor : UserControl
{
    public PhaserEditor() => InitializeComponent();
    private async void ReadClick(object sender, RoutedEventArgs e) => await Model.ReadPhaserAsync();
    private EditorViewModel Model => (EditorViewModel)DataContext;
    private async void ListenClick(object sender, RoutedEventArgs e) => await Model.ListenAdvancedModulationAsync(2);
    private async void RestoreClick(object sender, RoutedEventArgs e) => await Model.RestorePhaserAsync();
    private async void ImportClick(object sender, RoutedEventArgs e) => await Model.ImportPhaserAsync();
    private async void ExportClick(object sender, RoutedEventArgs e) => await Model.ExportPhaserAsync();
}
