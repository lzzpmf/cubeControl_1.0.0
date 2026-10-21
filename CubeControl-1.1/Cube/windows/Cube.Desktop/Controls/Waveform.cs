using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;

namespace Cube.Desktop.Controls;
public sealed class Waveform : FrameworkElement
{
    private double[] spectrum = [];
    private Point? hover;
    private const double Rate = 48000;
    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int), typeof(Waveform), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public Waveform()
    {
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
        MouseMove += (_, e) => { hover = e.GetPosition(this); InvalidateVisual(); };
        MouseLeave += (_, _) => { hover = null; ToolTip = null; InvalidateVisual(); };
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new WaveformPeer(this);
    private sealed class WaveformPeer(Waveform owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Image;
        protected override string GetClassNameCore() => nameof(Waveform);
    }
    public static readonly DependencyProperty SamplesProperty = DependencyProperty.Register(nameof(Samples), typeof(float[]), typeof(Waveform), new FrameworkPropertyMetadata(Array.Empty<float>(), FrameworkPropertyMetadataOptions.AffectsRender, (d, _) =>
    {
        var view = (Waveform)d;
        view.spectrum = Spectrum(view.Samples);
        view.hover = null; view.ToolTip = null;
    }));
    public float[] Samples { get => (float[])GetValue(SamplesProperty); set => SetValue(SamplesProperty, value); }
    // DFT of the prepared impulse itself, without a window that would alter its response.
    // Cache on sample changes, never recompute during mouse movement or rendering.
    internal static double[] Spectrum(float[] samples)
    {
        if (samples.Length < 2) return [];
        var magnitudes = new double[samples.Length / 2];
        for (int k = 1; k <= magnitudes.Length; k++)
        {
            double real = 0, imaginary = 0;
            for (int n = 0; n < samples.Length; n++)
            {
                double angle = 2 * Math.PI * k * n / samples.Length;
                real += samples[n] * Math.Cos(angle);
                imaginary -= samples[n] * Math.Sin(angle);
            }
            magnitudes[k - 1] = Math.Sqrt(real * real + imaginary * imaginary);
        }
        double peak = magnitudes.Max();
        return magnitudes.Select(m => peak <= 1e-12 ? -60 : Math.Max(-60, 20 * Math.Log10(Math.Max(m / peak, 1e-12)))).ToArray();
    }
    protected override void OnRender(DrawingContext dc)
    {
        if (ActualWidth < 120 || ActualHeight < 220) return;
        var muted = ThemeService.Brush("Muted");
        var ink = ThemeService.Brush("CubePurple");
        var grid = new Pen(ThemeService.Brush("CubeBorder"), .7);
        void Text(string text, double x, double y, bool strong = false)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI Variable Text, Segoe UI Variable, Segoe UI"), FontStyles.Normal, strong ? FontWeights.SemiBold : FontWeights.Normal, FontStretches.Normal),
                strong ? 14 : 12, strong ? ThemeService.Brush("CubeText") : muted, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(formatted, new(Math.Clamp(x, 0, Math.Max(0, ActualWidth - formatted.Width)), y));
        }
        var time = new Rect(38, 24, ActualWidth - 50, 70);
        var frequency = new Rect(38, 145, ActualWidth - 50, 70);
        double surfaceRadius = ((CornerRadius)FindResource("CubeControlCornerRadius")).TopLeft;
        dc.DrawRoundedRectangle(ThemeService.Brush("CubeWell"), null, new Rect(RenderSize), surfaceRadius, surfaceRadius);
        Text(LanguageService.T("Impulso · amplitude normalizada"), 10, 4, true);
        Text(LanguageService.T("Frequência · dB relativos ao pico"), 10, 123, true);
        foreach (int db in new[] { 0, -30, -60 })
        {
            double y = frequency.Top - db / 60d * frequency.Height;
            dc.DrawLine(grid, new(frequency.Left, y), new(frequency.Right, y)); Text(db.ToString(), 4, y - 7);
        }
        dc.DrawLine(grid, new(time.Left, time.Top + time.Height / 2), new(time.Right, time.Top + time.Height / 2));
        Text("+1", 6, time.Top - 5); Text("0", 10, time.Top + time.Height / 2 - 7); Text("−1", 6, time.Bottom - 9);
        foreach (int ms in new[] { 0, 2, 4, 6, 8, 10 })
        {
            double x = time.Left + ms / (512d / Rate * 1000) * time.Width;
            dc.DrawLine(grid, new(x, time.Top), new(x, time.Bottom)); Text(ms + (ms == 10 ? " ms" : ""), x - 4, time.Bottom + 4);
        }
        double Fx(double hz) => frequency.Left + Math.Log(hz / 93.75) / Math.Log(20000 / 93.75) * frequency.Width;
        foreach (double hz in new[] { 100d, 1000, 10000, 20000 })
        {
            double x = Fx(hz); dc.DrawLine(grid, new(x, frequency.Top), new(x, frequency.Bottom));
            // Avoid overlapping the two highest tick labels on narrow layouts.
            if (hz != 10000 || frequency.Width > 300) Text(hz >= 1000 ? (hz / 1000) + "k" : "100 Hz", x - 10, frequency.Bottom + 4);
        }
        if (Samples.Length < 2)
        {
            Text(LanguageService.T("Carregue um IR para analisar."), time.Left + 6, time.Top + 10);
            return;
        }
        double peak = Math.Max(1e-12, Samples.Max(s => Math.Abs(s)));
        void Draw(IEnumerable<Point> points)
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                bool first = true;
                foreach (var point in points)
                {
                    if (first) { context.BeginFigure(point, false, false); first = false; }
                    else context.LineTo(point, true, false);
                }
            }
            geometry.Freeze(); dc.DrawGeometry(null, new Pen(ink, 1.4), geometry);
        }
        Draw(Samples.Select((sample, i) => new Point(time.Left + i / (double)Samples.Length * time.Width, time.Top + time.Height / 2 - sample / peak * time.Height * .46)));
        Draw(spectrum.Select((db, i) => (Hz: (i + 1) * Rate / Samples.Length, Db: db)).Where(p => p.Hz <= 20000)
            .Select(p => new Point(Fx(p.Hz), frequency.Top - p.Db / 60 * frequency.Height)));
        Text(LanguageService.T("512 amostras · 48 kHz · resolução ≈ 94 Hz"), 10, 241);
        if (hover is not { } position) return;
        string? detail = null;
        Rect plot = time.Contains(position) ? time : frequency;
        if (time.Contains(position))
        {
            int sample = Math.Clamp((int)((position.X - time.Left) / time.Width * Samples.Length), 0, Samples.Length - 1);
            detail = $"{sample / Rate * 1000:0.00} ms · {LanguageService.T("Amplitude")}: {Samples[sample]:0.00000}";
        }
        else if (frequency.Contains(position))
        {
            double hz = 93.75 * Math.Pow(20000 / 93.75, (position.X - frequency.Left) / frequency.Width);
            int bin = Math.Clamp((int)Math.Round(hz * Samples.Length / Rate), 1, spectrum.Length);
            detail = $"{bin * Rate / Samples.Length:0} Hz · {spectrum[bin - 1]:0.0} dB";
        }
        if (detail != null)
        {
            dc.DrawLine(new Pen(ThemeService.Brush("CubeText"), .7), new(position.X, plot.Top), new(position.X, plot.Bottom));
            ToolTip = detail;
        }
        else ToolTip = null;
    }
}
