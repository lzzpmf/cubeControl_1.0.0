using System.Windows;
using System.Windows.Automation.Peers;

namespace Cube.Desktop.Controls;

// The same art is decorative on effects without a picker and an action on amp/cabinet.
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
    private void UpdateZoom()
    {
        if (Content is not GearArt art) return;
        art.RenderTransformOrigin = new Point(.5, .5);
        if (art.RenderTransform is not System.Windows.Media.ScaleTransform scale)
            art.RenderTransform = scale = new System.Windows.Media.ScaleTransform(1, 1);
        double target = IsInteractive && IsEnabled && (IsMouseOver || IsKeyboardFocusWithin) && SurfaceMotion.Allowed ? 1.035 : 1;
        foreach (var property in new[] { System.Windows.Media.ScaleTransform.ScaleXProperty, System.Windows.Media.ScaleTransform.ScaleYProperty })
        {
            if (!SurfaceMotion.Allowed) { scale.BeginAnimation(property, null); scale.SetValue(property, target); }
            else scale.BeginAnimation(property, new System.Windows.Media.Animation.DoubleAnimation(target, TimeSpan.FromMilliseconds(160))
            { EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut } });
        }
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new ImageButtonPeer(this);
    private sealed class ImageButtonPeer(GearImageButton owner) : ButtonAutomationPeer(owner)
    {
        protected override bool IsControlElementCore() => owner.IsInteractive && base.IsControlElementCore();
        protected override bool IsContentElementCore() => owner.IsInteractive && base.IsContentElementCore();
    }
}
