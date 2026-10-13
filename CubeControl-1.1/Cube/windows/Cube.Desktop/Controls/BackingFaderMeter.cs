using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Cube.Desktop.Appearance;
using Cube.Desktop.Services;

namespace Cube.Desktop.Controls;

// Passive drawing layer: the slider above owns all input and accessibility.
public sealed class BackingFaderMeter : FrameworkElement
{
    public static readonly DependencyProperty ReadingProperty = DependencyProperty.Register(
        nameof(Reading), typeof(BackingMeterReading), typeof(BackingFaderMeter),
        new FrameworkPropertyMetadata(BackingMeterReading.Silent, FrameworkPropertyMetadataOptions.AffectsRender));
    public BackingMeterReading Reading { get => (BackingMeterReading)GetValue(ReadingProperty); set => SetValue(ReadingProperty, value); }
    public BackingFaderMeter()
    {
        IsHitTestVisible = false; Focusable = false;
        Loaded += (_, _) => { ThemeService.Instance.Changed += InvalidateVisual; InvalidateVisual(); };
        Unloaded += (_, _) => ThemeService.Instance.Changed -= InvalidateVisual;
    }
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        double width = Math.Max(0, ActualWidth - 20);
        if (width < 1) return;
        bool contrast = ThemeService.Instance.IsHighContrast || SystemParameters.HighContrast;
        Brush Resource(string name) => (Brush)FindResource(name);
        Brush inactive = contrast ? SystemColors.WindowBrush : Resource("CubeWell");
        Brush normal = contrast ? SystemColors.HighlightBrush : Resource("CubeAccent");
        Brush warning = contrast ? SystemColors.HighlightBrush : Resource("CubeWarning");
        Brush text = contrast ? SystemColors.WindowTextBrush : Resource("CubeText");
        double y = ActualHeight / 2 - 10;
        int segments = Math.Max(1, (int)(width / 6));
        double step = width / segments;
        for (int i = 0; i < segments; i++)
        {
            double db = -60 + (i + 1d) / segments * 60;
            bool lit = Reading.LevelDb > -60 && db <= Math.Min(0, Reading.LevelDb) + 60d / segments;
            var rect = new Rect(10 + i * step, y, Math.Max(1, step - 2), 20);
            dc.DrawRoundedRectangle(lit ? db >= -6 ? warning : normal : inactive,
                contrast ? new Pen(text, .5) : null, rect, 1, 1);
        }
        if (Reading.PeakDb > -60)
        {
            double x = 10 + Math.Clamp((Reading.PeakDb + 60) / 60, 0, 1) * (width - 2);
            dc.DrawRectangle(text, null, new Rect(x, y - 1, 2, 22));
        }
        var typeface = new Typeface((FontFamily)FindResource("CubeUiFont"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        foreach (double db in new[] { -60d, -36, -18, -6, 0 })
        {
            if (width < 240 && (db is -36 or -6)) continue;
            string label = db == 0 ? "0 dBFS" : db.ToString("0", CultureInfo.InvariantCulture);
            var formatted = new FormattedText(label, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                typeface, (double)FindResource("CubeFontSizeCaption"), text, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            double x = Math.Clamp(10 + (db + 60) / 60 * width - formatted.Width / 2, 0, Math.Max(0, ActualWidth - formatted.Width));
            dc.DrawText(formatted, new Point(x, ActualHeight - formatted.Height));
        }
    }
}
