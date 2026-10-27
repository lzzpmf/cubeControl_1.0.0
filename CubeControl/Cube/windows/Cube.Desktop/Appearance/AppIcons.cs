using System.Windows;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Appearance;

internal static class AppIcons
{
    internal static SymbolIcon Create(SymbolRegular symbol, bool navigation = false)
    {
        var icon = new SymbolIcon();
        icon.SetResourceReference(SymbolIcon.SymbolProperty, "CubeIcon" + symbol);
        icon.SetResourceReference(FrameworkElement.StyleProperty, navigation ? "CubeNavigationIcon" : "CubeActionIcon");
        return icon;
    }
}
