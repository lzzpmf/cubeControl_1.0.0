using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cube.Desktop.Controls;

// Animate presentation only: never delay selection, focus or device commands.
public static class SurfaceMotion
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(SurfaceMotion), new PropertyMetadata(false, Configure));
    public static void SetEnabled(DependencyObject target, bool value) => target.SetValue(EnabledProperty, value);
    public static bool GetEnabled(DependencyObject target) => (bool)target.GetValue(EnabledProperty);
    public static readonly DependencyProperty OffsetProperty = DependencyProperty.RegisterAttached(
        "Offset", typeof(double), typeof(SurfaceMotion), new PropertyMetadata(0d));
    public static void SetOffset(DependencyObject target, double value) => target.SetValue(OffsetProperty, value);
    public static double GetOffset(DependencyObject target) => (double)target.GetValue(OffsetProperty);
    public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
        "Key", typeof(object), typeof(SurfaceMotion), new PropertyMetadata(null, (d, _) =>
        { if (d is FrameworkElement element && element.GetValue(StateProperty) is State state) state.Play(); }));
    public static void SetKey(DependencyObject target, object value) => target.SetValue(KeyProperty, value);
    public static object GetKey(DependencyObject target) => target.GetValue(KeyProperty);
    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
        "State", typeof(State), typeof(SurfaceMotion));
    public static bool Allowed => SystemParameters.ClientAreaAnimation && !SystemParameters.HighContrast && (RenderCapability.Tier >> 16) > 0;

    private static void Configure(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;
        if (element.GetValue(StateProperty) is State old) old.Dispose();
        element.SetValue(StateProperty, (bool)e.NewValue ? new State(element) : null);
    }

    private sealed class State : IDisposable
    {
        private readonly FrameworkElement element;
        private readonly DispatcherTimer cleanup;
        private bool listening;
        private TranslateTransform? translation;
        private Transform? originalTransform;
        public State(FrameworkElement element)
        {
            this.element = element;
            cleanup = new DispatcherTimer(DispatcherPriority.Render, element.Dispatcher) { Interval = TimeSpan.FromMilliseconds(200) };
            cleanup.Tick += (_, _) => Stop();
            element.Loaded += Loaded;
            element.Unloaded += Unloaded;
            element.IsVisibleChanged += VisibilityChanged;
            if (element.IsLoaded) Loaded(element, new RoutedEventArgs());
        }
        private void Loaded(object sender, RoutedEventArgs e)
        {
            if (!listening) { SystemParameters.StaticPropertyChanged += SettingsChanged; listening = true; }
            Play();
        }
        private void Unloaded(object sender, RoutedEventArgs e)
        {
            if (listening) { SystemParameters.StaticPropertyChanged -= SettingsChanged; listening = false; }
            Stop();
        }
        private void VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e) { if (element.IsVisible) Play(); else Stop(); }
        private void SettingsChanged(object? sender, PropertyChangedEventArgs e)
        { if (!Allowed) element.Dispatcher.Invoke(Stop); }
        public void Play()
        {
            Stop();
            if (!element.IsLoaded || !element.IsVisible || !Allowed) return;
            // Preserve the surface opacity and let WPF schedule up to 240 fps.
            var fade = new DoubleAnimation(0, element.Opacity, TimeSpan.FromMilliseconds(167))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Timeline.SetDesiredFrameRate(fade, 240);
            element.BeginAnimation(UIElement.OpacityProperty, fade);
            if (GetOffset(element) is double offset && offset != 0)
            {
                originalTransform = element.RenderTransform;
                translation = new TranslateTransform();
                var group = new TransformGroup();
                group.Children.Add(originalTransform);
                group.Children.Add(translation);
                element.SetCurrentValue(UIElement.RenderTransformProperty, group);
                var slide = new DoubleAnimation(offset, 0, TimeSpan.FromMilliseconds(167))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
                Timeline.SetDesiredFrameRate(slide, 240);
                translation.BeginAnimation(TranslateTransform.YProperty, slide);
            }
            cleanup.Start();
        }
        private void Stop()
        {
            cleanup.Stop();
            element.BeginAnimation(UIElement.OpacityProperty, null);
            if (translation != null)
            {
                translation.BeginAnimation(TranslateTransform.YProperty, null);
                element.SetCurrentValue(UIElement.RenderTransformProperty, originalTransform);
                translation = null;
            }
        }
        public void Dispose()
        {
            Unloaded(element, new RoutedEventArgs());
            element.Loaded -= Loaded;
            element.Unloaded -= Unloaded;
            element.IsVisibleChanged -= VisibilityChanged;
        }
    }
}
