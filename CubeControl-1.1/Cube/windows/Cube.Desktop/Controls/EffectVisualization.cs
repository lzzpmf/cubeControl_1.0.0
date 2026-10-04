using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Cube.Desktop.Appearance;

namespace Cube.Desktop.Controls;

// Illustrative geometry only: never reads audio or writes to the editor/device.
public sealed class EffectVisualization : FrameworkElement
{
    private static DependencyProperty Input(string name, Type type, object value) =>
        DependencyProperty.Register(name, type, typeof(EffectVisualization),
            new FrameworkPropertyMetadata(value, FrameworkPropertyMetadataOptions.AffectsRender, Changed));
    public static readonly DependencyProperty ModeProperty = Input(nameof(Mode), typeof(int), 0);
    public static readonly DependencyProperty ActiveProperty = Input(nameof(Active), typeof(bool), false);
    public static readonly DependencyProperty PositionProperty = Input(nameof(Position), typeof(double), 1d);
    public static readonly DependencyProperty TimeProperty = Input(nameof(Time), typeof(double), 0d);
    public static readonly DependencyProperty FeedbackProperty = Input(nameof(Feedback), typeof(double), 0d);
    public static readonly DependencyProperty MixProperty = Input(nameof(Mix), typeof(double), 0d);
    public static readonly DependencyProperty AccentProperty = Input(nameof(Accent), typeof(Brush), Brushes.Gray);
    private static readonly DependencyProperty ThemeRevisionProperty = Input("ThemeRevision", typeof(int), 0);
    public int Mode { get => (int)GetValue(ModeProperty); set => SetValue(ModeProperty, value); }
    public bool Active { get => (bool)GetValue(ActiveProperty); set => SetValue(ActiveProperty, value); }
    public double Position { get => (double)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
    public double Time { get => (double)GetValue(TimeProperty); set => SetValue(TimeProperty, value); }
    public double Feedback { get => (double)GetValue(FeedbackProperty); set => SetValue(FeedbackProperty, value); }
    // Raw mix ranges: 0..118 for Delay and 1..15 for Reverb.
    public double Mix { get => (double)GetValue(MixProperty); set => SetValue(MixProperty, value); }
    public Brush Accent { get => (Brush)GetValue(AccentProperty); set => SetValue(AccentProperty, value); }

    private readonly DispatcherTimer timer = new(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(1000d / 30) };
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private Window? owner;
    private double phase, lastTick, transitionStart;
    private double[] current = new double[4], from = new double[4], target = new double[4];
    internal bool IsAnimating => timer.IsEnabled;
    private bool MotionAllowed => SystemParameters.ClientAreaAnimation && !SystemParameters.HighContrast && !ThemeService.Instance.IsHighContrast;
    private bool CanAnimate => IsLoaded && IsVisible && owner?.IsVisible == true && owner.WindowState != WindowState.Minimized && Active && Mode is >= 1 and <= 6 && MotionAllowed && (Mode != 3 || Mix > 0);

    public EffectVisualization()
    {
        IsHitTestVisible = false;
        ClipToBounds = true;
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
        timer.Tick += (_, _) => Tick();
        Loaded += (_, _) =>
        {
            owner = Window.GetWindow(this);
            if (owner != null) { owner.StateChanged += WindowChanged; owner.Closed += WindowClosed; }
            SystemParameters.StaticPropertyChanged += SystemChanged;
            UpdateInputs(false);
        };
        Unloaded += (_, _) =>
        {
            timer.Stop();
            if (owner != null) { owner.StateChanged -= WindowChanged; owner.Closed -= WindowClosed; }
            owner = null;
            SystemParameters.StaticPropertyChanged -= SystemChanged;
        };
        IsVisibleChanged += (_, _) => UpdateInputs(false);
    }

    private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((EffectVisualization)d).UpdateInputs(e.Property != ModeProperty && e.Property != ActiveProperty);
    private void WindowChanged(object? sender, EventArgs e) => UpdateInputs(false);
    private void WindowClosed(object? sender, EventArgs e) => timer.Stop();
    private void SystemChanged(object? sender, PropertyChangedEventArgs e) => UpdateInputs(false);
    private static double Unit(double value) => double.IsFinite(value) ? Math.Clamp(value, 0, 1) : 0;
    private void UpdateInputs(bool interpolate)
    {
        Sample();
        from = (double[])current.Clone();
        target = [Mode == 5 ? Unit(Position / 7) : Mode == 6 ? Unit(Position / 15) : Unit((Position - 1) / 6), Unit(Time / 31), Unit(Feedback / 127), Unit(Mix / (Mode == 4 ? 15 : 118))];
        transitionStart = clock.Elapsed.TotalSeconds;
        if (!interpolate || !CanAnimate) { current = (double[])target.Clone(); from = (double[])target.Clone(); }
        if (CanAnimate)
        {
            if (!timer.IsEnabled) { lastTick = clock.Elapsed.TotalSeconds; timer.Start(); }
        }
        else { timer.Stop(); phase = Math.PI / 4; }
        InvalidateVisual();
    }
    private void Sample()
    {
        double t = Math.Clamp((clock.Elapsed.TotalSeconds - transitionStart) / .15, 0, 1);
        t = t * t * (3 - 2 * t);
        for (int i = 0; i < 4; i++) current[i] = from[i] + (target[i] - from[i]) * t;
    }
    private void Tick()
    {
        if (!CanAnimate) { UpdateInputs(false); return; }
        double now = clock.Elapsed.TotalSeconds;
        phase = (phase + Math.Min(now - lastTick, .1) * Math.PI * .55) % (2 * Math.PI);
        lastTick = now;
        Sample();
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (ActualWidth < 32 || ActualHeight < 20 || Mode is < 1 or > 6) return;
        bool contrast = ThemeService.Instance.IsHighContrast;
        var accent = contrast ? ThemeService.Brush("CubeText") : Accent;
        var neutral = ThemeService.Brush("CubeBorder");
        double left = 12, width = ActualWidth - 24, middle = ActualHeight * .5;
        double height = Math.Min(38, ActualHeight * .34);
        dc.PushOpacity(Active || contrast ? 1 : .4);
        dc.DrawLine(new Pen(neutral, 1), new(left, middle), new(left + width, middle));
        void Curve(Func<double, double> y, Brush brush, double opacity = 1)
        {
            var geometry = new StreamGeometry();
            using (var path = geometry.Open())
            {
                path.BeginFigure(new(left, y(0)), false, false);
                for (int i = 1; i <= 180; i++) { double x = i / 180d; path.LineTo(new(left + x * width, y(x)), true, false); }
            }
            geometry.Freeze();
            dc.PushOpacity(contrast ? 1 : opacity);
            dc.DrawGeometry(null, new Pen(brush, 1.5), geometry);
            dc.Pop();
        }
        void Pulse(double x, double amplitude, Brush brush, double opacity = 1)
        {
            dc.PushOpacity(contrast ? 1 : opacity);
            dc.DrawLine(new Pen(brush, 2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round },
                new(left + x * width, middle + amplitude * height * .3), new(left + x * width, middle - amplitude * height));
            dc.Pop();
        }
        switch (Mode)
        {
            case 5:
                // Illustrative saturation: more gain rounds off the wave peaks.
                Curve(x => middle + Math.Sin(x * Math.PI * 4 - phase) * height * .35, neutral);
                double drive = 1 + current[0] * 5;
                Curve(x => middle + Math.Tanh(Math.Sin(x * Math.PI * 4 - phase) * drive)
                    * height * (.35 + current[0] * .45), accent);
                break;
            case 6:
                // Illustrative tonal balance, not a measured frequency response.
                double tilt = current[0] * 2 - 1;
                Curve(x => middle - tilt * Math.Tanh((x - .5) * 4) * height * .8, accent);
                break;
            case 1:
                Curve(x => middle + Math.Sin(x * Math.PI * 4) * height * .42, neutral);
                double separation = .15 + current[0] * .95;
                for (int voice = -1; voice <= 1; voice += 2)
                {
                    int side = voice;
                    Curve(x => middle + Math.Sin(x * Math.PI * 4 + side * separation * (.65 + .35 * Math.Sin(phase))) * height * .42 + side * separation * height * .22, accent, side == 1 ? .9 : .5);
                }
                break;
            case 2:
                double sweep = Math.Sin(phase) * (.015 + current[0] * .065);
                Curve(x =>
                {
                    double valleys = 0;
                    for (int i = 0; i < 3; i++) valleys += Math.Exp(-Math.Pow((x - (.22 + i * .28 + sweep)) / .043, 2));
                    return middle - height * .4 + valleys * height * 1.05;
                }, accent);
                break;
            case 3:
                Pulse(.025, 1, contrast ? accent : ThemeService.Brush("Muted"));
                double spacing = .09 + current[1] * .14;
                for (int i = 1; i <= 10 && .025 + spacing * i < .98; i++)
                {
                    double level = current[3] * Math.Pow(current[2] * .88, i - 1);
                    if (level < .008) continue;
                    Pulse(.025 + spacing * i, level, accent, .65 + .25 * Math.Cos(phase - i * .55));
                }
                break;
            case 4:
                Pulse(.025, 1, contrast ? accent : ThemeService.Brush("Muted"));
                // Fixed envelope/density: only the wet contribution changes with Mix.
                Curve(x =>
                {
                    if (x < .07) return middle;
                    double t = (x - .07) / .93;
                    double envelope = Math.Exp(-4.2 * t) * Math.Min(1, t * 28) * current[3];
                    double texture = Math.Sin(t * 170) * .55 + Math.Sin(t * 293) * .3 + Math.Sin(t * 419) * .15;
                    return middle + texture * height * envelope;
                }, accent, .8 + .1 * Math.Sin(phase));
                break;
        }
        dc.Pop();
    }
}
