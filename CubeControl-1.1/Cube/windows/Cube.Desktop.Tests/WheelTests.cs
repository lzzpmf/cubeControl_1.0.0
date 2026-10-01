using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Controls;

internal static class WheelTests
{
    public static async Task Run()
    {
        var strip = new StackPanel { Orientation = Orientation.Horizontal };
        for (int i = 0; i < 8; i++) strip.Children.Add(new Border { Width = 240, Height = 100 });
        var viewer = new ScrollViewer { Content = strip, CanContentScroll = false, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled };
        SmoothHorizontalWheel.SetEnabled(viewer, true);
        var window = new Window { Content = viewer, Width = 600, Height = 180, ShowInTaskbar = false };
        try
        {
            window.Show(); window.UpdateLayout();
            void Wheel(int delta) => viewer.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, delta) { RoutedEvent = UIElement.PreviewMouseWheelEvent });
            Wheel(-120); await Task.Delay(600);
            if (viewer.HorizontalOffset < 175 || viewer.HorizontalOffset > 181) throw new Exception("Wheel must scroll horizontally in pixels.");
            Wheel(-30); Wheel(-30); await Task.Delay(600);
            if (viewer.HorizontalOffset < 265) throw new Exception("Fractional wheel input must accumulate.");
            Wheel(12000); await Task.Delay(900);
            if (viewer.HorizontalOffset > .5) throw new Exception("Scrolling must clamp to the start.");
            Wheel(-12000); await Task.Delay(900);
            if (Math.Abs(viewer.HorizontalOffset - viewer.ScrollableWidth) > .5) throw new Exception("Scrolling must clamp to the end.");
            Console.WriteLine("PASS smooth horizontal wheel, fractional input and both boundaries");
        }
        finally { SmoothHorizontalWheel.SetEnabled(viewer, false); window.Close(); }
    }
}
