using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cube.Desktop.Controls;

// Original vector illustrations: placeholders, not photographs of the referenced products.
public sealed class GearArt : FrameworkElement
{
    public GearArt()
    {
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        Unloaded += (_, _) => StopScroll();
        IsVisibleChanged += (_, _) => { if (!IsVisible) StopScroll(); };
        IsEnabledChanged += (_, _) => { if (!IsEnabled) StopScroll(); };
    }
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(string), typeof(GearArt), new FrameworkPropertyMetadata("amp", FrameworkPropertyMetadataOptions.AffectsRender, (d, _) => ((GearArt)d).StopScroll()));
    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(nameof(Accent), typeof(Brush), typeof(GearArt), new FrameworkPropertyMetadata(Brushes.Tan, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource), typeof(GearArt), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ArtworkScaleProperty = DependencyProperty.Register(nameof(ArtworkScale), typeof(double), typeof(GearArt),
        new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender),
        value => value is double scale && double.IsFinite(scale) && scale > 0 && scale <= 1);
    public string Kind { get => (string)GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public Brush Accent { get => (Brush)GetValue(AccentProperty); set => SetValue(AccentProperty, value); }
    public ImageSource? Image { get => (ImageSource?)GetValue(ImageProperty); set => SetValue(ImageProperty, value); }
    public double ArtworkScale { get => (double)GetValue(ArtworkScaleProperty); set => SetValue(ArtworkScaleProperty, value); }
    private static readonly DependencyProperty ScrollProgressProperty = DependencyProperty.Register("ScrollProgress", typeof(double), typeof(GearArt), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender));
    private DrawingGroup? outgoing;
    private int scrollDirection, scrollRevision;
    internal bool IsScrolling => outgoing != null;
    internal int ScrollDirection => scrollDirection;
    internal void AnimateScroll(int direction)
    {
        StopScroll();
        if (!IsLoaded || !IsVisible || !IsEnabled || Kind is not ("amp" or "cab") || !SurfaceMotion.Allowed || Appearance.ThemeService.Instance.IsHighContrast) return;
        outgoing = new DrawingGroup();
        using (var drawing = outgoing.Open()) DrawArtwork(drawing);
        outgoing.Freeze();
        scrollDirection = Math.Sign(direction);
        int revision = scrollRevision;
        var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(167))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }, FillBehavior = FillBehavior.Stop };
        Timeline.SetDesiredFrameRate(animation, 240);
        animation.Completed += (_, _) => { if (revision == scrollRevision) StopScroll(); };
        BeginAnimation(ScrollProgressProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }
    private void StopScroll()
    {
        scrollRevision++;
        BeginAnimation(ScrollProgressProperty, null);
        outgoing = null;
        InvalidateVisual();
    }
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (outgoing == null || !SurfaceMotion.Allowed || Appearance.ThemeService.Instance.IsHighContrast)
        { DrawArtwork(dc); return; }
        double progress = (double)GetValue(ScrollProgressProperty);
        double distance = 8;
        dc.PushClip(new RectangleGeometry(new Rect(RenderSize)));
        dc.PushOpacity(progress);
        dc.PushTransform(new TranslateTransform(0, distance * (1 - progress)));
        DrawArtwork(dc);
        dc.Pop(); dc.Pop(); dc.Pop();
    }
    private void DrawArtwork(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        if (Image != null)
        {
            if (Kind is "amp" or "cab" or "chorus" or "phaser" or "delay" or "reverb")
            {
                var rect = GearArtworkLayout.Place(new Size(Image.Width, Image.Height), GearArtworkLayout.VisibleBounds(Image), RenderSize, ArtworkScale);
                if (!rect.IsEmpty)
                {
                    dc.PushClip(new RectangleGeometry(new Rect(RenderSize)));
                    dc.DrawImage(Image, rect); dc.Pop();
                }
                return;
            }
            double scale = Math.Min(ActualWidth / Image.Width, ActualHeight / Image.Height) * ArtworkScale;
            dc.DrawImage(Image, new((ActualWidth - Image.Width * scale) / 2, (ActualHeight - Image.Height * scale) / 2, Image.Width * scale, Image.Height * scale)); return;
        }
        double factor = Math.Min(ActualWidth / 340, ActualHeight / 230) * ArtworkScale;
        var placeholder = new Rect((ActualWidth - 340 * factor) / 2, (ActualHeight - 230 * factor) / 2, 340 * factor, 230 * factor);
        if (Kind is "amp" or "cab" or "chorus" or "phaser" or "delay" or "reverb")
        {
            var visible = Kind is "amp" or "cab" ? new Rect(23, 20, 294, 200) : new Rect(30, 14, 280, 206);
            placeholder = GearArtworkLayout.Place(new Size(340, 230), visible, RenderSize, ArtworkScale);
            if (placeholder.IsEmpty) return;
            factor = placeholder.Width / 340;
        }
        dc.PushTransform(new TranslateTransform(placeholder.X, placeholder.Y)); dc.PushTransform(new ScaleTransform(factor, factor));
        Brush dark = Brush("#15191E"), grille = Brush("#292E31"), metal = Brush("#6E747B");
        dc.DrawEllipse(Brush("#0D1114"), null, new(170, 210), 140, 10);
        if (Kind is "amp" or "cab")
        {
            dc.DrawRoundedRectangle(dark, new Pen(Brush("#555650"), 2), new(24, 40, 292, 160), 8, 8);
            dc.DrawRoundedRectangle(null, new Pen(metal, 6), new(130, 23, 80, 16), 5, 5);
            dc.DrawRoundedRectangle(grille, new Pen(Brush("#101419"), 2), new(37, Kind == "amp" ? 89 : 53, 266, Kind == "amp" ? 96 : 132), 3, 3);
            if (Kind == "cab") foreach (double x in new[] { 103d, 236d })
            { dc.DrawEllipse(dark, new Pen(Brush("#424A4C"), 2), new(x, 119), 53, 53); dc.DrawEllipse(grille, new Pen(metal, 1), new(x, 119), 19, 19); }
            for (int y = Kind == "amp" ? 94 : 57; y < 183; y += 5) dc.DrawLine(new Pen(Brush("#3E4543"), .65), new(40, y), new(300, y));
            if (Kind == "amp")
            {
                dc.DrawRectangle(Accent, null, new(37, 51, 266, 28));
                for (int x = 62; x <= 239; x += 35) { dc.DrawEllipse(dark, new Pen(metal, 1), new(x, 65), 7, 7); dc.DrawLine(new Pen(Brushes.WhiteSmoke, 1.4), new(x, 65), new(x + 2, 60)); }
                dc.DrawEllipse(Brush("#B5D9A0"), null, new(282, 65), 3, 3);
            }
            dc.DrawRoundedRectangle(Accent, null, new(140, 111, 60, 21), 2, 2);
            for (int x = 150; x <= 189; x += 9) dc.DrawLine(new Pen(dark, 3), new(x, 117), new(x, 125));
            dc.DrawRectangle(metal, null, new(40, 200, 18, 6)); dc.DrawRectangle(metal, null, new(280, 200, 18, 6));
        }
        else
        {
            dc.DrawRoundedRectangle(dark, new Pen(Accent, 2), new(90, 15, 160, 185), 12, 12);
            dc.DrawRoundedRectangle(Accent, null, new(100, 24, 140, 94), 6, 6);
            foreach (double x in new[] { 128d, 210d }) { dc.DrawEllipse(dark, new Pen(metal, 2), new(x, 47), 13, 13); dc.DrawLine(new Pen(Brushes.WhiteSmoke, 2), new(x, 47), new(x + 2, 36)); }
            var wave = new StreamGeometry(); using (var g = wave.Open())
            {
                g.BeginFigure(new(113, 86), false, false);
                for (int x = 114; x < 228; x++) g.LineTo(new(x, 86 + Math.Sin((x - 113) / (Kind == "phaser" ? 10d : 17d)) * (Kind == "reverb" ? 14 * Math.Exp(-(x - 113) / 65d) : 12)), true, false);
            }
            dc.DrawGeometry(null, new Pen(dark, 2), wave);
            dc.DrawEllipse(Accent, null, new(170, 137), 3, 3);
            dc.DrawEllipse(metal, new Pen(Brushes.Silver, 2), new(170, 169), 13, 13);
            dc.DrawEllipse(Brush("#B5BAC0"), null, new(170, 168), 8, 8);
        }
        dc.Pop(); dc.Pop();
    }
    private static Brush Brush(string hex) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
}
