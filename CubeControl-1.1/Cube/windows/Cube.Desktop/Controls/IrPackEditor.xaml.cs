using System.IO;
using System.Windows;
using System.Windows.Controls;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Microsoft.Win32;

namespace Cube.Desktop.Controls;

public partial class IrPackEditor : UserControl
{
    private bool selecting;
    public IrPackEditor() => InitializeComponent();
    private void OpenMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.ContextMenu is { } menu)
        {
            menu.PlacementTarget = element;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menu.IsOpen = true;
        }
    }
    private async void PackSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (selecting || DataContext is not IrPackEditorViewModel model) return;
        selecting = true;
        try
        {
            var selected = PackSelector.SelectedItem as IrPack;
            // ItemsSource replacement can transiently clear selection during catalog refresh.
            if (selected != null) await model.SelectAsync(selected);
            PackSelector.GetBindingExpression(ComboBox.SelectedItemProperty)?.UpdateTarget();
        }
        finally { selecting = false; }
    }
    private async void ImportClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not IrPackEditorViewModel model || model.Owner.Busy) return;
        var dialog = new OpenFileDialog { Filter = "CubeControl IR Pack|*.cubeirpack.zip;*.cubeirpack.json" };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true && await model.ConfirmDiscard()) TryFileAction(model, () => model.Import(dialog.FileName));
    }
    private async void ExportClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not IrPackEditorViewModel model || model.Owner.Busy || !model.HasEntries || !await model.EnsureDetailsAsync()) return;
        var dialog = new SaveFileDialog { Filter = "CubeControl IR Pack|*.cubeirpack.zip", FileName = "Pack.cubeirpack.zip" };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true) TryFileAction(model, () => model.Export(dialog.FileName));
    }
    private static void TryFileAction(IrPackEditorViewModel model, Action action)
    {
        try { action(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or System.Text.Json.JsonException or InvalidOperationException) { model.ReportError(ex.Message); }
    }
}
