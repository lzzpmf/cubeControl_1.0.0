using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cube.Desktop.Controls;

// IsInteractive controls carousel feedback; every image offers customization.
public sealed class GearImageButton : Wpf.Ui.Controls.Button
{
    public static readonly DependencyProperty IsInteractiveProperty = DependencyProperty.Register(
        nameof(IsInteractive), typeof(bool), typeof(GearImageButton), new PropertyMetadata(true));
    public bool IsInteractive { get => (bool)GetValue(IsInteractiveProperty); set => SetValue(IsInteractiveProperty, value); }
    public GearImageButton()
    {
        MouseEnter += (_, _) => UpdateZoom();
        MouseLeave += (_, _) => UpdateZoom();
        IsKeyboardFocusWithinChanged += (_, _) => UpdateZoom();
        IsEnabledChanged += (_, _) => UpdateZoom();
        Loaded += (_, _) => UpdateZoom();
    }
    public void AnimateScrollChevron(int direction)
    {
        if (!IsInteractive || !IsEnabled || !SurfaceMotion.Allowed || direction == 0) return;
        foreach (var (name, sign) in new[] { ("CarouselUpChevron", -1), ("CarouselDownChevron", 1) })
        {
            if (Template?.FindName(name, this) is not FrameworkElement chevron) continue;
            if (chevron.RenderTransform is not TranslateTransform translation)
                chevron.RenderTransform = translation = new TranslateTransform();
            double current = translation.Y;
            translation.BeginAnimation(TranslateTransform.YProperty, null);
            translation.Y = 0;
            if (sign != Math.Sign(direction)) continue;
            var pulse = new DoubleAnimationUsingKeyFrames { FillBehavior = FillBehavior.Stop };
            pulse.KeyFrames.Add(new LinearDoubleKeyFrame(current, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            pulse.KeyFrames.Add(new EasingDoubleKeyFrame(sign * 5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80)),
                new CubicEase { EasingMode = EasingMode.EaseOut }));
            pulse.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(240)),
                new CubicEase { EasingMode = EasingMode.EaseInOut }));
            translation.BeginAnimation(TranslateTransform.YProperty, pulse);
        }
    }
    private void UpdateZoom()
    {
        if (Content is not GearArt art) return;
        art.RenderTransformOrigin = new Point(.5, .5);
        if (art.RenderTransform is not System.Windows.Media.ScaleTransform scale)
            art.RenderTransform = scale = new System.Windows.Media.ScaleTransform(1, 1);
        double target = IsInteractive && IsEnabled && (IsMouseOver || IsKeyboardFocusWithin) && SurfaceMotion.Allowed ? 1.19025 : 1;
        foreach (var property in new[] { System.Windows.Media.ScaleTransform.ScaleXProperty, System.Windows.Media.ScaleTransform.ScaleYProperty })
        {
            if (!SurfaceMotion.Allowed) { scale.BeginAnimation(property, null); scale.SetValue(property, target); }
            else scale.BeginAnimation(property, new System.Windows.Media.Animation.DoubleAnimation(target, TimeSpan.FromMilliseconds(160 / 0.85))
            { EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut } });
        }
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new ImageButtonPeer(this);
    private sealed class ImageButtonPeer(GearImageButton owner) : ButtonAutomationPeer(owner)
    {
        protected override bool IsControlElementCore() => base.IsControlElementCore();
        protected override bool IsContentElementCore() => base.IsContentElementCore();
    }
}
