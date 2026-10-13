using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
    private sealed record LibraryDrag(IrPackEditorViewModel Editor, IrLibraryEntry Entry);
    private sealed record SlotDrag(IrPackEditorViewModel Editor, int Index);
    private Point dragStart;
    private object? dragCandidate;
    private static T? Ancestor<T>(DependencyObject? node) where T : DependencyObject
    {
        while (node != null)
        {
            if (node is T found) return found;
            node = node is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
        }
        return null;
    }
    internal void LibraryMouseDown(object sender, MouseButtonEventArgs e)
    {
        dragCandidate = null;
        if (DataContext is IrPackEditorViewModel { CanEdit: true } model &&
            Ancestor<TreeViewItem>(e.OriginalSource as DependencyObject)?.DataContext is IrLibraryNode { Entry: { } entry })
        { dragStart = e.GetPosition(this); dragCandidate = new LibraryDrag(model, entry); }
    }
    internal void LibraryMouseMove(object sender, MouseEventArgs e) => StartDrag(sender, e);
    private void SlotMouseDown(object sender, MouseButtonEventArgs e)
    {
        dragCandidate = null;
        if (Ancestor<System.Windows.Controls.Primitives.ButtonBase>(e.OriginalSource as DependencyObject) != null) return;
        if (sender is Border { DataContext: IrPackSlotViewModel slot } card)
        {
            card.Focus();
            if (DataContext is IrPackEditorViewModel { CanEdit: true } model && slot.HasEntry)
            { dragStart = e.GetPosition(this); dragCandidate = new SlotDrag(model, slot.Index); }
        }
    }
    private void SlotMouseMove(object sender, MouseEventArgs e) => StartDrag(sender, e);
    private void StartDrag(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) { dragCandidate = null; return; }
        if (DataContext is not IrPackEditorViewModel { CanEdit: true } || dragCandidate == null) return;
        var position = e.GetPosition(this);
        if (Math.Abs(position.X - dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(position.Y - dragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        var candidate = dragCandidate; dragCandidate = null;
        var data = new DataObject(candidate.GetType(), candidate);
        try { DragDrop.DoDragDrop((DependencyObject)sender, data, candidate is LibraryDrag ? DragDropEffects.Copy : DragDropEffects.Move); }
        finally { foreach (var card in Cards(this)) card.Tag = null; }
        e.Handled = true;
    }
    private static IEnumerable<Border> Cards(DependencyObject root)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is Border { DataContext: IrPackSlotViewModel } card) yield return card;
            foreach (var nested in Cards(child)) yield return nested;
        }
    }
    private DragDropEffects DropEffect(DragEventArgs e, IrPackSlotViewModel slot)
    {
        if (DataContext is not IrPackEditorViewModel { CanEdit: true } model) return DragDropEffects.None;
        if (e.Data.GetData(typeof(LibraryDrag)) is LibraryDrag library && ReferenceEquals(library.Editor, model) && model.PackLibrary.Entries.Contains(library.Entry)) return DragDropEffects.Copy;
        if (e.Data.GetData(typeof(SlotDrag)) is SlotDrag source && ReferenceEquals(source.Editor, model) && source.Index != slot.Index && model.Slots[source.Index].HasEntry) return DragDropEffects.Move;
        return DragDropEffects.None;
    }
    private void SlotDragOver(object sender, DragEventArgs e)
    {
        if (sender is Border { DataContext: IrPackSlotViewModel slot } card)
        { e.Effects = DropEffect(e, slot); card.Tag = e.Effects == DragDropEffects.None ? null : "Drop"; }
        e.Handled = true;
    }
    private void SlotDragLeave(object sender, DragEventArgs e) { if (sender is Border card) card.Tag = null; e.Handled = true; }
    private void SlotDrop(object sender, DragEventArgs e)
    {
        if (sender is Border { DataContext: IrPackSlotViewModel slot } card && DataContext is IrPackEditorViewModel model)
        {
            card.Tag = null; e.Effects = DropEffect(e, slot);
            try
            {
                if (e.Effects == DragDropEffects.Copy && e.Data.GetData(typeof(LibraryDrag)) is LibraryDrag library) model.AssignIr(slot.Index, library.Entry);
                else if (e.Effects == DragDropEffects.Move && e.Data.GetData(typeof(SlotDrag)) is SlotDrag source) model.SwapSlots(source.Index, slot.Index);
                if (e.Effects != DragDropEffects.None) card.Focus();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or InvalidOperationException) { model.ReportError(ex.Message); }
        }
        e.Handled = true;
    }
    private void SlotKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && sender is Border { DataContext: IrPackSlotViewModel slot } && slot.Remove.CanExecute(null))
        { slot.Remove.Execute(null); e.Handled = true; }
    }
    private async void PackSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || selecting || DataContext is not IrPackEditorViewModel model) return;
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
