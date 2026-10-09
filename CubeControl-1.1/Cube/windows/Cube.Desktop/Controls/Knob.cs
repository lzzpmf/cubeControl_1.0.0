using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using Cube.Desktop.Appearance;

namespace Cube.Desktop.Controls;

public sealed class Knob : FrameworkElement
{
    public static readonly DependencyProperty StepSizeProperty = DependencyProperty.Register(nameof(StepSize), typeof(double), typeof(Knob),
        new PropertyMetadata(0d, (d, _) => d.CoerceValue(ValueProperty)), value => value is double step && double.IsFinite(step) && step >= 0);
    public double StepSize { get => (double)GetValue(StepSizeProperty); set => SetValue(StepSizeProperty, value); }
    private double InputStep => StepSize > 0 ? StepSize : Fine ? .1 : 1;
    public static readonly DependencyProperty FineAdjustmentProperty = DependencyProperty.Register(nameof(FineAdjustment), typeof(bool), typeof(Knob), new PropertyMetadata(false));
    public bool FineAdjustment { get => (bool)GetValue(FineAdjustmentProperty); set => SetValue(FineAdjustmentProperty, value); }
    private bool Fine => FineAdjustment && (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
    public static readonly DependencyProperty FaceProperty = DependencyProperty.Register(nameof(Face), typeof(ImageSource), typeof(Knob), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty IsFaderProperty = DependencyProperty.Register(nameof(IsFader), typeof(bool), typeof(Knob), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
    public ImageSource? Face { get => (ImageSource?)GetValue(FaceProperty); set => SetValue(FaceProperty, value); }
    public bool IsFader { get => (bool)GetValue(IsFaderProperty); set => SetValue(IsFaderProperty, value); }
    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int), typeof(Knob), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(Knob), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) =>
    {
        if (UIElementAutomationPeer.FromElement((Knob)d) is KnobPeer peer)
            peer.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
    }, (d, value) =>
    {
        var knob = (Knob)d;
        if (knob.StepSize <= 0) return value;
        double number = (double)value;
        if (!double.IsFinite(number)) return knob.Minimum;
        return Math.Clamp(knob.Minimum + Math.Round((number - knob.Minimum) / knob.StepSize, MidpointRounding.AwayFromZero) * knob.StepSize, knob.Minimum, knob.Maximum);
    }));
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(Knob), new FrameworkPropertyMetadata(127d, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(Knob), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty WheelStepDeltaProperty = DependencyProperty.Register(nameof(WheelStepDelta), typeof(int), typeof(Knob),
        new PropertyMetadata(120, (d, _) => ((Knob)d).wheelDelta = 0), value => value is int delta && delta > 0);
    public static readonly DependencyProperty WheelSpeedMultiplierProperty = DependencyProperty.Register(nameof(WheelSpeedMultiplier), typeof(double), typeof(Knob),
        new PropertyMetadata(1d, (d, _) => ((Knob)d).wheelDelta = 0), value => value is double factor && double.IsFinite(factor) && factor > 0 && factor <= 11.5);
    public double WheelSpeedMultiplier { get => (double)GetValue(WheelSpeedMultiplierProperty); set => SetValue(WheelSpeedMultiplierProperty, value); }
    public static readonly RoutedEvent GestureStartedEvent = EventManager.RegisterRoutedEvent("GestureStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Knob));
    public static readonly RoutedEvent GestureEndedEvent = EventManager.RegisterRoutedEvent("GestureEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Knob));
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Value { get => (double)GetValue(ValueProperty); set => SetCurrentValue(ValueProperty, Math.Clamp(Math.Round(value, FineAdjustment ? 1 : 0), Minimum, Maximum)); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    // 120 is one standard wheel detent. 100 produces exactly 20% more steps,
    // retaining fractional/high-resolution wheel input without rounding each event.
    public int WheelStepDelta { get => (int)GetValue(WheelStepDeltaProperty); set => SetValue(WheelStepDeltaProperty, value); }
    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(nameof(Accent), typeof(Brush), typeof(Knob), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(141, 185, 172)), FrameworkPropertyMetadataOptions.AffectsRender));
    public Brush Accent { get => (Brush)GetValue(AccentProperty); set => SetValue(AccentProperty, value); }
    public static readonly RoutedEvent RestoreRequestedEvent = EventManager.RegisterRoutedEvent(nameof(RestoreRequested), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Knob));
    public event RoutedEventHandler RestoreRequested { add => AddHandler(RestoreRequestedEvent, value); remove => RemoveHandler(RestoreRequestedEvent, value); }
    private void RequestRestore()
    {
        wheelDelta = 0;
        if (IsMouseCaptured) ReleaseMouseCapture();
        EndGesture();
        RaiseEvent(new RoutedEventArgs(RestoreRequestedEvent, this));
    }
    private Point start;
    private double startValue;
    private decimal wheelDelta;
    private readonly DispatcherTimer idle = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private bool gestureActive;
    public Knob()
    {
        Focusable = true; FocusVisualStyle = null; Cursor = Cursors.Hand; MinWidth = 48; MinHeight = 48;
        // Smooth the official textures when reduced and rotated at UI sizes.
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
        idle.Tick += (_, _) => EndGesture();
        Unloaded += (_, _) => EndGesture();
        IsEnabledChanged += (_, _) => { if (!IsEnabled) { wheelDelta = 0; EndGesture(); } InvalidateVisual(); };
    }
    private void BeginGesture(bool timed)
    {
        if (!gestureActive) { gestureActive = true; RaiseEvent(new(GestureStartedEvent, this)); }
        idle.Stop(); if (timed) idle.Start();
    }
    private void EndGesture()
    {
        idle.Stop(); if (!gestureActive) return; gestureActive = false; RaiseEvent(new(GestureEndedEvent, this));
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new KnobPeer(this);
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var center = new Point(ActualWidth / 2, ActualHeight / 2); double radius = Math.Min(ActualWidth, ActualHeight) / 2 - 6;
        if (radius < 2) return;
        dc.PushOpacity(IsEnabled || ThemeService.Instance.IsHighContrast ? 1 : 0.35);
        var accent = !IsEnabled && ThemeService.Instance.IsHighContrast ? ThemeService.Brush("CubeDisabledText") : Accent;
        if (!ThemeService.Instance.IsHighContrast || IsFader)
        {
            double position = Math.Clamp((Value - Minimum) / Math.Max(1, Maximum - Minimum), 0, 1);
            if (IsFader)
            {
                double top = 24, bottom = ActualHeight - 24;
                dc.DrawLine(new Pen(ThemeService.Brush("CubeKnobTrack"), 5), new(center.X, top), new(center.X, bottom));
                for (int i = 0; i <= 10; i++)
                {
                    double y = top + (bottom - top) * i / 10;
                    dc.DrawLine(new Pen(i == 5 ? accent : ThemeService.Brush("CubeKnobTrack"), 1), new(center.X - 22, y), new(center.X - 12, y));
                }
                double handleY = bottom - position * (bottom - top);
                if (!ThemeService.Instance.IsHighContrast)
                {
                    dc.DrawLine(new Pen(accent, 3) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, new(center.X, bottom), new(center.X, handleY));
                    double width = Math.Min(46, ActualWidth - 8);
                    var faderFace = ControlArtwork.FaderFace;
                    double height = width * faderFace.Height / faderFace.Width;
                    dc.DrawImage(faderFace, new Rect(center.X - width / 2, handleY - height / 2, width, height));
                }
                else
                {
                    dc.DrawLine(new Pen(accent, 3) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, new(center.X, bottom), new(center.X, handleY));
                    dc.DrawRoundedRectangle(ThemeService.Brush("CubeKnobTop"),
                        new Pen(ThemeService.Brush("CubeKnobTrack"), 1), new Rect(center.X - 15, handleY - 10, 30, 20), 4, 4);
                    dc.DrawLine(new Pen(accent, 2) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, new(center.X - 8, handleY), new(center.X + 8, handleY));
                }
            }
            else
            {
                DrawArc(dc, center, radius + 1, 270, new Pen(ThemeService.Brush("CubeKnobTrack"), 3));
                if (position > 0) DrawArc(dc, center, radius + 1, position * 270, new Pen(accent, 3));
                if (StepSize > 0)
                    for (double tick = Minimum; tick <= Maximum; tick += StepSize)
                    {
                        double tickAngle = (135 + (tick - Minimum) / Math.Max(1, Maximum - Minimum) * 270) * Math.PI / 180;
                        dc.DrawLine(new Pen(accent, 2), new(center.X + Math.Cos(tickAngle) * (radius - 2), center.Y + Math.Sin(tickAngle) * (radius - 2)),
                            new(center.X + Math.Cos(tickAngle) * (radius + 4), center.Y + Math.Sin(tickAngle) * (radius + 4)));
                    }
                dc.PushTransform(new RotateTransform(-135 + position * 270, center.X, center.Y));
                var knobFace = ControlArtwork.KnobFace;
                double pivotY = TryFindResource("CubeControlKnobPivotY") is double pivot ? pivot : .5;
                double faceExtent = Math.Max(knobFace.Width / 2, knobFace.Height * Math.Max(pivotY, 1 - pivotY));
                double faceScale = (radius - 4) / faceExtent;
                dc.DrawImage(knobFace, new Rect(center.X - knobFace.Width * faceScale / 2, center.Y - knobFace.Height * pivotY * faceScale,
                    knobFace.Width * faceScale, knobFace.Height * faceScale));
                if (TryFindResource("CubeControlExternalKnobMarker") is true)
                    dc.DrawLine(new Pen(ThemeService.Brush("CubeKnobTrack"), 3) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round },
                        new Point(center.X, center.Y - radius + 2), new Point(center.X, center.Y - radius - 3));
                dc.Pop();
            }
            dc.Pop(); return;
        }
        dc.DrawEllipse(ThemeService.Brush("CubeKnobBase"), new Pen(ThemeService.Brush("CubeKnobTrack"), 2), center, radius, radius);
        double ratio = Math.Clamp((Value - Minimum) / Math.Max(1, Maximum - Minimum), 0, 1);
        double angle = (135 + ratio * 270) * Math.PI / 180;
        DrawArc(dc, center, radius + 1, 270, new Pen(ThemeService.Brush("CubeKnobTrack"), 3));
        if (ratio > 0) DrawArc(dc, center, radius + 1, ratio * 270, new Pen(accent, 3) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round });
        var end = new Point(center.X + Math.Cos(angle) * (radius - 6), center.Y + Math.Sin(angle) * (radius - 6));
        var from = new Point(center.X + Math.Cos(angle) * radius * 0.35, center.Y + Math.Sin(angle) * radius * 0.35);
        var face = new LinearGradientBrush(((SolidColorBrush)ThemeService.Brush("CubeKnobTop")).Color, ((SolidColorBrush)ThemeService.Brush("CubeKnobBottom")).Color, 90);
        dc.DrawEllipse(face, new Pen(ThemeService.Brush("CubeKnobTrack"), 1), center, radius - 5, radius - 5);
        dc.DrawLine(new Pen(accent, 3) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, from, end);
        dc.Pop();
    }
    private static void DrawArc(DrawingContext dc, Point center, double radius, double degrees, Pen pen)
    {
        Point At(double a) => new(center.X + Math.Cos(a * Math.PI / 180) * radius, center.Y + Math.Sin(a * Math.PI / 180) * radius);
        var geometry = new StreamGeometry();
        using (var context = geometry.Open()) { context.BeginFigure(At(135), false, false); context.ArcTo(At(135 + degrees), new(radius, radius), 0, degrees > 180, SweepDirection.Clockwise, true, false); }
        dc.DrawGeometry(null, pen, geometry);
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { base.OnMouseLeftButtonDown(e); if (!IsEnabled) return; Focus(); if (e.ClickCount == 2) { RequestRestore(); e.Handled = true; return; } BeginGesture(false); start = e.GetPosition(this); startValue = Value; CaptureMouse(); e.Handled = true; }
    private void Adjust(Point now)
    {
        double delta = (start.Y - now.Y + (IsFader ? 0 : now.X - start.X)) * (Maximum - Minimum) / (IsFader ? Math.Max(1, ActualHeight - 48) : 160);
        if (!FineAdjustment) { Value = startValue + delta; return; }
        if (delta == 0) return;
        startValue = Math.Clamp(startValue + delta * (Fine ? .1 : 1), Minimum, Maximum);
        start = now;
        Value = Math.Round(startValue, Fine ? 1 : 0);
    }
    protected override void OnMouseMove(MouseEventArgs e) { if (IsMouseCaptured) Adjust(e.GetPosition(this)); }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) { if (IsMouseCaptured) { Adjust(e.GetPosition(this)); ReleaseMouseCapture(); EndGesture(); } e.Handled = true; }
    protected override void OnLostMouseCapture(MouseEventArgs e) { base.OnLostMouseCapture(e); EndGesture(); }
    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (!IsEnabled) { base.OnMouseWheel(e); return; }
        if ((Value <= Minimum && e.Delta < 0) || (Value >= Maximum && e.Delta > 0)) wheelDelta = 0;
        else
        {
            wheelDelta += e.Delta * (decimal)WheelSpeedMultiplier;
            int steps = (int)(wheelDelta / WheelStepDelta);
            wheelDelta %= WheelStepDelta;
            if (steps != 0)
            {
                BeginGesture(true); Value += steps * InputStep;
                if (Value <= Minimum || Value >= Maximum) wheelDelta = 0;
            }
        }
        e.Handled = true;
    }
    protected override void OnMouseLeave(MouseEventArgs e) { base.OnMouseLeave(e); wheelDelta = 0; if (!IsMouseCaptured) EndGesture(); }
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnGotKeyboardFocus(e); InvalidateVisual(); }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnLostKeyboardFocus(e); InvalidateVisual(); }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsEnabled) return;
        if (e.Key == Key.Back) { RequestRestore(); e.Handled = true; return; }
        if (e.Key is Key.Up or Key.Right or Key.Down or Key.Left or Key.Home or Key.End) BeginGesture(true);
        switch (e.Key)
        {
            case Key.Up: case Key.Right: Value += InputStep; break;
            case Key.Down: case Key.Left: Value -= InputStep; break;
            case Key.Home: Value = Minimum; break;
            case Key.End: Value = Maximum; break;
            default: base.OnKeyDown(e); return;
        }
        e.Handled = true;
    }
    private sealed class KnobPeer(Knob owner) : FrameworkElementAutomationPeer(owner), IRangeValueProvider
    {
        protected override string GetClassNameCore() => "Knob";
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;
        public override object? GetPattern(PatternInterface patternInterface) => patternInterface == PatternInterface.RangeValue ? this : base.GetPattern(patternInterface);
        public bool IsReadOnly => !owner.IsEnabled;
        public double LargeChange => 5;
        public double SmallChange => owner.StepSize > 0 ? owner.StepSize : 1;
        public double Maximum => owner.Maximum;
        public double Minimum => owner.Minimum;
        public double Value => owner.Value;
        public void SetValue(double value)
        {
            if (IsReadOnly) throw new ElementNotEnabledException();
            if (!double.IsFinite(value) || value < Minimum || value > Maximum) throw new ArgumentOutOfRangeException(nameof(value));
            owner.BeginGesture(false);
            try { owner.Value = value; }
            finally { owner.EndGesture(); }
        }
    }
}
