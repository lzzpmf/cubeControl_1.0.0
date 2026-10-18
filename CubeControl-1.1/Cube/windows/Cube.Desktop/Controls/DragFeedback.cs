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
    private static long lastScroll;
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
        Clear(); layer = AdornerLayer.GetAdornerLayer(target);
        if (layer == null) return;
        current = new(target, text, pointer, valid, insertAfter) { IsHitTestVisible = false }; layer.Add(current);
    }
    public static void Clear()
    {
        if (current != null) layer?.Remove(current);
        current = null; layer = null;
        scrollTimer.Stop(); scrolling = null;
    }
    public static void AutoScroll(DependencyObject source, DragEventArgs e)
    {
        long now = Environment.TickCount64;
        if (now - lastScroll < 50) return;
        for (DependencyObject? node = source; node != null; node = node is Visual or System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
        {
            if (node is not ScrollViewer viewer || viewer.ScrollableHeight <= 0) continue;
            double y = e.GetPosition(viewer).Y;
            if (y < 28) scrollStep = -16;
            else if (y > viewer.ActualHeight - 28) scrollStep = 16;
            else continue;
            scrolling = viewer; scrollTimer.Start(); viewer.ScrollToVerticalOffset(viewer.VerticalOffset + scrollStep);
            lastScroll = now; break;
        }
    }
    private sealed class FeedbackAdorner(UIElement target, string text, Point pointer, bool valid, bool? after) : Adorner(target)
    {
        protected override void OnRender(DrawingContext dc)
        {
            IsHitTestVisible = false;
            var accent = ThemeService.Brush(valid ? "CubeAccent" : "CubeDisabledText");
            var bounds = new Rect(AdornedElement.RenderSize);
            if (bounds.Width < 4 || bounds.Height < 4) return;
            if (after.HasValue)
            {
                double y = after.Value ? bounds.Height - 1 : 1;
                dc.DrawLine(new Pen(accent, 3), new(0, y), new(bounds.Width, y));
                dc.DrawEllipse(accent, null, new(3, y), 3, 3);
            }
            else
            {
                dc.PushOpacity(.12); dc.DrawRoundedRectangle(accent, null, bounds, 8, 8); dc.Pop();

            }
            var label = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), 12, ThemeService.Brush("CubeText"), VisualTreeHelper.GetDpi(this).PixelsPerDip)
                { MaxTextWidth = Math.Max(20, Math.Min(360, bounds.Width - 20)), Trimming = TextTrimming.CharacterEllipsis };
            double x = Math.Clamp(pointer.X + 12, 0, Math.Max(0, bounds.Width - label.Width - 16));
            double top = Math.Clamp(pointer.Y + 16, 0, Math.Max(0, bounds.Height - label.Height - 12));
            dc.DrawRoundedRectangle(ThemeService.Brush("CubeFloatingSurface"), null, new Rect(x, top, label.Width + 16, label.Height + 12), 6, 6);
            dc.DrawText(label, new(x + 8, top + 6));
        }
    }
}
