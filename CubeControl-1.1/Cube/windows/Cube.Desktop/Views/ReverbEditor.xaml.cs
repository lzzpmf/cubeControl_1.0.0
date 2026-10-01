using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Views;

public partial class ReverbEditor : UserControl
{
    public ReverbEditor() => InitializeComponent();
    private EditorViewModel Model => (EditorViewModel)DataContext;
    private async void ReadClick(object sender, RoutedEventArgs e) => await Model.ReadReverbAsync();
    private async void RestoreClick(object sender, RoutedEventArgs e) => await Model.RestoreReverbAsync();
    private async void ImportClick(object sender, RoutedEventArgs e) => await Model.ImportReverbAsync();
    private async void ExportClick(object sender, RoutedEventArgs e) => await Model.ExportReverbAsync();
    private void ApplyClick(object sender, RoutedEventArgs e) => Model.ApplyReverbEdits();
}
