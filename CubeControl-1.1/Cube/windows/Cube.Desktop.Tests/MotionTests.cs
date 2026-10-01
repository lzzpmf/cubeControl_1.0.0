using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cube.Desktop.Controls;

internal static class MotionTests
{
    public static async Task Run()
    {
        var button = new Button { Content = "Focus stays here" };
        var transform = new ScaleTransform(.9, .9);
        var surface = new Border { Child = button, RenderTransform = transform, Opacity = .8 };
        SurfaceMotion.SetOffset(surface, 8);
        SurfaceMotion.SetEnabled(surface, true);
        var window = new Window { Content = surface, Width = 400, Height = 240, ShowInTaskbar = false };
        window.Show();
        button.Focus();
        var size = surface.RenderSize;
        for (int i = 0; i < 12; i++)
        {
            SurfaceMotion.SetKey(surface, i);
            await Task.Delay(15);
            if (!button.IsKeyboardFocused || surface.RenderSize != size)
                throw new Exception("Motion changed focus or layout.");
        }
        await Task.Delay(300);
        if (surface.HasAnimatedProperties || surface.Opacity != .8 || surface.RenderTransform != transform)
            throw new Exception("Motion did not release clocks and restore base values.");
        SurfaceMotion.SetKey(surface, 99);
        surface.Visibility = Visibility.Collapsed;
        if (surface.HasAnimatedProperties || surface.RenderTransform != transform)
            throw new Exception("Hidden surface retained an animation.");
        surface.Visibility = Visibility.Visible;
        SurfaceMotion.SetEnabled(surface, false);
        if (surface.HasAnimatedProperties || surface.RenderTransform != transform)
            throw new Exception("Disabled motion retained an animation.");
        window.Close();
        // Equal elapsed time must produce equal motion independently of sample frequency.
        foreach (int hz in new[] { 60, 120, 144, 240 })
        {
            double previous = 0;
            for (int frame = 0; frame <= hz; frame++)
            {
                double value = SmoothScroll.Ease(frame / (double)hz);
                if (value < previous || value > 1) throw new Exception("Nonmonotonic scroll at " + hz);
                previous = value;
            }
            if (previous != 1 || SmoothScroll.Ease(.5) != .875) throw new Exception("Frame-rate dependent scroll");
        }
        var scroll = new ScrollViewer { Content = new Border { Height = 3000 }, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var scrollWindow = new Window { Content = scroll, Width = 400, Height = 300, ShowInTaskbar = false };
        scrollWindow.Show(); await Task.Delay(100); scrollWindow.UpdateLayout();
        SmoothScroll.Wheel(scroll, -120);
        double firstTarget = SmoothScroll.Target(scroll);
        SmoothScroll.Wheel(scroll, -120);
        double target = SmoothScroll.Target(scroll);
        if (target <= firstTarget) throw new Exception("Repeated wheel input lost distance");
        await Task.Delay(300); scrollWindow.UpdateLayout();
        if (Math.Abs(scroll.VerticalOffset - target) > 1 || SmoothScroll.IsRunning(scroll)) throw new Exception("Scroll did not settle/release rendering subscription");
        SmoothScroll.Wheel(scroll, 120); await Task.Delay(300); scrollWindow.UpdateLayout();
        if (scroll.VerticalOffset >= target) throw new Exception("Reverse scroll failed");
        SmoothScroll.Wheel(scroll, -120); scroll.ScrollToEnd(); await Task.Delay(80); scrollWindow.UpdateLayout();
        if (SmoothScroll.IsRunning(scroll) || Math.Abs(scroll.VerticalOffset - scroll.ScrollableHeight) > 1) throw new Exception("Programmatic navigation fought smooth scroll");
        if (SmoothScroll.Wheel(scroll, -120)) throw new Exception("Boundary should allow parent scrolling");
        SmoothScroll.Wheel(scroll, 120); scrollWindow.Close();
        if (SmoothScroll.IsRunning(scroll)) throw new Exception("Closed viewer retained render subscription");
        Console.WriteLine("PASS smooth scroll: time-based 60/120/144/240 samples, accumulation, reverse, boundaries, external navigation and cleanup.");
        Console.WriteLine($"PASS motion: rapid changes, focus, stable layout, cleanup, hide and disable (system motion allowed: {SurfaceMotion.Allowed}).");
    }
}
