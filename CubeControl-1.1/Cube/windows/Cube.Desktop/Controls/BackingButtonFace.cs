using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.Appearance;

namespace Cube.Desktop.Controls;

// Artwork is decoded once; alpha bounds normalize the different source margins.
// The original PNGs remain untouched.
public sealed class BackingButtonFace : FrameworkElement
{
    private static readonly Dictionary<string, ImageSource> Images = [];
    public static readonly DependencyProperty ArtProperty = DependencyProperty.Register(nameof(Art), typeof(string), typeof(BackingButtonFace), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(BackingButtonFace), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(BackingButtonFace), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    public bool IsActive { get => (bool)GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    public string Art { get => (string)GetValue(ArtProperty); set => SetValue(ArtProperty, value); }
    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public BackingButtonFace()
    {
        IsHitTestVisible = false;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        Loaded += (_, _) => { SystemParameters.StaticPropertyChanged += SystemSettingsChanged; ThemeService.Instance.Changed += InvalidateVisual; InvalidateVisual(); };
        Unloaded += (_, _) => { SystemParameters.StaticPropertyChanged -= SystemSettingsChanged; ThemeService.Instance.Changed -= InvalidateVisual; };
    }
    private void SystemSettingsChanged(object? sender, PropertyChangedEventArgs e) { if (e.PropertyName == nameof(SystemParameters.HighContrast)) InvalidateVisual(); }
    private static ImageSource Load(string name)
    {
        if (Images.TryGetValue(name, out var cached)) return cached;
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri("pack://application:,,,/CubeControl;component/Assets/Images/Controls/BackingTrack/" + Uri.EscapeDataString(name) + ".png");
        bitmap.DecodePixelWidth = 256;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit(); bitmap.Freeze();
        var pixels = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
        int w = pixels.PixelWidth, h = pixels.PixelHeight, left = w, top = h, right = -1, bottom = -1;
        var buffer = new byte[w * h * 4]; pixels.CopyPixels(buffer, w * 4, 0);
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            if (buffer[(y * w + x) * 4 + 3] >= 24) { left = Math.Min(left, x); right = Math.Max(right, x); top = Math.Min(top, y); bottom = Math.Max(bottom, y); }
        ImageSource result = bitmap;
        if (right >= left && bottom >= top)
        {
            left = Math.Max(0, left - 1); top = Math.Max(0, top - 1);
            right = Math.Min(w - 1, right + 1); bottom = Math.Min(h - 1, bottom + 1);
            var cropped = new CroppedBitmap(bitmap, new Int32Rect(left, top, right - left + 1, bottom - top + 1));
            cropped.Freeze(); result = cropped;
        }
        Images.Add(name, result); return result;
    }
    protected override void OnRender(DrawingContext dc)
    {
        double size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0) return;
        if (SystemParameters.HighContrast || ThemeService.Instance.IsHighContrast || string.IsNullOrEmpty(Art))
        {
            var brush = SystemParameters.HighContrast || ThemeService.Instance.IsHighContrast ? SystemColors.WindowTextBrush : (Brush)(TryFindResource("CubeText") ?? Brushes.White);
            var text = new FormattedText(Glyph, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI Symbol"), size * .48, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            if (string.IsNullOrEmpty(Art))
            {
                var surface = (Brush)(TryFindResource(IsActive ? "CubeSelected" : "CubeInset") ?? Brushes.Transparent);
                dc.DrawEllipse(surface, IsActive ? new Pen(brush, 1.5) : null, new Point(ActualWidth / 2, ActualHeight / 2), size * .44, size * .44);
            }
            dc.DrawText(text, new Point((ActualWidth - text.Width) / 2, (ActualHeight - text.Height) / 2));
            return;
        }
        bool official = TryFindResource("CubePlayerUsesOfficialArtwork") is not false;
        var image = !official && TryFindResource("CubePlayerButtonFace") is ImageSource face ? face : Load(Art);
        double scale = size / Math.Max(image.Width, image.Height);
        var bounds = new Rect((ActualWidth - image.Width * scale) / 2, (ActualHeight - image.Height * scale) / 2, image.Width * scale, image.Height * scale);
        dc.DrawImage(image, bounds);
        if (IsActive)
        {
            dc.PushOpacity(.3);
            dc.PushOpacityMask(new ImageBrush(image) { Stretch = Stretch.Fill });
            dc.DrawRectangle((Brush)(TryFindResource("BackingPlayed") ?? Brushes.SteelBlue), null, bounds);
            dc.Pop(); dc.Pop();
        }
        if (!official)
        {
            var brush = (Brush)(TryFindResource(IsActive ? "BackingPlayed" : "CubePlayerGlyph") ?? Brushes.Black);
            if (Art is "sound" or "mute")
            {
                DrawSpeaker(dc, brush, size, Art == "mute");
                return;
            }
            if (DrawTransport(dc, brush, size)) return;
            var text = new FormattedText(Glyph, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface("Segoe UI Symbol"), size * .44, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            if (text.Width > size * .68) text.SetFontSize(size * .44 * size * .68 / text.Width);
            dc.DrawText(text, new Point((ActualWidth - text.Width) / 2, (ActualHeight - text.Height) / 2));
        }
    }
    private bool DrawTransport(DrawingContext dc, Brush brush, double size)
    {
        if (Art is not ("play" or "pause" or "stop" or "previous" or "next" or "back" or "forward" or "loop")) return false;
        dc.PushTransform(new TranslateTransform((ActualWidth - size) / 2, (ActualHeight - size) / 2));
        dc.PushTransform(new ScaleTransform(size / 56, size / 56));
        void Triangle(double left, double right, bool forward)
        {
            var triangle = new StreamGeometry();
            using (var path = triangle.Open())
            {
                path.BeginFigure(new Point(forward ? left : right, 18), true, true);
                path.LineTo(new Point(forward ? left : right, 38), true, false);
                path.LineTo(new Point(forward ? right : left, 28), true, false);
            }
            dc.DrawGeometry(brush, null, triangle);
        }
        switch (Art)
        {
            case "play": Triangle(21, 38, true); break;
            case "pause":
                dc.DrawRectangle(brush, null, new Rect(19, 18, 6, 20));
                dc.DrawRectangle(brush, null, new Rect(31, 18, 6, 20)); break;
            case "stop": dc.DrawRectangle(brush, null, new Rect(19, 19, 18, 18)); break;
            case "previous": Triangle(21, 36, false); dc.DrawRectangle(brush, null, new Rect(16, 18, 3, 20)); break;
            case "next": Triangle(20, 35, true); dc.DrawRectangle(brush, null, new Rect(37, 18, 3, 20)); break;
            case "back": Triangle(14, 26, false); Triangle(28, 40, false); break;
            case "forward": Triangle(16, 28, true); Triangle(30, 42, true); break;
            case "loop":
                var arrows = new StreamGeometry();
                using (var path = arrows.Open())
                {
                    path.BeginFigure(new Point(20, 20), false, false);
                    path.LineTo(new Point(36, 20), true, false);
                    path.ArcTo(new Point(42, 26), new Size(6, 6), 0, false, SweepDirection.Clockwise, true, false);
                    path.LineTo(new Point(42, 31), true, false);
                    path.BeginFigure(new Point(36, 36), false, false);
                    path.LineTo(new Point(20, 36), true, false);
                    path.ArcTo(new Point(14, 30), new Size(6, 6), 0, false, SweepDirection.Clockwise, true, false);
                    path.LineTo(new Point(14, 25), true, false);
                }
                dc.DrawGeometry(null, new Pen(brush, 2.5), arrows);
                var heads = new StreamGeometry();
                using (var path = heads.Open())
                {
                    path.BeginFigure(new Point(38, 29), true, true);
                    path.LineTo(new Point(46, 29), true, false); path.LineTo(new Point(42, 36), true, false);
                    path.BeginFigure(new Point(10, 27), true, true);
                    path.LineTo(new Point(18, 27), true, false); path.LineTo(new Point(14, 20), true, false);
                }
                dc.DrawGeometry(brush, null, heads); break;
        }
        dc.Pop(); dc.Pop();
        return true;
    }
    private void DrawSpeaker(DrawingContext dc, Brush brush, double size, bool muted)
    {
        dc.PushTransform(new TranslateTransform((ActualWidth - size) / 2, (ActualHeight - size) / 2));
        dc.PushTransform(new ScaleTransform(size / 56, size / 56));
        var speaker = new StreamGeometry();
        using (var path = speaker.Open())
        {
            path.BeginFigure(new Point(14, 24), true, true);
            path.PolyLineTo(new[] { new Point(20, 24), new Point(28, 18), new Point(28, 38), new Point(20, 32), new Point(14, 32) }, true, false);
        }
        dc.DrawGeometry(brush, null, speaker);
        var pen = new Pen(brush, 2.2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        if (muted)
        {
            dc.DrawLine(pen, new Point(33, 24), new Point(41, 32));
            dc.DrawLine(pen, new Point(33, 32), new Point(41, 24));
        }
        else
        {
            var waves = new StreamGeometry();
            using (var path = waves.Open())
            {
                path.BeginFigure(new Point(33, 23), false, false);
                path.BezierTo(new Point(38, 25), new Point(38, 31), new Point(33, 33), true, false);
                path.BeginFigure(new Point(36, 18), false, false);
                path.BezierTo(new Point(45, 22), new Point(45, 34), new Point(36, 38), true, false);
            }
            dc.DrawGeometry(null, pen, waves);
        }
        dc.Pop(); dc.Pop();
    }
}

public sealed class BackingFader : Slider
{
    public BackingFader()
    {
        Loaded += (_, _) => { ThemeService.Instance.Changed += RefreshTemplate; RefreshTemplate(); };
        Unloaded += (_, _) => ThemeService.Instance.Changed -= RefreshTemplate;
    }
    private int wheelDelta;
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!IsEnabled) { base.OnMouseWheel(e); return; }
        wheelDelta += e.Delta; int steps = wheelDelta / 120; wheelDelta %= 120;
        if (steps != 0) SetCurrentValue(ValueProperty, Math.Clamp(Value + steps, Minimum, Maximum));
        e.Handled = true;
    }
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == OrientationProperty && IsLoaded) RefreshTemplate();
    }
    private void RefreshTemplate()
    {
        bool accessible = ThemeService.Instance.IsHighContrast || SystemParameters.HighContrast;
        string key = Orientation == Orientation.Vertical
            ? accessible ? "BackingAccessibleVerticalFaderTemplate" : "OfficialVerticalFaderTemplate"
            : accessible ? "BackingAccessibleFaderTemplate" : "OfficialHorizontalFaderTemplate";
        SetCurrentValue(TemplateProperty, FindResource(key));
    }
}
