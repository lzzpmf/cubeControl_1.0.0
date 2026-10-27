using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Controls;

public partial class IrLibraryBrowser : UserControl
{
    internal void SetViewportHeight(double height)
    {
        IrLibraryTree.MaxHeight = double.IsFinite(height) ? Math.Max(320, height - 180) : 420;
    }
    public IrLibraryBrowser()
    {
        InitializeComponent();
        Tree.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(FolderExpanded));
    }

    private void FolderExpanded(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not TreeViewItem item) return;
        // Wait for the expanded rows (and any collapsed sibling brand) to lay out.
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            if (!item.IsLoaded || !item.IsExpanded || item.Items.Count == 0) return;
            item.UpdateLayout();
            if (item.ItemContainerGenerator.ContainerFromIndex(0) is not TreeViewItem child) return;
            child.ApplyTemplate();
            // Reveal a usable child row, not the whole potentially huge subtree.
            var header = child.Template.FindName("PART_Header", child) as FrameworkElement ?? child;
            header.BringIntoView();
        }));
    }
    public TreeView Tree => IrLibraryTree;
    public FrameworkElement SearchBox => IrLibrarySearch;
    private void FolderClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: IrLibraryNode { IsFolder: true, IsBrand: false } node })
        { node.IsSelected = true; node.IsExpanded = !node.IsExpanded; e.Handled = true; }
    }
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);
        TreeViewItem? item = null; bool chevron = false;
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null;)
        {
            if (ReferenceEquals(node, IrCaptureDetails)) return;
            if (ReferenceEquals(node, Tree))
            {
                if (e.ChangedButton == MouseButton.Left && item != null)
                {
                    if (item.DataContext is IrLibraryNode { IsBrand: true })
                    {
                        item.Focus(); item.SetCurrentValue(TreeViewItem.IsSelectedProperty, true);
                        item.SetCurrentValue(TreeViewItem.IsExpandedProperty, !item.IsExpanded); e.Handled = true;
                    }
                    else if (chevron) item.SetCurrentValue(TreeViewItem.IsSelectedProperty, true);
                }
                return;
            }
            if (node is System.Windows.Controls.Primitives.ToggleButton) chevron = true;
            if (node is TreeViewItem found) item ??= found;
            node = node is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
        }
        if (DataContext is IrLibraryViewModel library) library.ClearSelection();
    }
}
