using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop.Controls;

public sealed class AmpEqGraph : FrameworkElement
{
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(AmpViewModel), typeof(AmpEqGraph),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, ModelChanged));
    public static readonly DependencyProperty SelectedFilterProperty = DependencyProperty.Register(nameof(SelectedFilter), typeof(int), typeof(AmpEqGraph),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public AmpViewModel? Model { get => (AmpViewModel?)GetValue(ModelProperty); set => SetValue(ModelProperty, value); }
    public int SelectedFilter { get => (int)GetValue(SelectedFilterProperty); set => SetCurrentValue(SelectedFilterProperty, Math.Clamp(value, 0, 1)); }
    public event RoutedEventHandler? EditCommitted;
    private bool dragging, changed, subscribed;
    private Point previous;
    private static readonly Brush A = new SolidColorBrush(Color.FromRgb(255, 162, 147));
    private static readonly Brush B = new SolidColorBrush(Color.FromRgb(115, 198, 222));
    private Rect Plot => new(48, 16, Math.Max(1, ActualWidth - 68), Math.Max(1, ActualHeight - 50));
    public AmpEqGraph()
    {
        MinHeight = 260; Focusable = true;
        Loaded += (_, _) => Subscribe();
        Unloaded += (_, _) => { Unsubscribe(); dragging = false; if (IsMouseCaptured) ReleaseMouseCapture(); };
    }
    private static void ModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var graph = (AmpEqGraph)d;
        if (graph.subscribed && e.OldValue is AmpViewModel old) { old.Edited -= graph.Refresh; old.PropertyChanged -= graph.ModelPropertyChanged; }
        graph.subscribed = false;
        if (graph.IsLoaded) graph.Subscribe();
    }
    private void Subscribe()
    {
        if (subscribed || Model == null) return;
        Model.Edited += Refresh; Model.PropertyChanged += ModelPropertyChanged; subscribed = true; InvalidateVisual();
    }
    private void Unsubscribe()
    {
        if (!subscribed || Model == null) return;
        Model.Edited -= Refresh; Model.PropertyChanged -= ModelPropertyChanged; subscribed = false;
    }
    private void Refresh() => InvalidateVisual();
    private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e) => InvalidateVisual();
    private double X(double hz) => Plot.Left + Math.Log(hz / 20) / Math.Log(1000) * Plot.Width;
    private double Y(double db) => Plot.Top + (24 - db) / 48 * Plot.Height;
    private void Text(DrawingContext dc, string text, Point at, Brush brush) => dc.DrawText(
        new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 11, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip), at);
    private static double Response(double[] c, double hz)
    {
        var z = Complex.FromPolarCoordinates(1, -2 * Math.PI * hz / 48000);
        var denominator = 1 + c[3] * z + c[4] * z * z;
        double magnitude = ((c[0] + c[1] * z + c[2] * z * z) / denominator).Magnitude;
        return double.IsFinite(magnitude) ? 20 * Math.Log10(Math.Max(1e-12, magnitude)) : 0;
    }
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var foreground = TryFindResource("TextFillColorPrimaryBrush") as Brush ?? Brushes.White;
        var grid = TryFindResource("ControlStrokeColorDefaultBrush") as Brush ?? Brushes.DimGray;
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        foreach (double hz in new double[] { 20, 100, 1000, 10000, 20000 })
        {
            double x = X(hz); dc.DrawLine(new Pen(grid, 1), new(x, Plot.Top), new(x, Plot.Bottom));
            Text(dc, hz >= 1000 ? $"{hz / 1000:0}k" : $"{hz:0}", new(x - 8, Plot.Bottom + 6), foreground);
        }
        foreach (int db in new[] { -24, -12, 0, 12, 24 })
        {
            double y = Y(db); dc.DrawLine(new Pen(grid, db == 0 ? 1.5 : .5), new(Plot.Left, y), new(Plot.Right, y));
            Text(dc, $"{db} dB", new(0, y - 7), foreground);
        }
        if (Model is not { Loaded: true } model) return;
        var ca = model.EqCoefficients(true); var cb = model.EqCoefficients(false);
        dc.PushClip(new RectangleGeometry(Plot));
        void Curve(Brush brush, int filter)
        {
            var geometry = new StreamGeometry();
            using (var path = geometry.Open())
                for (int i = 0; i <= 320; i++)
                {
                    double hz = 20 * Math.Pow(1000, i / 320d);
                    double db = filter == 0 ? Response(ca, hz) : filter == 1 ? Response(cb, hz) : Response(ca, hz) + Response(cb, hz);
                    var point = new Point(X(hz), Y(Math.Clamp(db, -120, 120)));
                    if (i == 0) path.BeginFigure(point, false, false); else path.LineTo(point, true, false);
                }
            dc.DrawGeometry(null, new Pen(brush, filter == 2 ? 2 : 1), geometry);
        }
        Curve(A, 0); Curve(B, 1); Curve(foreground, 2);
        dc.Pop();
        for (int i = 0; i < 2; i++)
        {
            var controls = i == 0 ? model.EqAControls : model.EqBControls;
            if (controls.Length != 3) continue;
            var point = new Point(X(controls[0].Number), Y(controls[1].Number));
            dc.DrawEllipse(i == 0 ? A : B, new Pen(foreground, i == SelectedFilter ? 2 : 0), point, 8, 8);
            Text(dc, i == 0 ? "A" : "B", new(point.X + 10, point.Y - 16), foreground);
        }
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!IsEnabled || Model is not { Loaded: true }) return;
        Point at = e.GetPosition(this);
        var candidates = new[] { Model.EqAControls, Model.EqBControls };
        double best = 20;
        int selected = -1;
        for (int i = 0; i < 2; i++)
        {
            double distance = (at - new Point(X(candidates[i][0].Number), Y(candidates[i][1].Number))).Length;
            if (distance < best) { best = distance; selected = i; }
        }
        if (selected < 0) return;
        Focus(); SelectedFilter = selected; previous = at; changed = false; dragging = true; CaptureMouse(); e.Handled = true;
    }
    private void Move(Point at)
    {
        if (!dragging || Model == null) return;
        var controls = SelectedFilter == 0 ? Model.EqAControls : Model.EqBControls;
        double factor = (Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? .1 : 1;
        double hz = Math.Clamp(controls[0].Number * Math.Pow(1000, (at.X - previous.X) * factor / Plot.Width), 20, 20000);
        double db = Math.Clamp(controls[1].Number - (at.Y - previous.Y) * factor * 48 / Plot.Height, -12, 12);
        changed |= Math.Abs(hz - controls[0].Number) > 1e-10 || Math.Abs(db - controls[1].Number) > 1e-10;
        controls[0].SetNumber(hz); controls[1].SetNumber(db); previous = at;
    }
    protected override void OnMouseMove(MouseEventArgs e) { if (dragging) Move(e.GetPosition(this)); }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!dragging) return;
        Move(e.GetPosition(this)); Finish(); e.Handled = true;
    }
    protected override void OnLostMouseCapture(MouseEventArgs e) { base.OnLostMouseCapture(e); Finish(); }
    private void Finish()
    {
        if (!dragging) return;
        dragging = false; if (IsMouseCaptured) ReleaseMouseCapture();
        if (changed && IsLoaded) EditCommitted?.Invoke(this, new RoutedEventArgs());
    }
}
