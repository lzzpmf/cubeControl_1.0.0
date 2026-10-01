using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Cube.Desktop.Appearance;

internal static class PopupRendering
{
    // A shadow on the content border rasterizes its text at fractional UI scales.
    // Retain the native menu template and put only the empty background in that effect.
    internal static void SeparateShadow(Border surface)
    {
        surface.UseLayoutRounding = true;
        surface.SnapsToDevicePixels = true;
        TextOptions.SetTextFormattingMode(surface, TextFormattingMode.Display);
        RenderOptions.SetClearTypeHint(surface, ClearTypeHint.Enabled);
        if (surface.Effect is not DropShadowEffect effect) return;
        Grid parent;
        if (VisualTreeHelper.GetParent(surface) is Grid grid) parent = grid;
        else if (VisualTreeHelper.GetParent(surface) is Decorator decorator)
        {
            parent = new Grid { UseLayoutRounding = true, SnapsToDevicePixels = true };
            decorator.Child = null;
            parent.Children.Add(surface);
            decorator.Child = parent;
        }
        else return;
        var shadow = new Border { IsHitTestVisible = false, Effect = effect.Clone(), Name = "PopupShadowLayer" };
        foreach (var (property, path) in new[]
        {
            (Border.BackgroundProperty, nameof(Border.Background)),
            (Border.CornerRadiusProperty, nameof(Border.CornerRadius)),
            (FrameworkElement.WidthProperty, nameof(FrameworkElement.ActualWidth)),
            (FrameworkElement.HeightProperty, nameof(FrameworkElement.ActualHeight)),
            (FrameworkElement.MarginProperty, nameof(FrameworkElement.Margin)),
            (FrameworkElement.HorizontalAlignmentProperty, nameof(FrameworkElement.HorizontalAlignment)),
            (FrameworkElement.VerticalAlignmentProperty, nameof(FrameworkElement.VerticalAlignment)),
            (UIElement.RenderTransformProperty, nameof(UIElement.RenderTransform)),
            (UIElement.OpacityProperty, nameof(UIElement.Opacity))
        }) shadow.SetBinding(property, new Binding(path) { Source = surface });
        Grid.SetRow(shadow, Grid.GetRow(surface)); Grid.SetColumn(shadow, Grid.GetColumn(surface));
        surface.Effect = null;
        parent.Children.Insert(parent.Children.IndexOf(surface), shadow);
        if (ThemeService.Instance.IsHighContrast) shadow.Visibility = Visibility.Collapsed;
    }
}
