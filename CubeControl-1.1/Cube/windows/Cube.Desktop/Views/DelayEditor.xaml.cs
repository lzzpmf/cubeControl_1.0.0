using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Views;

public partial class DelayEditor : UserControl
{
    public DelayEditor() => InitializeComponent();
    private async void ReadClick(object sender, RoutedEventArgs e) => await Model.ReadDelayAsync();
    private EditorViewModel Model => (EditorViewModel)DataContext;
    private async void RestoreClick(object sender, RoutedEventArgs e) => await Model.RestoreDelayAsync();
    private async void ImportClick(object sender, RoutedEventArgs e) => await Model.ImportDelayAsync();
    private async void ExportClick(object sender, RoutedEventArgs e) => await Model.ExportDelayAsync();
}
