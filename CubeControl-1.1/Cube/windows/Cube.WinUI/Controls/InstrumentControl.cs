using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Path = Microsoft.UI.Xaml.Shapes.Path;
using Windows.Foundation;
using Windows.System;
using Windows.UI.ViewManagement;
using Cube.WinUI.Appearance;

namespace Cube.WinUI.Controls;

/// <summary>Musical range editor. Model updates only repaint; input is emitted separately.</summary>
public sealed class InstrumentControl : UserControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(InstrumentControl), new PropertyMetadata(0d, Changed));
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get; set; }
    public double Maximum { get; set; } = 100;
    public double Increment { get; set; } = 1;
    public bool FineAdjustment { get; set; }
    public bool IsFader { get; set; }
    public double WheelSpeedMultiplier { get; set; } = 1;
    public event EventHandler<double>? ValueEdited;
    public event EventHandler? GestureStarted;
    public event EventHandler? GestureEnded;
    public event EventHandler? RestoreRequested;
    private readonly Grid surface = new();
    private readonly Canvas drawing = new() { Width = 112, Height = 112, IsHitTestVisible = false };
    private readonly Border focus = new() { BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(6), IsHitTestVisible = false, Visibility = Visibility.Collapsed };
    private readonly DispatcherTimer idle = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private readonly AccessibilitySettings accessibility = new();
    private readonly UISettings uiSettings = new();
    private bool active, dragging;
    private Point last;
    private double accumulator, wheel;
    private (bool Fader, bool Contrast, ElementTheme Theme, Cube.Desktop.Appearance.ControlFinish Finish)? visualKind;
    private Path? activeArc;
    private Line? marker, faderRange;
    private FrameworkElement? faderHandle;
    private RotateTransform? rotation;
    public InstrumentControl()
    {
        Width = 112; Height = 112; IsTabStop = true; UseSystemFocusVisuals = true;
        surface.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        surface.Children.Add(drawing); surface.Children.Add(focus); Content = surface;
        idle.Tick += (_, _) => EndGesture();
        Loaded += (_, _) => { InstrumentAppearance.Changed += AppearanceChanged; uiSettings.ColorValuesChanged += ContrastChanged; Draw(); };
        Unloaded += (_, _) => { CancelGesture(); InstrumentAppearance.Changed -= AppearanceChanged; uiSettings.ColorValuesChanged -= ContrastChanged; };
        ActualThemeChanged += (_, _) => Draw();
        IsEnabledChanged += (_, _) => { if (!IsEnabled) CancelGesture(); Draw(); };
        GotFocus += (_, _) =>
        {
            focus.BorderBrush = Brush("SystemControlHighlightAccentBrush"); focus.Visibility = Visibility.Visible;
            if (FocusState != FocusState.Pointer) StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = .5 });
        };
        LostFocus += (_, _) => { focus.Visibility = Visibility.Collapsed; CancelGesture(); };
        PointerPressed += Pressed; PointerMoved += Moved;
        PointerReleased += (_, e) => { if (!dragging) return; dragging = false; ReleasePointerCaptures(); EndGesture(); e.Handled = true; };
        PointerCaptureLost += (_, _) => CancelGesture(); PointerCanceled += (_, _) => CancelGesture();
        PointerWheelChanged += Wheel;
        DoubleTapped += (_, e) => { Restore(); e.Handled = true; };
        KeyDown += Key;
    }
    private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (InstrumentControl)d; control.Draw();
        if (FrameworkElementAutomationPeer.FromElement(control) is InstrumentPeer peer)
            peer.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
    }
    private void AppearanceChanged(object? sender, EventArgs e) { visualKind = null; Draw(); }
    private void ContrastChanged(UISettings sender, object args) => DispatcherQueue.TryEnqueue(() => { visualKind = null; Draw(); });
    private Brush Brush(string key)
    {
        if (accessibility.HighContrast)
            return new SolidColorBrush(uiSettings.GetColorValue(key == "ControlFillColorDefaultBrush" ? UIColorType.Background : UIColorType.Foreground));
        return new SolidColorBrush(key switch
        {
            "ControlFillColorDefaultBrush" => ActualTheme == ElementTheme.Light ? Microsoft.UI.ColorHelper.FromArgb(255, 245, 246, 244) : Microsoft.UI.ColorHelper.FromArgb(255, 34, 40, 45),
            "SystemControlHighlightAccentBrush" => ActualTheme == ElementTheme.Light ? Microsoft.UI.ColorHelper.FromArgb(255, 33, 102, 83) : Microsoft.UI.ColorHelper.FromArgb(255, 141, 185, 172),
            _ => ActualTheme == ElementTheme.Light ? Microsoft.UI.ColorHelper.FromArgb(255, 89, 97, 106) : Microsoft.UI.ColorHelper.FromArgb(255, 157, 170, 180)
        });
    }
    private void Draw()
    {
        double ratio = Math.Clamp((Value - Minimum) / Math.Max(double.Epsilon, Maximum - Minimum), 0, 1);
        if (!double.IsFinite(ratio)) ratio = 0;
        var kind = (IsFader, accessibility.HighContrast, ActualTheme, InstrumentAppearance.Finish);
        if (visualKind != kind) { BuildVisuals(); visualKind = kind; }
        drawing.Opacity = IsEnabled || accessibility.HighContrast ? 1 : .45;
        if (IsFader)
        {
            double y = 142 - ratio * 124;
            if (faderRange != null) faderRange.Y1 = y;
            if (faderHandle != null) Canvas.SetTop(faderHandle, y - 16);
        }
        else
        {
            if (activeArc != null) { activeArc.Data = ArcGeometry(Math.Max(.001, ratio * 270)); activeArc.Visibility = ratio > 0 ? Visibility.Visible : Visibility.Collapsed; }
            if (rotation != null) rotation.Angle = -135 + ratio * 270;
            double angle = (135 + ratio * 270) * Math.PI / 180;
            if (marker != null)
            {
                marker.X1 = 56 + Math.Cos(angle) * 30; marker.Y1 = 56 + Math.Sin(angle) * 30;
                marker.X2 = 56 + Math.Cos(angle) * 42; marker.Y2 = 56 + Math.Sin(angle) * 42;
            }
        }
    }
    private void BuildVisuals()
    {
        drawing.Children.Clear(); drawing.Height = IsFader ? 160 : 112; Height = drawing.Height;
        activeArc = null; marker = null; faderRange = null; faderHandle = null; rotation = null;
        var track = Brush("TextFillColorSecondaryBrush"); var accent = Brush("SystemControlHighlightAccentBrush");
        if (IsFader)
        {
            drawing.Children.Add(new Line { X1 = 56, X2 = 56, Y1 = 18, Y2 = 142, Stroke = track, StrokeThickness = 5 });
            for (int i = 0; i <= 10; i++) drawing.Children.Add(new Line { X1 = 30, X2 = i == 5 ? 44 : 39, Y1 = 18 + i * 12.4, Y2 = 18 + i * 12.4, Stroke = track, StrokeThickness = 1 });
            faderRange = new Line { X1 = 56, X2 = 56, Y1 = 142, Y2 = 142, Stroke = accent, StrokeThickness = 3 };
            drawing.Children.Add(faderRange);
            faderHandle = accessibility.HighContrast ? new Border { Width = 42, Height = 22, Background = track, BorderBrush = accent, BorderThickness = new Thickness(2) } : new Image { Source = InstrumentAppearance.Face("fader", ActualTheme), Width = 46, Height = 32, Stretch = Stretch.Uniform };
            Canvas.SetLeft(faderHandle, 33); drawing.Children.Add(faderHandle);
        }
        else
        {
            drawing.Children.Add(Arc(270, track)); activeArc = Arc(.001, accent); drawing.Children.Add(activeArc);
            if (!accessibility.HighContrast)
            {
                rotation = new RotateTransform();
                var face = new Image { Source = InstrumentAppearance.Face("knob", ActualTheme), Width = 84, Height = 84, Stretch = Stretch.Uniform, RenderTransformOrigin = new Point(.5, InstrumentAppearance.Pivot), RenderTransform = rotation };
                Canvas.SetLeft(face, 14); Canvas.SetTop(face, 56 - 84 * InstrumentAppearance.Pivot); drawing.Children.Add(face);
            }
            else
            {
                var face = new Ellipse { Width = 76, Height = 76, Fill = Brush("ControlFillColorDefaultBrush"), Stroke = track, StrokeThickness = 2 };
                Canvas.SetLeft(face, 18); Canvas.SetTop(face, 18); drawing.Children.Add(face);
            }
            marker = new Line { Stroke = accent, StrokeThickness = 3 }; drawing.Children.Add(marker);
        }
    }
    private static Path Arc(double degrees, Brush brush) => new() { Data = ArcGeometry(degrees), Stroke = brush, StrokeThickness = 3 };
    private static PathGeometry ArcGeometry(double degrees)
    {
        Point At(double angle) => new(56 + Math.Cos(angle * Math.PI / 180) * 49, 56 + Math.Sin(angle * Math.PI / 180) * 49);
        var figure = new PathFigure { StartPoint = At(135), IsClosed = false };
        figure.Segments.Add(new ArcSegment { Point = At(135 + degrees), Size = new Size(49, 49), IsLargeArc = degrees > 180, SweepDirection = SweepDirection.Clockwise });
        var geometry = new PathGeometry(); geometry.Figures.Add(figure);
        return geometry;
    }
    private bool Fine => FineAdjustment && Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
    private void BeginGesture(bool timed)
    {
        if (!active) { active = true; GestureStarted?.Invoke(this, EventArgs.Empty); }
        idle.Stop(); if (timed) idle.Start();
    }
    private void EndGesture() { idle.Stop(); if (!active) return; active = false; GestureEnded?.Invoke(this, EventArgs.Empty); }
    internal void CancelGesture() { dragging = false; wheel = 0; ReleasePointerCaptures(); EndGesture(); }
    private void Pressed(object sender, PointerRoutedEventArgs e)
    {
        if (!IsEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        Focus(FocusState.Pointer); last = e.GetCurrentPoint(this).Position; accumulator = Value;
        dragging = CapturePointer(e.Pointer); App.Trace("Instrument capture " + dragging); if (dragging) BeginGesture(false); e.Handled = true;
    }
    private void Moved(object sender, PointerRoutedEventArgs e)
    {
        if (!dragging) return;
        var now = e.GetCurrentPoint(this).Position;
        accumulator = Math.Clamp(accumulator + (last.Y - now.Y + (IsFader ? 0 : now.X - last.X)) * (Maximum - Minimum) / (IsFader ? 124 : 160) * (Fine ? .1 : 1), Minimum, Maximum);
        last = now; Commit(accumulator); App.Trace("Instrument drag " + accumulator); e.Handled = true;
    }
    private void Wheel(object sender, PointerRoutedEventArgs e)
    {
        if (!IsEnabled) return;
        Focus(FocusState.Pointer);
        int delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
        if ((Value <= Minimum && delta < 0) || (Value >= Maximum && delta > 0)) { wheel = 0; e.Handled = true; return; }
        wheel += delta * WheelSpeedMultiplier;
        int steps = (int)(wheel / 120); wheel -= steps * 120;
        if (steps != 0) { BeginGesture(true); Commit(Value + steps * Increment * (Fine ? .1 : 1)); }
        e.Handled = true;
    }
    private void Key(object sender, KeyRoutedEventArgs e)
    {
        if (!IsEnabled) return;
        if (e.Key == VirtualKey.Back) { Restore(); e.Handled = true; return; }
        double? next = e.Key switch { VirtualKey.Left or VirtualKey.Down => Value - Increment * (Fine ? .1 : 1), VirtualKey.Right or VirtualKey.Up => Value + Increment * (Fine ? .1 : 1), VirtualKey.Home => Minimum, VirtualKey.End => Maximum, VirtualKey.PageUp => Value + Increment * 10, VirtualKey.PageDown => Value - Increment * 10, _ => null };
        if (next is not double value) return;
        BeginGesture(true); Commit(value); e.Handled = true;
    }
    private void Restore() { if (!IsEnabled) return; CancelGesture(); RestoreRequested?.Invoke(this, EventArgs.Empty); }
    private void Commit(double value)
    {
        if (!double.IsFinite(value)) return;
        double step = Increment * (Fine ? .1 : 1);
        double next = Math.Clamp(Minimum + Math.Round((value - Minimum) / step, MidpointRounding.AwayFromZero) * step, Minimum, Maximum);
        if (Math.Abs(next - Value) > 1e-10) ValueEdited?.Invoke(this, next);
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new InstrumentPeer(this);
    private sealed class InstrumentPeer(InstrumentControl owner) : FrameworkElementAutomationPeer(owner), IRangeValueProvider, IScrollItemProvider
    {
        protected override string GetClassNameCore() => owner.IsFader ? "CubeFader" : "CubeKnob";
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;
        protected override void SetFocusCore()
        {
            base.SetFocusCore();
            owner.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = .5 });
        }
        protected override object GetPatternCore(PatternInterface pattern) => pattern is PatternInterface.RangeValue or PatternInterface.ScrollItem ? this : base.GetPatternCore(pattern);
        public void ScrollIntoView() => owner.StartBringIntoView(new BringIntoViewOptions { AnimationDesired = false, VerticalAlignmentRatio = .5 });
        public bool IsReadOnly => !owner.IsEnabled;
        public double LargeChange => owner.Increment * 10;
        public double SmallChange => owner.Increment;
        public double Maximum => owner.Maximum;
        public double Minimum => owner.Minimum;
        public double Value => owner.Value;
        public void SetValue(double value)
        {
            if (IsReadOnly) throw new InvalidOperationException("Control disabled.");
            if (!double.IsFinite(value) || value < Minimum || value > Maximum) throw new ArgumentOutOfRangeException(nameof(value));
            owner.BeginGesture(false); try { owner.Commit(value); } finally { owner.EndGesture(); }
        }
    }
}
