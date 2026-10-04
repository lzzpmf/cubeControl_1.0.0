using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation;
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
    public static readonly DependencyProperty BandABrushProperty = DependencyProperty.Register(nameof(BandABrush), typeof(Brush), typeof(AmpEqGraph), new FrameworkPropertyMetadata(Brushes.Coral, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty BandBBrushProperty = DependencyProperty.Register(nameof(BandBBrush), typeof(Brush), typeof(AmpEqGraph), new FrameworkPropertyMetadata(Brushes.LightBlue, FrameworkPropertyMetadataOptions.AffectsRender));
    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int), typeof(AmpEqGraph), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public Brush BandABrush { get => (Brush)GetValue(BandABrushProperty); set => SetValue(BandABrushProperty, value); }
    public Brush BandBBrush { get => (Brush)GetValue(BandBBrushProperty); set => SetValue(BandBBrushProperty, value); }
    public event RoutedEventHandler? EditCommitted;
    private bool dragging, changed, subscribed;
    private Point previous;
    private int hovered = -1;
    private Rect Plot => new(48, 16, Math.Max(1, ActualWidth - 68), Math.Max(1, ActualHeight - 50));
    public AmpEqGraph()
    {
        MinHeight = 260; Focusable = true;
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
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
    private double Y(double db) => Plot.Top + (12 - db) / 24 * Plot.Height;
    private void Text(DrawingContext dc, string text, Point at, Brush brush) => dc.DrawText(
        new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(TryFindResource("CubeUiFont") as FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 11, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip), at);
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
        foreach (int db in new[] { -12, -6, 0, 6, 12 })
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
            var pen = new Pen(brush, filter == 2 ? 2 : filter == SelectedFilter ? 2.5 : 1.25);
            if (filter == 1) pen.DashStyle = DashStyles.Dash;
            dc.DrawGeometry(null, pen, geometry);
        }
        Curve(BandABrush, 0); Curve(BandBBrush, 1); Curve(foreground, 2);
        dc.Pop();
        for (int i = 0; i < 2; i++)
        {
            var controls = i == 0 ? model.EqAControls : model.EqBControls;
            if (controls.Length != 3) continue;
            var point = new Point(X(controls[0].Number), Y(controls[1].Number));
            Brush brush = i == 0 ? BandABrush : BandBBrush;
            if (i == SelectedFilter && (dragging || hovered == i || IsKeyboardFocused))
            {
                var guide = new Pen(grid, 1) { DashStyle = DashStyles.Dot };
                dc.DrawLine(guide, new(point.X, Plot.Top), new(point.X, Plot.Bottom));
                dc.DrawLine(guide, new(Plot.Left, point.Y), new(Plot.Right, point.Y));
            }
            dc.DrawEllipse(brush, new Pen(foreground, i == SelectedFilter ? 2 : 0), point, i == hovered ? 10 : 8, i == hovered ? 10 : 8);
            Text(dc, i == 0 ? "A" : "B", new(Math.Min(point.X + 10, Plot.Right - 10), point.Y - 16), foreground);
            if (i == SelectedFilter)
                AutomationProperties.SetHelpText(this, $"EQ {(i == 0 ? "A" : "B")}: {controls[0].Display}; {controls[1].Display}; Q {controls[2].Display}");
        }
    }
    public int FilterAt(Point at)
    {
        if (!IsEnabled || Model is not { Loaded: true } model) return -1;
        int found = -1;
        double best = 20;
        for (int i = 0; i < 2; i++)
        {
            var controls = i == 0 ? model.EqAControls : model.EqBControls;
            if (controls.Length != 3) continue;
            double distance = (at - new Point(X(controls[0].Number), Y(controls[1].Number))).Length;
            if (distance < best || (distance == best && i == SelectedFilter))
            { best = distance; found = i; }
        }
        return found;
    }
    public bool AdjustQ(Point at, int delta)
    {
        int filter = FilterAt(at);
        if (filter < 0 || Model == null || delta == 0) return false;
        SelectedFilter = filter;
        var q = (filter == 0 ? Model.EqAControls : Model.EqBControls)[2];
        double step = (Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? .01 : .1;
        q.SetNumber(Math.Clamp(q.Number + delta / 120d * step, q.Minimum, q.Maximum));
        return true;
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
        double db = Math.Clamp(controls[1].Number - (at.Y - previous.Y) * factor * 24 / Plot.Height, -12, 12);
        changed |= Math.Abs(hz - controls[0].Number) > 1e-10 || Math.Abs(db - controls[1].Number) > 1e-10;
        controls[0].SetNumber(hz); controls[1].SetNumber(db); previous = at;
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (dragging) { Move(e.GetPosition(this)); return; }
        int next = -1;
        if (Model is { Loaded: true } model && IsEnabled)
        {
            Point at = e.GetPosition(this); double best = 20;
            for (int i = 0; i < 2; i++)
            {
                var controls = i == 0 ? model.EqAControls : model.EqBControls;
                double distance = (at - new Point(X(controls[0].Number), Y(controls[1].Number))).Length;
                if (distance < best) { best = distance; next = i; }
            }
        }
        Cursor = next >= 0 ? Cursors.SizeAll : Cursors.Arrow;
        if (hovered != next) { hovered = next; InvalidateVisual(); }
    }
    protected override void OnMouseLeave(MouseEventArgs e) { base.OnMouseLeave(e); hovered = -1; if (!dragging) Cursor = Cursors.Arrow; InvalidateVisual(); }
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnGotKeyboardFocus(e); InvalidateVisual(); }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnLostKeyboardFocus(e); InvalidateVisual(); }
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
