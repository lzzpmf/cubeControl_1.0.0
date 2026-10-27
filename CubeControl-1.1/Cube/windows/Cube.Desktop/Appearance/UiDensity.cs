using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cube.Desktop.Appearance;

internal static class UiDensity
{
    public static double Factor => ZoomService.Instance.Percent / 100d;
    private static bool initialized;

    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;
        // These detached popup windows do not inherit the client's transform.
        // ComboBox and submenu template Popups already inherit it from WPF;
        // deliberately leave those alone to avoid applying zoom twice.
        EventManager.RegisterClassHandler(typeof(ContextMenu), ContextMenu.OpenedEvent,
            new RoutedEventHandler((sender, _) => Scale((FrameworkElement)sender)));
        EventManager.RegisterClassHandler(typeof(ToolTip), ToolTip.OpenedEvent,
            new RoutedEventHandler((sender, _) => Scale((FrameworkElement)sender)));
        EventManager.RegisterClassHandler(typeof(Wpf.Ui.Controls.MessageBox), FrameworkElement.LoadedEvent,
            new RoutedEventHandler(ScaleMessageBox));
    }

    private static void Scale(FrameworkElement element) =>
        element.SetCurrentValue(FrameworkElement.LayoutTransformProperty, new ScaleTransform(Factor, Factor));

    private static void ScaleMessageBox(object sender, RoutedEventArgs args)
    {
        if (!ReferenceEquals(sender, args.OriginalSource)) return;
        var dialog = (Wpf.Ui.Controls.MessageBox)sender;
        // WPF UI 4.3: scale the message and action area, preserving the title bar.
        if (dialog.Template.FindName("ContentPresenter", dialog) is ContentPresenter content
            && VisualTreeHelper.GetParent(content) is Grid grid)
            foreach (FrameworkElement child in grid.Children)
                if (Grid.GetRow(child) > 0) Scale(child);
    }
}
