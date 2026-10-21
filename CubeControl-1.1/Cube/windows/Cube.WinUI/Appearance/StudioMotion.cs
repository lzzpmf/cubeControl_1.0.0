using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;

namespace Cube.WinUI.Appearance;
internal static class StudioMotion
{
    public static void Enter(FrameworkElement element)
    {
        void Begin()
        {
            if (!new UISettings().AnimationsEnabled) return;
            var animation = new DoubleAnimation { From = .35, To = 1, Duration = new Duration(TimeSpan.FromMilliseconds(160)) };
            var storyboard = new Storyboard(); Storyboard.SetTarget(animation, element); Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation); storyboard.Begin();
        }
        if (element.IsLoaded) Begin();
        else
        {
            RoutedEventHandler? loaded = null;
            loaded = (_, _) => { element.Loaded -= loaded; Begin(); };
            element.Loaded += loaded;
        }
    }
    public static void Attach(ButtonBase button)
    {
        var scale = new ScaleTransform(); button.RenderTransform = scale; button.RenderTransformOrigin = new Windows.Foundation.Point(.5, .5);
        void Animate(double value)
        {
            if (!new UISettings().AnimationsEnabled) { scale.ScaleX = scale.ScaleY = 1; return; }
            var storyboard = new Storyboard();
            foreach (string property in new[] { "ScaleX", "ScaleY" })
            {
                var animation = new DoubleAnimation { To = value, Duration = new Duration(TimeSpan.FromMilliseconds(120)), EnableDependentAnimation = true };
                Storyboard.SetTarget(animation, scale); Storyboard.SetTargetProperty(animation, property); storyboard.Children.Add(animation);
            }
            storyboard.Begin();
        }
        button.AddHandler(UIElement.PointerPressedEvent, new Microsoft.UI.Xaml.Input.PointerEventHandler((_, _) => Animate(.97)), true);
        button.AddHandler(UIElement.PointerReleasedEvent, new Microsoft.UI.Xaml.Input.PointerEventHandler((_, _) => Animate(1)), true);
        button.PointerCaptureLost += (_, _) => Animate(1); button.PointerExited += (_, _) => Animate(1);
        button.KeyDown += (_, e) => { if (e.Key is Windows.System.VirtualKey.Space or Windows.System.VirtualKey.Enter) Animate(.97); };
        button.KeyUp += (_, _) => Animate(1);
        button.LostFocus += (_, _) => Animate(1);
    }
}
