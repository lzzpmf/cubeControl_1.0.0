using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cube.Desktop.Studio.Controls;

/// <summary>Detached menus follow the DAW's fitted size as well as the app's zoom.</summary>
internal static class StudioPopupScale
{
    internal static void Apply(ContextMenu menu)
    {
        if (menu.PlacementTarget is not FrameworkElement anchor || Window.GetWindow(anchor) is not { } window) return;
        var transform = anchor.TransformToAncestor(window);
        var origin = transform.Transform(new Point());
        var unit = transform.Transform(new Point(1, 1));
        var x = Math.Abs(unit.X - origin.X);
        var y = Math.Abs(unit.Y - origin.Y);
        if (!double.IsFinite(x) || !double.IsFinite(y) || x <= 0 || y <= 0) return;
        menu.LayoutTransform = new ScaleTransform(x, y);
        menu.MaxHeight = Math.Max(80, (SystemParameters.WorkArea.Height - 24) / y);
        menu.MaxWidth = Math.Max(120, (SystemParameters.WorkArea.Width - 24) / x);
    }
}
