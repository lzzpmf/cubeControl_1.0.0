using System.Windows;
using System.Windows.Media;

namespace Cube.Desktop.Studio.Controls;

/// <summary>Lightweight rendering of peaks already decoded by the media service.</summary>
public sealed class WaveformPreviewControl : FrameworkElement
{
    private StreamGeometry? geometry;
    private IReadOnlyList<float>? drawnPeaks;
    private Size drawnSize;
    public static readonly DependencyProperty PeaksProperty = DependencyProperty.Register(nameof(Peaks), typeof(IReadOnlyList<float>), typeof(WaveformPreviewControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(nameof(Progress), typeof(double), typeof(WaveformPreviewControl), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(WaveformPreviewControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    public IReadOnlyList<float>? Peaks { get => (IReadOnlyList<float>?)GetValue(PeaksProperty); set => SetValue(PeaksProperty, value); }
    public double Progress { get => (double)GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
    public bool IsPlaying { get => (bool)GetValue(IsPlayingProperty); set => SetValue(IsPlayingProperty, value); }

    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int), typeof(WaveformPreviewControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public WaveformPreviewControl() { Height = 28; ClipToBounds = true; SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision"); }
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == PeaksProperty) { geometry = null; drawnPeaks = null; }
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (ActualWidth < 1 || ActualHeight < 1 || Peaks is not { Count: > 0 } peaks) return;
        var fill = TryFindResource("CubeItemHover") as Brush ?? new SolidColorBrush(Color.FromRgb(45, 51, 64));
        var accent = TryFindResource("CubeAccent") as Brush ?? Brushes.CornflowerBlue;
        dc.DrawRoundedRectangle(fill, null, new Rect(RenderSize), 4, 4);
        if (geometry == null || !ReferenceEquals(drawnPeaks, peaks) || drawnSize != RenderSize)
        {
            geometry = new StreamGeometry();
            using var context = geometry.Open();
            for (double x = 3; x < ActualWidth - 3; x += 3)
            {
                var start = Math.Clamp((int)(x / ActualWidth * peaks.Count), 0, peaks.Count - 1);
                var end = Math.Clamp((int)((x + 3) / ActualWidth * peaks.Count), start + 1, peaks.Count);
                float value = 0;
                for (var i = start; i < end; i++) if (float.IsFinite(peaks[i])) value = Math.Max(value, Math.Abs(peaks[i]));
                var peak = (ActualHeight / 2 - 4) * Math.Clamp(value, 0, 1);
                context.BeginFigure(new Point(x, ActualHeight / 2 - peak), false, false);
                context.LineTo(new Point(x, ActualHeight / 2 + peak), true, false);
            }
            drawnPeaks = peaks; drawnSize = RenderSize;
        }
        if (!geometry.IsFrozen) geometry.Freeze();
        dc.PushOpacity(.65);
        dc.DrawGeometry(null, new Pen(accent, 1), geometry);
        dc.Pop();
        if (IsPlaying)
        {
            var x = Math.Clamp(double.IsFinite(Progress) ? Progress : 0, 0, 1) * ActualWidth;
            dc.DrawLine(new Pen(accent, 2), new Point(x, 0), new Point(x, ActualHeight));
        }
    }
}
