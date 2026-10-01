using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Views;

public partial class ChorusEditor : UserControl
{
    public ChorusEditor() => InitializeComponent();
    private EditorViewModel Model => (EditorViewModel)DataContext;
    private async void ReadClick(object sender, RoutedEventArgs e) => await Model.ReadChorusAsync();
    private async void RestoreClick(object sender, RoutedEventArgs e) => await Model.RestoreChorusAsync();
    private async void ImportClick(object sender, RoutedEventArgs e) => await Model.ImportChorusAsync();
    private async void ExportClick(object sender, RoutedEventArgs e) => await Model.ExportChorusAsync();
    private void ApplyClick(object sender, RoutedEventArgs e) => Model.ApplyChorusEdits();
}
