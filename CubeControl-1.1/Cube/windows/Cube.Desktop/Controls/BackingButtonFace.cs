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
        bitmap.UriSource = new Uri("pack://application:,,,/CubeControl;component/Assets/imagens/backingTrackBotoes/oficial/" + Uri.EscapeDataString(name) + ".png");
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
            dc.DrawText(text, new Point((ActualWidth - text.Width) / 2, (ActualHeight - text.Height) / 2));
            return;
        }
        var image = Load(Art);
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
    private void RefreshTemplate() => SetCurrentValue(TemplateProperty,
        FindResource(ThemeService.Instance.IsHighContrast || SystemParameters.HighContrast ? "BackingAccessibleFaderTemplate" : "OfficialHorizontalFaderTemplate"));
}
