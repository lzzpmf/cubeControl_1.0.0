using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Controls;

public sealed class BackingWaveform : FrameworkElement
{
    public static readonly DependencyProperty PeaksProperty = DependencyProperty.Register(nameof(Peaks), typeof(float[]), typeof(BackingWaveform), new FrameworkPropertyMetadata(Array.Empty<float>(), FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    public static readonly DependencyProperty PositionProperty = Register(nameof(Position), 0);
    public static readonly DependencyProperty RangeStartProperty = Register(nameof(RangeStart), double.NaN);
    public static readonly DependencyProperty RangeEndProperty = Register(nameof(RangeEnd), double.NaN);
    public static readonly DependencyProperty IsLoopingProperty = DependencyProperty.Register(nameof(IsLooping), typeof(bool), typeof(BackingWaveform), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    private static DependencyProperty Register(string name, double value) => DependencyProperty.Register(name, typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(value, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    private static void GeometryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((BackingWaveform)sender).ClearWaveCache();
    public float[] Peaks { get => (float[])GetValue(PeaksProperty); set => SetValue(PeaksProperty, value); }
    public double Duration { get => (double)GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public double Position { get => (double)GetValue(PositionProperty); set => SetCurrentValue(PositionProperty, value); }
    public double RangeStart { get => (double)GetValue(RangeStartProperty); set => SetCurrentValue(RangeStartProperty, value); }
    public double RangeEnd { get => (double)GetValue(RangeEndProperty); set => SetCurrentValue(RangeEndProperty, value); }
    public bool IsLooping { get => (bool)GetValue(IsLoopingProperty); set => SetValue(IsLoopingProperty, value); }
    private StreamGeometry? geometry;
    private RenderTargetBitmap? neutralWave, playedWave;
    private Brush? cachedNeutral, cachedAccent;
    private DpiScale cachedDpi;
    private Size geometrySize;
    private int drag;
    private double preview, hover = double.NaN;
    private Brush ResourceBrush(string name, Brush fallback) => TryFindResource(name) as Brush ?? fallback;
    private double X(double seconds) => 12 + Math.Clamp(seconds / Math.Max(.001, Duration), 0, 1) * Math.Max(1, ActualWidth - 24);
    private double Time(double x) => Math.Clamp((x - 12) / Math.Max(1, ActualWidth - 24), 0, 1) * Duration;
    public BackingWaveform()
    {
        Focusable = true; Cursor = Cursors.Hand; MinHeight = 150;
        MouseLeave += (_, _) => { hover = double.NaN; InvalidateVisual(); };
        Unloaded += (_, _) => ClearWaveCache();
    }
    private void ClearWaveCache() { geometry = null; neutralWave = playedWave = null; cachedNeutral = cachedAccent = null; }
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (ActualWidth < 26 || ActualHeight < 40) return;
        var foreground = ResourceBrush("CubeText", Brushes.White);
        var accent = ResourceBrush("CubeAccent", Brushes.Coral);
        var neutral = ResourceBrush("Muted", Brushes.Gray);
        var border = ResourceBrush("CubeBorder", Brushes.Gray);
        dc.DrawRoundedRectangle(ResourceBrush("CubeInset", Brushes.Black), null, new Rect(RenderSize), 8, 8);
        void Text(string text, double x, double y, Brush? brush = null)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 11, brush ?? foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(formatted, new Point(Math.Clamp(x, 2, Math.Max(2, ActualWidth - formatted.Width - 2)), y));
        }
        if (Duration <= 0) { Text(LanguageService.T("Carregue uma faixa MP3/WAV"), 16, ActualHeight / 2); return; }
        double a = drag == 1 ? preview : RangeStart, b = drag == 2 ? preview : RangeEnd;
        if (double.IsFinite(a) && double.IsFinite(b) && b > a)
        {
            dc.PushOpacity(IsLooping ? .16 : .06); dc.DrawRectangle(accent, null, new Rect(X(a), 25, Math.Max(0, X(b) - X(a)), ActualHeight - 30)); dc.Pop();
        }
        double desired = Duration / Math.Max(1, (ActualWidth - 24) / 80);
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(.1, desired))));
        double tick = new[] { 1d, 2, 5, 10 }.First(v => v * magnitude >= desired) * magnitude;
        for (double t = 0; t < Duration; t += tick)
        {
            double x = X(t);
            if (t > 0 && X(Duration) - x < 80) break;
            dc.DrawLine(new Pen(border, 1), new(x, 24), new(x, ActualHeight - 8)); Text(BackingTracksViewModel.FormatTime(t, tick < 1), x, 4);
        }
        Text(BackingTracksViewModel.FormatTime(Duration), ActualWidth - 52, 4);
        if (geometry == null || geometrySize != RenderSize)
        {
            geometry = new StreamGeometry(); geometrySize = RenderSize;
            using (var drawing = geometry.Open())
            {
                var peaks = Peaks;
                int bars = Math.Max(1, (int)(ActualWidth - 24) / 2); double center = (ActualHeight + 20) / 2, height = ActualHeight / 2 - 32;
                for (int i = 0; i < bars && peaks.Length > 0; i++)
                {
                    int first = i * peaks.Length / bars, last = Math.Min(peaks.Length, Math.Max(first + 1, (i + 1) * peaks.Length / bars));
                    float peak = 0; for (int j = first; j < last; j++) peak = Math.Max(peak, peaks[j]);
                    double x = 12 + i * (ActualWidth - 24) / bars, y = Math.Max(.5, Math.Min(1, peak) * height);
                    drawing.BeginFigure(new(x, center - y), false, false); drawing.LineTo(new(x, center + y), true, false);
                }
            }
            geometry.Freeze();
            neutralWave = playedWave = null;
        }
        // Rasterize the immutable envelope once. Re-stroking hundreds of vertical
        // segments on every playhead tick saturated a CPU core in WPF rendering.
        var dpi = VisualTreeHelper.GetDpi(this);
        if (neutralWave == null || playedWave == null || !cachedDpi.Equals(dpi) ||
            cachedNeutral != neutral || cachedAccent != accent)
        {
            neutralWave = RasterizeWave(neutral, dpi);
            playedWave = RasterizeWave(accent, dpi);
            cachedNeutral = neutral; cachedAccent = accent; cachedDpi = dpi;
        }
        dc.DrawImage(neutralWave, new Rect(RenderSize));
        double playhead = X(drag == 3 ? preview : Position);
        dc.PushClip(new RectangleGeometry(new Rect(0, 24, playhead, ActualHeight - 24)));
        dc.DrawImage(playedWave, new Rect(RenderSize)); dc.Pop();
        foreach (var (time, label) in new[] { (a, "A"), (b, "B") })
        {
            if (!double.IsFinite(time)) continue;
            double x = X(time); dc.DrawLine(new Pen(accent, 1.5), new(x, 25), new(x, ActualHeight - 5));
            dc.DrawRoundedRectangle(ResourceBrush("CubeInset", Brushes.Black), new Pen(accent, 1), new Rect(x - 10, 25, 20, 20), 3, 3); Text(label, x - 4, 28);
        }
        dc.DrawLine(new Pen(ResourceBrush("CubeInset", Brushes.Black), 5), new(playhead, 24), new(playhead, ActualHeight - 5));
        dc.DrawLine(new Pen(accent, 2), new(playhead, 24), new(playhead, ActualHeight - 5));
        if (double.IsFinite(hover)) Text(BackingTracksViewModel.FormatTime(drag != 0 ? preview : hover, true), X(hover) + 8, ActualHeight - 19);
        if (IsKeyboardFocused) dc.DrawRoundedRectangle(null, new Pen(accent, 1), new Rect(1, 1, ActualWidth - 2, ActualHeight - 2), 8, 8);
    }
    private RenderTargetBitmap RasterizeWave(Brush brush, DpiScale dpi)
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen()) dc.DrawGeometry(null, new Pen(brush, 1.4), geometry);
        var bitmap = new RenderTargetBitmap(Math.Max(1, (int)Math.Ceiling(ActualWidth * dpi.DpiScaleX)),
            Math.Max(1, (int)Math.Ceiling(ActualHeight * dpi.DpiScaleY)), dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
        bitmap.Render(visual); bitmap.Freeze();
        return bitmap;
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (Duration <= 0 || !IsEnabled) return;
        var at = e.GetPosition(this);
        drag = double.IsFinite(RangeStart) && Math.Abs(at.X - X(RangeStart)) < 12 ? 1 : double.IsFinite(RangeEnd) && Math.Abs(at.X - X(RangeEnd)) < 12 ? 2 : 3;
        Focus(); CaptureMouse(); Preview(at.X); e.Handled = true;
    }
    private void Preview(double x)
    {
        preview = Time(x);
        if (drag == 1) preview = Math.Clamp(preview, 0, Math.Max(0, (double.IsFinite(RangeEnd) ? RangeEnd : Duration) - .1));
        if (drag == 2) preview = Math.Clamp(preview, Math.Min(Duration, (double.IsFinite(RangeStart) ? RangeStart : 0) + .1), Duration);
        InvalidateVisual();
    }
    protected override void OnMouseMove(MouseEventArgs e) { hover = Time(e.GetPosition(this).X); if (IsMouseCaptured) Preview(e.GetPosition(this).X); else InvalidateVisual(); }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!IsMouseCaptured) return;
        Preview(e.GetPosition(this).X);
        if (drag == 1) RangeStart = preview; else if (drag == 2) RangeEnd = preview; else if (drag == 3) Position = preview;
        drag = 0; ReleaseMouseCapture(); InvalidateVisual(); e.Handled = true;
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsMouseCaptured) { drag = 0; ReleaseMouseCapture(); InvalidateVisual(); e.Handled = true; }
        base.OnKeyDown(e);
    }
    protected override void OnLostMouseCapture(MouseEventArgs e) { drag = 0; InvalidateVisual(); base.OnLostMouseCapture(e); }
    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        if (DataContext is not BackingTracksViewModel model) return;
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = LanguageService.T("Limpar A"), Command = model.ClearStart });
        menu.Items.Add(new MenuItem { Header = LanguageService.T("Limpar B"), Command = model.ClearEnd });
        menu.Items.Add(new MenuItem { Header = LanguageService.T("Limpar A/B"), Command = model.ResetRange });
        menu.PlacementTarget = this; menu.IsOpen = true; e.Handled = true;
    }
}
