using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Cube.Desktop.Appearance;

namespace Cube.Desktop.Controls;

// A separate visual layer: never overwrite a selected item's border or bindings.
internal static class DragFeedback
{
    private static FeedbackAdorner? current;
    private static AdornerLayer? layer;
    private static ScrollViewer? scrolling;
    private static double scrollStep;
    private static readonly System.Windows.Threading.DispatcherTimer scrollTimer = CreateScrollTimer();
    internal static bool IsVisible => current != null;
    private static System.Windows.Threading.DispatcherTimer CreateScrollTimer()
    {
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        timer.Tick += (_, _) =>
        {
            if (scrolling?.IsLoaded != true) { timer.Stop(); scrolling = null; return; }
            scrolling.ScrollToVerticalOffset(scrolling.VerticalOffset + scrollStep);
        };
        return timer;
    }
    public static void Show(UIElement target, string text, Point pointer, bool valid, bool? insertAfter = null)
    {
        if (current != null) layer?.Remove(current);
        current = null; layer = AdornerLayer.GetAdornerLayer(target);
        if (layer == null) return;
        current = new(target, text, pointer, valid, insertAfter) { IsHitTestVisible = false }; layer.Add(current);
    }
    public static void Clear()
    {
        if (current != null) layer?.Remove(current);
        current = null; layer = null;
        scrollTimer.Stop(); scrolling = null;
    }
    public static void AutoScroll(DependencyObject source, CubeDropContext e)
    {
        for (DependencyObject? node = source; node != null; node = node is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
        {
            if (node is not ScrollViewer viewer || viewer.ScrollableHeight <= 0) continue;
            var point = e.GetPosition(viewer);
            if (point.X < 0 || point.X > viewer.ActualWidth || point.Y < 0 || point.Y > viewer.ActualHeight) continue;
            double y = point.Y;
            if (y < 28) scrollStep = -16;
            else if (y > viewer.ActualHeight - 28) scrollStep = 16;
            else continue;
            scrolling = viewer; scrollTimer.Start(); return;
        }
        scrollTimer.Stop(); scrolling = null;
    }
    private sealed class FeedbackAdorner(UIElement target, string text, Point pointer, bool valid, bool? after) : Adorner(target)
    {
        protected override void OnRender(DrawingContext dc)
        {
            IsHitTestVisible = false;
            var accent = ThemeService.Brush(valid ? "CubeAccent" : "CubeDisabledText");
            var bounds = new Rect(AdornedElement.RenderSize);
            double radius = ((CornerRadius)FindResource("CubeCardCornerRadius")).TopLeft;
            if (bounds.Width < 4 || bounds.Height < 4) return;
            if (after.HasValue)
            {
                double y = after.Value ? bounds.Height - 1 : 1;
                dc.DrawLine(new Pen(accent, 3), new(0, y), new(bounds.Width, y));
                dc.DrawEllipse(accent, null, new(3, y), 3, 3);
            }
            else
            {
                if (SystemParameters.HighContrast) dc.DrawRoundedRectangle(null, new Pen(accent, 2), bounds, radius, radius);
                else { dc.PushOpacity(.12); dc.DrawRoundedRectangle(accent, null, bounds, radius, radius); dc.Pop(); }

            }
            var label = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface((FontFamily)FindResource("CubeUiFont"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), (double)FindResource("CubeFontSizeCaption"), ThemeService.Brush("CubeText"), VisualTreeHelper.GetDpi(this).PixelsPerDip)
                { MaxTextWidth = Math.Max(20, Math.Min(360, bounds.Width - 20)), Trimming = TextTrimming.CharacterEllipsis };
            double x = Math.Clamp(pointer.X + 12, 0, Math.Max(0, bounds.Width - label.Width - 16));
            double top = Math.Clamp(pointer.Y + 16, 0, Math.Max(0, bounds.Height - label.Height - 12));
            dc.DrawRoundedRectangle(ThemeService.Brush("CubeFloatingSurface"), null, new Rect(x, top, label.Width + 16, label.Height + 12), radius, radius);
            dc.DrawText(label, new(x + 8, top + 6));
        }
    }
}
