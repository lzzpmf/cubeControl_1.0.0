using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Views;

public partial class ChorusEditor : UserControl
{
    public ChorusEditor() => InitializeComponent();
    private async void ReadClick(object sender, RoutedEventArgs e) => await Model.ReadChorusAsync();
    private EditorViewModel Model => (EditorViewModel)DataContext;
    private async void ListenClick(object sender, RoutedEventArgs e) => await Model.ListenAdvancedModulationAsync(1);
    private async void RestoreClick(object sender, RoutedEventArgs e) => await Model.RestoreChorusAsync();
    private async void ImportClick(object sender, RoutedEventArgs e) => await Model.ImportChorusAsync();
    private async void ExportClick(object sender, RoutedEventArgs e) => await Model.ExportChorusAsync();
}
