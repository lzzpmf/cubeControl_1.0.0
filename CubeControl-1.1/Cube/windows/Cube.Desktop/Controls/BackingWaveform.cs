using System.Globalization;
using System.Buffers;
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
    public static readonly DependencyProperty PeakOffsetProperty = DependencyProperty.Register(nameof(PeakOffset), typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    public static readonly DependencyProperty PeakDurationProperty = DependencyProperty.Register(nameof(PeakDuration), typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    public double PeakOffset { get => (double)GetValue(PeakOffsetProperty); set => SetValue(PeakOffsetProperty, value); }
    public double PeakDuration { get => (double)GetValue(PeakDurationProperty); set => SetValue(PeakDurationProperty, value); }
    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    public static readonly DependencyProperty PositionProperty = Register(nameof(Position), 0);
    public static readonly DependencyProperty RangeStartProperty = Register(nameof(RangeStart), double.NaN);
    public static readonly DependencyProperty RangeEndProperty = Register(nameof(RangeEnd), double.NaN);
    public static readonly DependencyProperty IsLoopingProperty = DependencyProperty.Register(nameof(IsLooping), typeof(bool), typeof(BackingWaveform), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    private static DependencyProperty Register(string name, double value) => DependencyProperty.Register(name, typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(value, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    private static void GeometryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var wave = (BackingWaveform)sender;
        if (!wave.IsOverview || args.Property != ViewportStartProperty && args.Property != ViewportDurationProperty) wave.ClearWaveCache();
    }
    public float[] Peaks { get => (float[])GetValue(PeaksProperty); set => SetValue(PeaksProperty, value); }
    public double Duration { get => (double)GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public double Position { get => (double)GetValue(PositionProperty); set => SetCurrentValue(PositionProperty, value); }
    public double RangeStart { get => (double)GetValue(RangeStartProperty); set => SetCurrentValue(RangeStartProperty, value); }
    public double RangeEnd { get => (double)GetValue(RangeEndProperty); set => SetCurrentValue(RangeEndProperty, value); }
    public bool IsLooping { get => (bool)GetValue(IsLoopingProperty); set => SetValue(IsLoopingProperty, value); }
    public static readonly DependencyProperty ViewportStartProperty = DependencyProperty.Register(nameof(ViewportStart), typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, GeometryChanged));
    public static readonly DependencyProperty ViewportDurationProperty = DependencyProperty.Register(nameof(ViewportDuration), typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    public static readonly DependencyProperty ShowRulerProperty = DependencyProperty.Register(nameof(ShowRuler), typeof(bool), typeof(BackingWaveform), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty MusicalRulerProperty = DependencyProperty.Register(nameof(MusicalRuler), typeof(bool), typeof(BackingWaveform), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, MusicalGridChanged));
    public bool MusicalRuler { get => (bool)GetValue(MusicalRulerProperty); set => SetValue(MusicalRulerProperty, value); }
    public bool ShowRuler { get => (bool)GetValue(ShowRulerProperty); set => SetValue(ShowRulerProperty, value); }
    public static readonly DependencyProperty IsOverviewProperty = DependencyProperty.Register(nameof(IsOverview), typeof(bool), typeof(BackingWaveform), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    public static readonly DependencyProperty MeterProperty = DependencyProperty.Register(nameof(Meter), typeof(string), typeof(BackingWaveform), new FrameworkPropertyMetadata("4/4", FrameworkPropertyMetadataOptions.AffectsRender, MusicalGridChanged));
    public static readonly DependencyProperty FirstBeatProperty = DependencyProperty.Register(nameof(FirstBeat), typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender, MusicalGridChanged));
    public string Meter { get => (string)GetValue(MeterProperty); set => SetValue(MeterProperty, value); }
    public double FirstBeat { get => (double)GetValue(FirstBeatProperty); set => SetValue(FirstBeatProperty, value); }
    private static void MusicalGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { ((BackingWaveform)d).guides = null; }
    public static readonly DependencyProperty BpmProperty = Register(nameof(Bpm), 120);
    public static readonly DependencyProperty SectionsProperty = DependencyProperty.Register(nameof(Sections), typeof(System.Collections.ObjectModel.ObservableCollection<BackingSection>), typeof(BackingWaveform), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, SectionsChanged));
    public double ViewportStart { get => (double)GetValue(ViewportStartProperty); set => SetCurrentValue(ViewportStartProperty, value); }
    public double ViewportDuration { get => (double)GetValue(ViewportDurationProperty); set => SetValue(ViewportDurationProperty, value); }
    public bool IsOverview { get => (bool)GetValue(IsOverviewProperty); set => SetValue(IsOverviewProperty, value); }
    public double Bpm { get => (double)GetValue(BpmProperty); set => SetValue(BpmProperty, value); }
    public System.Collections.ObjectModel.ObservableCollection<BackingSection>? Sections { get => (System.Collections.ObjectModel.ObservableCollection<BackingSection>?)GetValue(SectionsProperty); set => SetValue(SectionsProperty, value); }
    private static void SectionsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var wave = (BackingWaveform)sender;
        if (args.OldValue is System.Collections.Specialized.INotifyCollectionChanged old) System.Collections.Specialized.CollectionChangedEventManager.RemoveHandler(old, wave.MarkersChanged);
        if (args.NewValue is System.Collections.Specialized.INotifyCollectionChanged current) System.Collections.Specialized.CollectionChangedEventManager.AddHandler(current, wave.MarkersChanged);
        wave.guides = null;
    }
    private void MarkersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) { guides = null; InvalidateVisual(); }
    private double VisibleStart => IsOverview ? 0 : Math.Clamp(ViewportStart, 0, Math.Max(0, Duration - VisibleDuration));
    private double VisibleDuration => IsOverview || ViewportDuration <= 0 ? Duration : Math.Min(Duration, ViewportDuration);
    internal int RenderCount { get; private set; }
    internal int WaveCacheBuildCount { get; private set; }
    internal long WaveCacheBuildTicks { get; private set; }
    private BitmapSource? neutralWave, playedWave;
    private DrawingGroup? guides;
    private (Size Size, double Start, double Duration, double Bpm, bool Overview, DpiScale Dpi, Brush Text, Brush Neutral, Brush Border) guideKey;
    private static readonly Geometry PlayheadMarker = MakePlayheadMarker();
    private static Geometry MakePlayheadMarker()
    {
        var shape = new StreamGeometry();
        using (var path = shape.Open()) { path.BeginFigure(new(-6, -8), true, true); path.LineTo(new(6, -8), true, false); path.LineTo(new(0, 0), true, false); }
        shape.Freeze(); return shape;
    }
    private static readonly Typeface WaveTypeface = new("Segoe UI");
    private Brush? cachedNeutral, cachedAccent;
    private DpiScale cachedDpi;
    private Size geometrySize;
    private int drag;
    private double preview, hover = double.NaN;
    private Brush ResourceBrush(string name, Brush fallback) => TryFindResource(name) as Brush ?? fallback;
    private double X(double seconds) => 12 + Math.Clamp((seconds - VisibleStart) / Math.Max(.001, VisibleDuration), 0, 1) * Math.Max(1, ActualWidth - 24);
    private double Time(double x) => Math.Clamp((x - 12) / Math.Max(1, ActualWidth - 24), 0, 1) * VisibleDuration + VisibleStart;
    public BackingWaveform()
    {
        Focusable = true; Cursor = Cursors.Hand; MinHeight = 24; ClipToBounds = true;
        MouseLeave += (_, _) => { hover = double.NaN; InvalidateVisual(); };
        // Tab navigation detaches the same control temporarily. Keep its bounded cache.
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
    }
    private void ClearWaveCache() { guides = null; neutralWave = playedWave = null; cachedNeutral = cachedAccent = null; }
    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int), typeof(BackingWaveform), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, GeometryChanged));
    protected override void OnRender(DrawingContext dc)
    {
        RenderCount++;
        base.OnRender(dc);
        if (MusicalRuler) { RenderMusicalRuler(dc); return; }
        if (ActualWidth < 26 || ActualHeight < (MusicalRuler ? 32 : IsOverview ? 20 : 80)) return;
        var foreground = ResourceBrush("CubeText", Brushes.White);
        var accent = ResourceBrush("BackingPlayed", Brushes.MediumAquamarine);
        var neutral = ResourceBrush("BackingFuture", Brushes.SteelBlue);
        var border = ResourceBrush("CubeBorder", Brushes.Gray);
        double surfaceRadius = ((CornerRadius)FindResource("CubeControlCornerRadius")).TopLeft;
        dc.DrawRoundedRectangle(ResourceBrush("CubeInset", Brushes.Black), null, new Rect(RenderSize), surfaceRadius, surfaceRadius);
        void Text(string text, double x, double y, Brush? brush = null)
        {
            var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, WaveTypeface, text is "A" or "B" ? 16 : 12, brush ?? foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(formatted, new Point(Math.Clamp(x, 2, Math.Max(2, ActualWidth - formatted.Width - 2)), y));
        }
        if (Duration <= 0) { Text(LanguageService.T("Carregue uma faixa MP3/WAV"), 16, ActualHeight / 2); return; }
        double a = drag == 1 ? preview : RangeStart, b = drag == 2 ? preview : RangeEnd;
        if (double.IsFinite(a) && double.IsFinite(b) && b > a)
        {
            dc.PushOpacity(IsLooping ? .28 : .13); dc.DrawRectangle(accent, null, new Rect(X(a), 25, Math.Max(0, X(b) - X(a)), ActualHeight - 30)); dc.Pop();
        }
        var dpi = VisualTreeHelper.GetDpi(this);
        var currentGuideKey = (RenderSize, VisibleStart, VisibleDuration, Bpm, IsOverview, dpi, foreground, neutral, border);
        if (guides == null || !guideKey.Equals(currentGuideKey))
        {
            guides = new DrawingGroup(); guideKey = currentGuideKey;
            using (var grid = guides.Open())
            {
                void GuideText(string text, double x, double y, Brush? brush = null)
                {
                    var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, WaveTypeface, 12, brush ?? foreground, dpi.PixelsPerDip);
                    grid.DrawText(formatted, new Point(Math.Clamp(x, 2, Math.Max(2, ActualWidth - formatted.Width - 2)), y));
                }
                double desired = VisibleDuration / Math.Max(1, (ActualWidth - 24) / 80);
                double magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(.1, desired))));
                double tick = new[] { 1d, 2, 5, 10 }.First(v => v * magnitude >= desired) * magnitude;
                for (double t = Math.Ceiling(VisibleStart / tick) * tick; !IsOverview && ShowRuler && !MusicalRuler && t < VisibleStart + VisibleDuration; t += tick)
                {
                    double x = X(t);
                    if (t > 0 && X(Duration) - x < 80) break;
                    grid.DrawLine(new Pen(border, 1), new(x, 24), new(x, ActualHeight - 8)); GuideText(BackingTracksViewModel.FormatTime(t, tick < 1), x, 4);
                }
                if (!IsOverview && ShowRuler && !MusicalRuler) GuideText(BackingTracksViewModel.FormatTime(VisibleStart + VisibleDuration, tick < 1), ActualWidth - 52, 4);
                if (!IsOverview && ShowRuler && Bpm >= 20 && Bpm <= 300)
                {
                    var meter = Meter.Split('/');
                    double bar = 60 / Bpm * (int.TryParse(meter[0], out int numerator) ? numerator : 4) * 4 / (meter.Length > 1 && int.TryParse(meter[1], out int denominator) ? denominator : 4);
                    int stride = Math.Max(1, (int)Math.Ceiling(VisibleDuration / bar / Math.Max(1, ActualWidth / 70)));
                    for (double t = FirstBeat + Math.Ceiling((VisibleStart - FirstBeat) / (bar * stride)) * bar * stride; t < VisibleStart + VisibleDuration; t += bar * stride)
                    {
                        double x = X(t); grid.PushOpacity(.4); grid.DrawLine(new Pen(border, 1), new(x, 48), new(x, ActualHeight - 28)); grid.Pop();
                        // The 104-DIP session ruler reserves separate bands for
                        // time (4), A/B (30), sections (55) and bar numbers (80).
                        GuideText($"{(int)Math.Round((t - FirstBeat) / bar) + 1}", x + 3, MusicalRuler ? 2 : Math.Min(80, ActualHeight - 16), neutral);
                    }
                }
                if (!IsOverview && ShowRuler && Sections != null)
                {
                    var markers = Sections.Where(m => m.Position >= VisibleStart && m.Position <= VisibleStart + VisibleDuration).OrderBy(m => m.Position).ToArray();
                    for (int i = 0; i < markers.Length; i++)
                    {
                        double x = X(markers[i].Position);
                        grid.DrawLine(new Pen(foreground, 1), new(x, 55), new(x, 70));
                        double available = (i + 1 < markers.Length ? X(markers[i + 1].Position) : ActualWidth) - x - 8;
                        if (available < 24) continue;
                        var label = new FormattedText(markers[i].Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, WaveTypeface, 12, foreground, dpi.PixelsPerDip)
                        { MaxTextWidth = available, MaxLineCount = 1, Trimming = TextTrimming.CharacterEllipsis };
                        grid.DrawText(label, new Point(x + 3, 55));
                    }
                }
            }
            guides.Freeze();
        }
        dc.DrawDrawing(guides);
        // Build pixels directly: RenderTargetBitmap.Render blocks the UI while WPF
        // rasterizes every segment, taking seconds on software/remote renderers.
        if (neutralWave == null || playedWave == null || geometrySize != RenderSize || !cachedDpi.Equals(dpi) ||
            cachedNeutral != neutral || cachedAccent != accent)
        {
            BuildWaveImages(neutral, accent, dpi);
            geometrySize = RenderSize; cachedNeutral = neutral; cachedAccent = accent; cachedDpi = dpi;
        }
        dc.DrawImage(neutralWave, new Rect(RenderSize));
        double playhead = X(drag == 3 ? preview : Position);
        dc.PushClip(new RectangleGeometry(new Rect(0, IsOverview ? 0 : 24, playhead, ActualHeight - (IsOverview ? 0 : 24))));
        dc.DrawImage(playedWave, new Rect(RenderSize)); dc.Pop();
        foreach (var (time, label) in new[] { (a, "A"), (b, "B") })
        {
            if (!double.IsFinite(time) || time < VisibleStart || time > VisibleStart + VisibleDuration || IsOverview) continue;
            double x = X(time); dc.DrawLine(new Pen(accent, 3), new(x, 25), new(x, ActualHeight - 5));
            dc.DrawRoundedRectangle(ResourceBrush("CubeInset", Brushes.Black), new Pen(accent, 2), MarkerBounds(time), 3, 3); Text(label, x - 5, 30);
        }
        dc.DrawLine(new Pen(ResourceBrush("CubeInset", Brushes.Black), 5), new(playhead, IsOverview ? 0 : 24), new(playhead, ActualHeight - 5));
        var cursorBrush = ResourceBrush("BackingPlayhead", Brushes.Gold);
        dc.DrawLine(new Pen(cursorBrush, IsOverview ? 2 : 3), new(playhead, IsOverview ? 0 : 24), new(playhead, ActualHeight - 5));
        if (!IsOverview)
        {
            dc.PushTransform(new TranslateTransform(playhead, 24));
            dc.DrawGeometry(cursorBrush, null, PlayheadMarker); dc.Pop();
        }
        if (IsOverview && ViewportDuration > 0)
        {
            double left = X(ViewportStart), right = X(ViewportStart + ViewportDuration);
            dc.PushOpacity(.18); dc.DrawRectangle(accent, null, new Rect(left, 1, Math.Max(1, right - left), ActualHeight - 2)); dc.Pop();
            dc.DrawRectangle(null, new Pen(accent, 2), new Rect(left, 1, Math.Max(1, right - left), ActualHeight - 2));
        }
        if (!IsOverview && double.IsFinite(hover)) Text(BackingTracksViewModel.FormatTime(drag != 0 ? preview : hover, true), X(hover) + 8, ActualHeight - 19);
        if (IsKeyboardFocused) dc.DrawRoundedRectangle(ResourceBrush("CubeFocus", accent), null, new Rect(0, Math.Max(0, (ActualHeight - 24) / 2), 3, Math.Min(24, ActualHeight)), 2, 2);
    }
    private void BuildWaveImages(Brush neutral, Brush accent, DpiScale dpi)
    {
        long began = System.Diagnostics.Stopwatch.GetTimestamp();
        int width = Math.Clamp((int)Math.Ceiling(ActualWidth * dpi.DpiScaleX), 1, 8192);
        int height = Math.Clamp((int)Math.Ceiling(ActualHeight * dpi.DpiScaleY), 1, 2048);
        int count = checked(width * height);
        var coverage = ArrayPool<byte>.Shared.Rent(count);
        var pixels = ArrayPool<uint>.Shared.Rent(count);
        try
        {
            Array.Clear(coverage, 0, count);
            double sx = width / ActualWidth, sy = height / ActualHeight;
            int bars = Math.Max(1, (int)(ActualWidth - 24) / 2);
            double center = IsOverview ? ActualHeight / 2 : (ActualHeight + 20) / 2;
            double amplitude = Math.Max(0, IsOverview ? ActualHeight / 2 - 5 : ActualHeight / 2 - 32);
            var peaks = Peaks;
            for (int i = 0; i < bars && peaks.Length > 0; i++)
            {
                double peakDuration = PeakDuration > 0 ? PeakDuration : Duration;
                double from = VisibleStart + i * VisibleDuration / bars - PeakOffset;
                double to = VisibleStart + (i + 1) * VisibleDuration / bars - PeakOffset;
                if (to <= 0 || from >= peakDuration || peakDuration <= 0) continue;
                int first = Math.Clamp((int)(from / peakDuration * peaks.Length), 0, peaks.Length - 1);
                int last = Math.Min(peaks.Length, Math.Max(first + 1, (int)(to / peakDuration * peaks.Length)));
                float peak = 0;
                bool available = false;
                for (int j = first; j < last; j++)
                    if (float.IsFinite(peaks[j])) { peak = Math.Max(peak, peaks[j]); available = true; }
                if (!available) continue;
                double x = (12 + i * (ActualWidth - 24) / bars) * sx;
                double y = Math.Max(.5, Math.Min(1, peak) * amplitude);
                double left = x - .7 * sx, right = x + .7 * sx, top = (center - y) * sy, bottom = (center + y) * sy;
                for (int px = Math.Max(0, (int)Math.Floor(left)); px < Math.Min(width, (int)Math.Ceiling(right)); px++)
                {
                    double horizontal = Math.Max(0, Math.Min(px + 1, right) - Math.Max(px, left));
                    for (int py = Math.Max(0, (int)Math.Floor(top)); py < Math.Min(height, (int)Math.Ceiling(bottom)); py++)
                    {
                        byte alpha = (byte)Math.Round(255 * horizontal * Math.Max(0, Math.Min(py + 1, bottom) - Math.Max(py, top)));
                        int index = py * width + px; coverage[index] = Math.Max(coverage[index], alpha);
                    }
                }
            }
            BitmapSource Create(Brush brush)
            {
                Color color = (brush as SolidColorBrush)?.Color ?? Colors.Gray;
                var palette = new uint[256];
                for (int i = 0; i < palette.Length; i++)
                {
                    uint alpha = (uint)Math.Round(i * color.A / 255d * brush.Opacity);
                    palette[i] = alpha << 24 | (uint)(color.R * alpha / 255) << 16 | (uint)(color.G * alpha / 255) << 8 | (uint)(color.B * alpha / 255);
                }
                for (int i = 0; i < count; i++) pixels[i] = palette[coverage[i]];
                // Create copies the supplied pixels; pooled buffers can be returned safely.
                var bitmap = BitmapSource.Create(width, height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32, null, pixels, width * 4);
                bitmap.Freeze(); WaveCacheBuildCount++; return bitmap;
            }
            neutralWave = Create(neutral); playedWave = Create(accent);
        }
        finally { ArrayPool<byte>.Shared.Return(coverage); ArrayPool<uint>.Shared.Return(pixels); }
        WaveCacheBuildTicks += System.Diagnostics.Stopwatch.GetTimestamp() - began;
    }
    internal Action<int, double>? ZoomRequested { get; set; }
    private int wheelDelta;
    private bool wheelZoom;
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (IsOverview || Duration <= 0) { base.OnMouseWheel(e); return; }
        bool zoom = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        if (zoom != wheelZoom) { wheelDelta = 0; wheelZoom = zoom; }
        wheelDelta += e.Delta; int steps = wheelDelta / 120; wheelDelta %= 120;
        if (zoom)
        {
            if (steps != 0) ZoomRequested?.Invoke(steps, Math.Clamp(e.GetPosition(this).X / Math.Max(1, ActualWidth), 0, 1));
            e.Handled = true;
            return;
        }
        double lower = IsLooping && double.IsFinite(RangeStart) ? RangeStart : 0;
        double upper = IsLooping && double.IsFinite(RangeEnd) ? RangeEnd : Duration;
        if (steps != 0) Position = Math.Clamp(Position + steps * (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 1 : 5), lower, upper);
        e.Handled = true;
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (Duration <= 0 || !IsEnabled) return;
        var at = e.GetPosition(this);
        drag = IsOverview ? 5 : HitMarker(at);
        if (drag == 0) drag = 3;
        Focus(); CaptureMouse(); Preview(at.X); e.Handled = true;
    }
    private void RenderMusicalRuler(DrawingContext dc)
    {
        if (ActualWidth < 26 || ActualHeight < 24 || VisibleDuration <= 0) return;
        var foreground = ResourceBrush("CubeText", Brushes.White);
        var border = ResourceBrush("CubeBorder", Brushes.Gray);
        var accent = ResourceBrush("BackingPlayed", Brushes.MediumAquamarine);
        dc.DrawRectangle(ResourceBrush("CubeWell", Brushes.Black), null, new Rect(RenderSize));
        void Label(string text, double x, double y, Brush brush)
        {
            var label = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, WaveTypeface, 12, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(label, new Point(Math.Clamp(x, 0, Math.Max(0, ActualWidth - label.Width)), y));
        }
        string[] meter = Meter.Split('/');
        int numerator = int.TryParse(meter[0], out int n) ? n : 4;
        int denominator = meter.Length > 1 && int.TryParse(meter[1], out int d) ? d : 4;
        double beat = 60 / Math.Max(20, Bpm) * 4 / denominator;
        double bar = beat * numerator;
        int stride = Math.Max(1, (int)Math.Ceiling(VisibleDuration / bar / Math.Max(1, ActualWidth / 50)));
        for (double t = FirstBeat + Math.Ceiling((VisibleStart - FirstBeat) / beat) * beat; t <= VisibleStart + VisibleDuration; t += beat)
        {
            bool major = Math.Abs((t - FirstBeat) / bar - Math.Round((t - FirstBeat) / bar)) < .0001;
            dc.DrawLine(new Pen(border, 1), new Point(X(t), major ? 20 : 25), new Point(X(t), 30));
            int index = (int)Math.Round((t - FirstBeat) / bar);
            if (major && index % stride == 0) Label((index + 1).ToString(CultureInfo.CurrentCulture), X(t) + 2, 0, foreground);
        }
        dc.DrawLine(new Pen(border, 1), new Point(0, 30), new Point(ActualWidth, 30));
        double a = drag == 1 ? preview : RangeStart, b = drag == 2 ? preview : RangeEnd;
        if (double.IsFinite(a) && double.IsFinite(b) && b > a)
        {
            double left = X(a), right = X(b);
            var fill = accent.Clone(); fill.Opacity = SystemParameters.HighContrast ? 1 : .2;
            dc.DrawRectangle(fill, new Pen(accent, 1), new Rect(left, 31, Math.Max(0, right - left), 19));
            int first = (int)Math.Floor((a - FirstBeat) / bar) + 1, last = (int)Math.Ceiling((b - FirstBeat) / bar);
            Label($"Loop ({first} – {last})", left + Math.Max(0, (right - left) / 2 - 35), 32, foreground);
            foreach (var (x, direction) in new[] { (left, 1), (right, -1) })
            {
                var triangle = new StreamGeometry(); using (var g = triangle.Open()) { g.BeginFigure(new Point(x,31),true,true); g.LineTo(new Point(x+direction*10,40),true,false); g.LineTo(new Point(x,50),true,false); }
                dc.DrawGeometry(accent,null,triangle);
            }
        }
        double cursor = X(drag == 3 ? preview : Position);
        dc.DrawLine(new Pen(ResourceBrush("BackingPlayhead", Brushes.Gold), 1), new Point(cursor,30), new Point(cursor,ActualHeight));
    }
    private Rect MarkerBounds(double time) => MusicalRuler
        ? new Rect(X(time) - 10, 28, 20, 24)
        : new Rect(Math.Clamp(X(time) - 18, 0, Math.Max(0, ActualWidth - 36)), 24, 36, 30);
    private int HitMarker(Point point)
    {
        if (IsOverview) return 0;
        bool Hit(double time) => double.IsFinite(time) && time >= VisibleStart && time <= VisibleStart + VisibleDuration && MarkerBounds(time).Contains(point);
        // B is painted last when the markers overlap.
        return Hit(RangeEnd) ? 2 : Hit(RangeStart) ? 1 : 0;
    }
    private void Preview(double x)
    {
        preview = Time(x);
        if (drag == 1) preview = Math.Clamp(preview, 0, double.IsFinite(RangeEnd) ? Math.Max(0, RangeEnd - .1) : Duration);
        if (drag == 2) preview = Math.Clamp(preview, double.IsFinite(RangeStart) ? Math.Min(Duration, RangeStart + .1) : 0, Duration);
        if (drag == 5) ViewportStart = Math.Clamp(preview - ViewportDuration / 2, 0, Math.Max(0, Duration - ViewportDuration));
        InvalidateVisual();
    }
    protected override void OnMouseMove(MouseEventArgs e) { hover = Time(e.GetPosition(this).X); if (IsMouseCaptured) Preview(e.GetPosition(this).X); else InvalidateVisual(); }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!IsMouseCaptured) return;
        Preview(e.GetPosition(this).X);
        if (drag == 1) RangeStart = preview;
        else if (drag == 2) RangeEnd = preview;
        else if (drag == 3) Position = preview;
        drag = 0; ReleaseMouseCapture(); InvalidateVisual(); e.Handled = true;
    }
    internal Action<BackingWaveform, double?>? MenuRequested { get; set; }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if ((e.Key == Key.Apps || e.Key == Key.F10 && Keyboard.Modifiers == ModifierKeys.Shift) && MenuRequested != null)
        { MenuRequested(this, null); e.Handled = true; return; }
        if (e.Key == Key.Escape && IsMouseCaptured) { drag = 0; ReleaseMouseCapture(); InvalidateVisual(); e.Handled = true; }
        base.OnKeyDown(e);
    }
    protected override void OnLostMouseCapture(MouseEventArgs e) { drag = 0; InvalidateVisual(); base.OnLostMouseCapture(e); }
    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        if (MenuRequested != null) { MenuRequested(this, Time(e.GetPosition(this).X)); e.Handled = true; return; }
        if (DataContext is not BackingTracksViewModel model) return;
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = LanguageService.T("Limpar A"), Command = model.ClearStart });
        menu.Items.Add(new MenuItem { Header = LanguageService.T("Limpar B"), Command = model.ClearEnd });
        menu.Items.Add(new MenuItem { Header = LanguageService.T("Limpar A/B"), Command = model.ResetRange });
        menu.PlacementTarget = this; menu.IsOpen = true; e.Handled = true;
    }
}
