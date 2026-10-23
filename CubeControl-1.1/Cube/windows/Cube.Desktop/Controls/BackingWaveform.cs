using System.Windows;
using System.Windows.Media;
namespace Cube.Desktop.Controls;
public sealed class BackingWaveform : FrameworkElement
{
    public BackingWaveform()
    {
        Loaded += (_, _) => Appearance.ThemeService.Instance.Changed += InvalidateVisual;
        Unloaded += (_, _) => Appearance.ThemeService.Instance.Changed -= InvalidateVisual;
    }
    private static DependencyProperty Register(string name, Type type, object value) => DependencyProperty.Register(name, type, typeof(BackingWaveform), new FrameworkPropertyMetadata(value, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty PeaksProperty = Register(nameof(Peaks), typeof(float[]), Array.Empty<float>());
    public static readonly DependencyProperty DurationProperty = Register(nameof(Duration), typeof(double), 0d);
    public static readonly DependencyProperty PositionProperty = Register(nameof(Position), typeof(double), 0d);
    public static readonly DependencyProperty IsOverviewProperty = Register(nameof(IsOverview), typeof(bool), true);
    public static readonly DependencyProperty ShowRulerProperty = Register(nameof(ShowRuler), typeof(bool), false);
    public float[] Peaks { get => (float[])GetValue(PeaksProperty); set => SetValue(PeaksProperty, value); }
    public double Duration { get => (double)GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    public double Position { get => (double)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
    public bool IsOverview { get => (bool)GetValue(IsOverviewProperty); set => SetValue(IsOverviewProperty, value); }
    public bool ShowRuler { get => (bool)GetValue(ShowRulerProperty); set => SetValue(ShowRulerProperty, value); }
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var peaks = Peaks;
        if (peaks.Length == 0 || ActualWidth <= 0 || ActualHeight <= 0) return;
        var played = TryFindResource("BackingPlayed") as Brush ?? Brushes.MediumAquamarine;
        var future = TryFindResource("BackingFuture") as Brush ?? Brushes.SteelBlue;
        double fraction = Duration > 0 ? Math.Clamp(Position / Duration, 0, 1) : 0;
        for (int x = 0; x < (int)ActualWidth; x += 2)
        {
            int index = Math.Min(peaks.Length - 1, (int)(x / ActualWidth * peaks.Length));
            float peak = peaks[index]; if (!float.IsFinite(peak)) continue;
            double height = Math.Max(1, Math.Clamp(Math.Abs(peak), 0, 1) * ActualHeight);
            dc.DrawRectangle(x / ActualWidth <= fraction ? played : future, null, new Rect(x, (ActualHeight - height) / 2, 1, height));
        }
    }
}
