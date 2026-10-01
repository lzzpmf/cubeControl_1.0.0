using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cube.Desktop.Controls;

public static class SmoothHorizontalWheel
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(SmoothHorizontalWheel), new PropertyMetadata(false, Changed));
    public static bool GetEnabled(DependencyObject obj) => (bool)obj.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject obj, bool value) => obj.SetValue(EnabledProperty, value);
    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached("State", typeof(Motion), typeof(SmoothHorizontalWheel));
    private static void Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not FrameworkElement element) return;
        (element.GetValue(StateProperty) as Motion)?.Dispose();
        element.SetValue(StateProperty, (bool)e.NewValue ? new Motion(element) : null);
    }
    private sealed class Motion : IDisposable
    {
        private readonly FrameworkElement element;
        private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
        private readonly Stopwatch clock = new();
        private ScrollViewer? viewer;
        private double target;
        public Motion(FrameworkElement element)
        {
            this.element = element;
            element.PreviewMouseWheel += Wheel;
            element.Unloaded += Unloaded;
            element.PreviewMouseDown += Stop;
            element.PreviewKeyDown += Stop;
            timer.Tick += Tick;
        }
        private static ScrollViewer? Find(DependencyObject root)
        {
            if (root is ScrollViewer scroll) return scroll;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                if (Find(VisualTreeHelper.GetChild(root, i)) is { } result) return result;
            return null;
        }
        private void Wheel(object sender, MouseWheelEventArgs e)
        {
            viewer ??= Find(element);
            if (viewer == null || viewer.ScrollableWidth <= 0) return;
            if (!timer.IsEnabled) target = viewer.HorizontalOffset;
            target = Math.Clamp(target - e.Delta * 1.5, 0, viewer.ScrollableWidth);
            e.Handled = true;
            if (!SystemParameters.ClientAreaAnimation) { viewer.ScrollToHorizontalOffset(target); return; }
            clock.Restart(); timer.Start();
        }
        private void Tick(object? sender, EventArgs e)
        {
            if (viewer == null) { timer.Stop(); return; }
            target = Math.Clamp(target, 0, viewer.ScrollableWidth);
            double elapsed = clock.Elapsed.TotalSeconds; clock.Restart();
            double delta = target - viewer.HorizontalOffset;
            if (Math.Abs(delta) < .5) { viewer.ScrollToHorizontalOffset(target); timer.Stop(); }
            else viewer.ScrollToHorizontalOffset(viewer.HorizontalOffset + delta * (1 - Math.Exp(-18 * elapsed)));
        }
        private void Stop(object? sender, RoutedEventArgs e) => timer.Stop();
        private void Unloaded(object sender, RoutedEventArgs e) { timer.Stop(); viewer = null; }
        public void Dispose()
        {
            timer.Stop(); timer.Tick -= Tick;
            element.PreviewMouseWheel -= Wheel; element.Unloaded -= Unloaded;
            element.PreviewMouseDown -= Stop; element.PreviewKeyDown -= Stop;
        }
    }
}
