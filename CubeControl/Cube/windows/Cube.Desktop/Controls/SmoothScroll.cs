using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cube.Desktop.Controls;

// The compositor supplies ticks; elapsed time, not a 60 Hz timer, determines travel.
internal static class SmoothScroll
{
    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
        "State", typeof(State), typeof(SmoothScroll));
    internal static bool IsRunning(ScrollViewer viewer) => (viewer.GetValue(StateProperty) as State)?.Running == true;
    internal static double Target(ScrollViewer viewer) => viewer.GetValue(StateProperty) is State { Running: true } state ? state.Target : viewer.VerticalOffset;
    internal static double Ease(double progress) => 1 - Math.Pow(1 - Math.Clamp(progress, 0, 1), 3);
    public static void RouteWheel(object sender, MouseWheelEventArgs e)
    {
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null;)
        {
            if (node is Knob { IsEnabled: true } || node is ComboBox) return;
            if (node is ScrollViewer viewer && viewer.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled && Wheel(viewer, e.Delta))
            { e.Handled = true; return; }
            if (ReferenceEquals(node, sender)) break;
            node = node is Visual || node is System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
        }
    }
    public static void Stop(ScrollViewer viewer) => (viewer.GetValue(StateProperty) as State)?.Stop();
    public static bool Wheel(ScrollViewer viewer, int delta)
    {
        if (delta == 0 || SystemParameters.WheelScrollLines == 0 || viewer.ScrollableHeight <= 0) return false;
        double current = Target(viewer);
        if (delta > 0 ? current <= 0 : current >= viewer.ScrollableHeight) return false;
        double amount = SystemParameters.WheelScrollLines < 0 ? viewer.ViewportHeight : SystemParameters.WheelScrollLines * 16d;
        if (viewer.GetValue(StateProperty) is not State state)
        { state = new State(viewer); viewer.SetValue(StateProperty, state); }
        state.Move(Math.Clamp(current - delta / 120d * amount, 0, viewer.ScrollableHeight));
        return true;
    }
    private sealed class State
    {
        private readonly ScrollViewer viewer;
        private long started;
        private double origin, lastApplied;
        private TimeSpan lastFrame = TimeSpan.MinValue;
        public bool Running { get; private set; }
        public double Target { get; private set; }
        public State(ScrollViewer viewer)
        {
            this.viewer = viewer;
            viewer.Unloaded += (_, _) => Stop();
            viewer.IsVisibleChanged += (_, _) => { if (!viewer.IsVisible) Stop(); };
            viewer.PreviewKeyDown += (_, _) => Stop();
            viewer.PreviewMouseDown += (_, _) => Stop();
            viewer.PreviewTouchDown += (_, _) => Stop();
            viewer.ScrollChanged += (_, e) =>
            {
                if (e.OriginalSource != viewer || !Running) return;
                if (e.ViewportHeightChange != 0 || e.ExtentHeightChange != 0 || Math.Abs(viewer.VerticalOffset - lastApplied) > 1.5) Stop();
            };
        }
        public void Move(double target)
        {
            Target = target;
            if (!SurfaceMotion.Allowed || !viewer.IsVisible)
            { Stop(); viewer.ScrollToVerticalOffset(target); return; }
            origin = lastApplied = viewer.VerticalOffset;
            started = Stopwatch.GetTimestamp();
            if (!Running) { Running = true; lastFrame = TimeSpan.MinValue; CompositionTarget.Rendering += Render; }
        }
        private void Render(object? sender, EventArgs args)
        {
            if (args is RenderingEventArgs frame)
            { if (frame.RenderingTime == lastFrame) return; lastFrame = frame.RenderingTime; }
            if (!SurfaceMotion.Allowed) { viewer.ScrollToVerticalOffset(Target); Stop(); return; }
            double progress = Stopwatch.GetElapsedTime(started).TotalMilliseconds / 180;
            lastApplied = origin + (Target - origin) * Ease(progress);
            viewer.ScrollToVerticalOffset(lastApplied);
            if (progress >= 1) Stop();
        }
        public void Stop()
        {
            if (!Running) return;
            CompositionTarget.Rendering -= Render;
            Running = false;
        }
    }
}
