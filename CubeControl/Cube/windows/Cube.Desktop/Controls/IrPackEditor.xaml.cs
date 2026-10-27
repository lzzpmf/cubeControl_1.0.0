using Cube.Desktop.Localization;
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
    private Border? pressedCard;
    private Point cardPressPosition;
    public IrPackEditor() => InitializeComponent();
    internal void SetViewportHeight(double height)
    {
        Height = height;
        PackBodyScroll.VerticalScrollBarVisibility = double.IsFinite(height)
            ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
    }
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
    internal void ConfigureLibraryDrag(TreeView tree, Func<bool> enabled)
    {
        CubeDragDrop.Attach(tree, info => enabled() && DataContext is IrPackEditorViewModel { CanEdit: true } model &&
            info.SourceItem is IrLibraryNode { Entry: { } entry }
                ? (new LibraryDrag(model, entry), DragDropEffects.Copy) : (null, DragDropEffects.None));
    }
    private void SlotLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Border card || GongSolutions.Wpf.DragDrop.DragDrop.GetDragHandler(card) is CubeDragDrop) return;
        CubeDragDrop.Attach(card, _ => DataContext is IrPackEditorViewModel { CanEdit: true } model &&
            card.DataContext is IrPackSlotViewModel { HasEntry: true } slot
                ? (new SlotDrag(model, slot.Index), DragDropEffects.Move) : (null, DragDropEffects.None), SlotDragOver, SlotDrop);
    }
    private static IEnumerable<Border> Cards(DependencyObject root)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is Border { Name: "SlotCard", DataContext: IrPackSlotViewModel } card) yield return card;
            foreach (var nested in Cards(child)) yield return nested;
        }
    }
    private DragDropEffects DropEffect(CubeDropContext e, IrPackSlotViewModel slot)
    {
        if (DataContext is not IrPackEditorViewModel { CanEdit: true } model) return DragDropEffects.None;
        if (e.Data.GetData(typeof(LibraryDrag)) is LibraryDrag library && ReferenceEquals(library.Editor, model) && model.PackLibrary.Entries.Contains(library.Entry)) return DragDropEffects.Copy;
        if (e.Data.GetData(typeof(SlotDrag)) is SlotDrag source && ReferenceEquals(source.Editor, model) && source.Index != slot.Index && model.Slots[source.Index].HasEntry) return DragDropEffects.Move;
        return DragDropEffects.None;
    }
    private void SlotDragOver(object sender, CubeDropContext e)
    {
        if (sender is Border { DataContext: IrPackSlotViewModel slot } card)
        {
            e.Effects = DropEffect(e, slot);
            string message = LanguageService.T("Destino inválido");
            if (e.Effects == DragDropEffects.Copy && e.Data.GetData(typeof(LibraryDrag)) is LibraryDrag library)
                message = library.Entry.Name + " → " + LanguageService.T(slot.HasEntry ? "Substituir" : "Adicionar") + " “" + slot.Title + ": " + slot.Name + "”";
            else if (e.Effects == DragDropEffects.Move && e.Data.GetData(typeof(SlotDrag)) is SlotDrag source && DataContext is IrPackEditorViewModel model)
                message = LanguageService.T("Trocar slots") + ": " + model.Slots[source.Index].Title + " → " + model.Slots[source.Index].Name + " ↔ " + slot.Title + " → " + slot.Name;
            DragFeedback.Show(card, message, e.GetPosition(card), e.Effects != DragDropEffects.None);
            DragFeedback.AutoScroll(card, e);
        }
        e.Handled = true;
    }

    private async void SlotDrop(object sender, CubeDropContext e)
    {
        DragFeedback.Clear();
        if (sender is Border { DataContext: IrPackSlotViewModel slot } card && DataContext is IrPackEditorViewModel model)
        {
            card.Tag = null; e.Effects = DropEffect(e, slot);
            try
            {
                if (e.Effects == DragDropEffects.Copy && e.Data.GetData(typeof(LibraryDrag)) is LibraryDrag library) { if (await model.AssignIr(slot.Index, library.Entry)) DragMotion.Settle(card); }
                else if (e.Effects == DragDropEffects.Move && e.Data.GetData(typeof(SlotDrag)) is SlotDrag source) {
                    var origin = Cards(this).FirstOrDefault(x => x.DataContext is IrPackSlotViewModel item && item.Index == source.Index);
                    if (origin != null) DragMotion.Swap(this, origin, card, () => model.SwapSlots(source.Index, slot.Index));
                }
                if (e.Effects != DragDropEffects.None) card.Focus();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or InvalidOperationException) { model.ReportError(ex.Message); }
        }
        e.Handled = true;
    }
    private void SlotMouseDown(object sender, MouseButtonEventArgs e)
    {
        pressedCard = null;
        if (sender is not Border card || e.ClickCount != 1 ||
            CubeDragDrop.IsInteractive(e.OriginalSource as DependencyObject, card)) return;
        pressedCard = card;
        cardPressPosition = e.GetPosition(card);
    }
    private void SlotMouseMove(object sender, MouseEventArgs e)
    {
        if (pressedCard is not { } card) return;
        var delta = e.GetPosition(card) - cardPressPosition;
        if (e.LeftButton != MouseButtonState.Pressed ||
            Math.Abs(delta.X) >= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(delta.Y) >= SystemParameters.MinimumVerticalDragDistance) pressedCard = null;
    }
    private void SlotMouseUp(object sender, MouseButtonEventArgs e)
    {
        var card = pressedCard;
        pressedCard = null;
        if (card == null || !ReferenceEquals(sender, card) ||
            CubeDragDrop.IsInteractive(e.OriginalSource as DependencyObject, card)) return;
        if (card.DataContext is IrPackSlotViewModel slot && slot.Preview.CanExecute(null))
        {
            card.Focus();
            slot.Preview.Execute(null);
            e.Handled = true;
        }
    }
    private void SlotKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not Border { DataContext: IrPackSlotViewModel slot } card ||
            CubeDragDrop.IsInteractive(e.OriginalSource as DependencyObject, card)) return;
        if (e.Key == Key.Delete && slot.Remove.CanExecute(null))
        { slot.Remove.Execute(null); e.Handled = true; }
        else if ((e.Key is Key.Enter or Key.Space) && !e.IsRepeat && slot.Preview.CanExecute(null))
        { slot.Preview.Execute(null); e.Handled = true; }
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
