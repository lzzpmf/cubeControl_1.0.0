using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Controls;

public partial class IrLibraryBrowser : UserControl
{
    public IrLibraryBrowser() => InitializeComponent();
    public TreeView Tree => IrLibraryTree;
    public FrameworkElement SearchBox => IrLibrarySearch;
    private void SearchLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Wpf.Ui.Controls.AutoSuggestBox box) return;
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
        {
            if (box.Template?.FindName("PART_TextBox", box) is Wpf.Ui.Controls.TextBox search)
            {
                search.IconPlacement = Wpf.Ui.Controls.ElementPlacement.Left;
                search.SetValue(Border.CornerRadiusProperty, new CornerRadius(22));
                search.MinHeight = 40; search.Padding = new Thickness(12, 8, 12, 8);
            }
        }));
    }
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
