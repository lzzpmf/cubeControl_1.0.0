using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Cube.Desktop.Controls;

public sealed class BackingWaveform : FrameworkElement
{
    public static readonly DependencyProperty PeaksProperty = DependencyProperty.Register(nameof(Peaks), typeof(float[]), typeof(BackingWaveform), new FrameworkPropertyMetadata(Array.Empty<float>(), FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(nameof(Duration), typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty PositionProperty = Register(nameof(Position));
    public static readonly DependencyProperty RangeStartProperty = Register(nameof(RangeStart));
    public static readonly DependencyProperty RangeEndProperty = Register(nameof(RangeEnd));
    private static DependencyProperty Register(string name) => DependencyProperty.Register(name, typeof(double), typeof(BackingWaveform), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public float[] Peaks { get => (float[])GetValue(PeaksProperty); set => SetValue(PeaksProperty, value); }
    public double Duration { get => (double)GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public double Position { get => (double)GetValue(PositionProperty); set => SetCurrentValue(PositionProperty, value); }
    public double RangeStart { get => (double)GetValue(RangeStartProperty); set => SetCurrentValue(RangeStartProperty, value); }
    public double RangeEnd { get => (double)GetValue(RangeEndProperty); set => SetCurrentValue(RangeEndProperty, value); }
    private int drag;
    private double X(double seconds) => 12 + Math.Clamp(seconds / Math.Max(.001, Duration), 0, 1) * Math.Max(1, ActualWidth - 24);
    private Brush Brush(string name, Brush fallback) => TryFindResource(name) as Brush ?? fallback;
    public BackingWaveform() { Focusable = true; Cursor = Cursors.Hand; MinHeight = 150; }
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var foreground = Brush("CubeText", Brushes.White);
        var accent = Brush("CubeAccent", Brushes.Coral);
        var grid = Brush("CubeBorder", Brushes.Gray);
        dc.DrawRoundedRectangle(Brush("CubeInset", Brushes.Black), null, new Rect(RenderSize), 8, 8);
        double center = (ActualHeight + 20) / 2, height = Math.Max(1, ActualHeight / 2 - 32);
        void Text(string text, double x, double y) => dc.DrawText(new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 11, foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip), new Point(x, y));
        if (Duration <= 0) return;
        dc.PushOpacity(.12);
        dc.DrawRectangle(accent, null, new Rect(X(RangeStart), 24, Math.Max(0, X(RangeEnd) - X(RangeStart)), Math.Max(0, ActualHeight - 30)));
        dc.Pop();
        for (int i = 0; i <= 4; i++)
        {
            double t = Duration * i / 4, x = X(t);
            dc.DrawLine(new Pen(grid, 1), new(x, 24), new(x, ActualHeight - 8));
            Text(TimeSpan.FromSeconds(t).ToString(@"mm\:ss"), Math.Min(x, Math.Max(0, ActualWidth - 42)), 4);
        }
        dc.DrawLine(new Pen(grid, 1), new(12, center), new(ActualWidth - 12, center));
        int bars = Math.Max(1, (int)(ActualWidth - 24) / 2);
        for (int i = 0; i < bars && Peaks.Length > 0; i++)
        {
            int first = i * Peaks.Length / bars, last = Math.Min(Peaks.Length, Math.Max(first + 1, (i + 1) * Peaks.Length / bars));
            float peak = 0; for (int j = first; j < last; j++) peak = Math.Max(peak, Peaks[j]);
            double x = 12 + i * (ActualWidth - 24) / bars, y = Math.Max(.5, peak * height);
            dc.DrawLine(new Pen(accent, 1.4), new(x, center - y), new(x, center + y));
        }
        foreach (var (time, label) in new[] { (RangeStart, "A"), (RangeEnd, "B") })
        {
            double x = X(time);
            dc.DrawLine(new Pen(accent, 2), new(x, 24), new(x, ActualHeight - 5));
            dc.DrawRoundedRectangle(accent, null, new Rect(x - 5, 24, 10, 14), 2, 2);
            Text(label, Math.Clamp(x + 7, 0, Math.Max(0, ActualWidth - 14)), 26);
        }
        dc.DrawLine(new Pen(foreground, 2), new(X(Position), 22), new(X(Position), ActualHeight - 5));
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (Duration <= 0 || !IsEnabled) return;
        var at = e.GetPosition(this);
        drag = Math.Abs(at.X - X(RangeStart)) < 10 ? 1 : Math.Abs(at.X - X(RangeEnd)) < 10 ? 2 : 3;
        Focus(); CaptureMouse(); Move(at); e.Handled = true;
    }
    private void Move(Point at)
    {
        double time = Math.Clamp((at.X - 12) / Math.Max(1, ActualWidth - 24), 0, 1) * Duration;
        if (drag == 1) RangeStart = Math.Min(time, Math.Max(0, RangeEnd - .1));
        else if (drag == 2) RangeEnd = Math.Max(time, Math.Min(Duration, RangeStart + .1));
        else if (drag == 3) Position = time;
    }
    protected override void OnMouseMove(MouseEventArgs e) { if (IsMouseCaptured) Move(e.GetPosition(this)); }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) { if (IsMouseCaptured) { Move(e.GetPosition(this)); ReleaseMouseCapture(); } drag = 0; e.Handled = true; }
    protected override void OnLostMouseCapture(MouseEventArgs e) { drag = 0; base.OnLostMouseCapture(e); }
}
