using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;

namespace Cube.Desktop.Studio.Controls;

/// <summary>A native rotary input using the supplied knob artwork.</summary>
public sealed class KnobControl : FrameworkElement
{
    private static readonly DependencyProperty ThemeRevisionProperty = DependencyProperty.Register("ThemeRevision", typeof(int), typeof(KnobControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    private bool _dragging;
    private double _lastY;
    private double _dragValue;
    private string? _generatedTooltip;

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(KnobControl),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
            FrameworkPropertyMetadataOptions.AffectsRender, ValueChangedCallback, CoerceValue));
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(double), typeof(KnobControl), new FrameworkPropertyMetadata(-61d, BoundsChanged));
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(KnobControl), new FrameworkPropertyMetadata(6d, BoundsChanged));
    public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register(
        nameof(DefaultValue), typeof(double), typeof(KnobControl), new PropertyMetadata(0d));
    public static readonly DependencyProperty IsPanProperty = DependencyProperty.Register(
        nameof(IsPan), typeof(bool), typeof(KnobControl), new PropertyMetadata(false, PresentationChanged));

    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(KnobControl));
    public static readonly RoutedEvent EditStartedEvent = EventManager.RegisterRoutedEvent(
        nameof(EditStarted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(KnobControl));
    public static readonly RoutedEvent EditCompletedEvent = EventManager.RegisterRoutedEvent(
        nameof(EditCompleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(KnobControl));

    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double DefaultValue { get => (double)GetValue(DefaultValueProperty); set => SetValue(DefaultValueProperty, value); }
    public bool IsPan { get => (bool)GetValue(IsPanProperty); set => SetValue(IsPanProperty, value); }
    public event RoutedPropertyChangedEventHandler<double> ValueChanged { add => AddHandler(ValueChangedEvent, value); remove => RemoveHandler(ValueChangedEvent, value); }
    public event RoutedEventHandler EditStarted { add => AddHandler(EditStartedEvent, value); remove => RemoveHandler(EditStartedEvent, value); }
    public event RoutedEventHandler EditCompleted { add => AddHandler(EditCompletedEvent, value); remove => RemoveHandler(EditCompletedEvent, value); }

    public KnobControl()
    {
        SetResourceReference(ThemeRevisionProperty, "CubeThemeRevision");
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        Unloaded += (_, _) => { if (_dragging) FinishDrag(); };
        Width = 30;
        Height = 30;
        Focusable = true;
        Cursor = Cursors.SizeNS;
        IsEnabledChanged += (_, _) => InvalidateVisual();
        Loaded += (_, _) => UpdateTooltip();
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new KnobAutomationPeer(this);
    protected override Size MeasureOverride(Size availableSize) => new(30, 30);

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        var diameter = Math.Max(0, Math.Min(ActualWidth, ActualHeight) - 4);
        if (diameter <= 0) return;
        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        var face = new Rect(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
        var angle = -135 + (Value - Minimum) / Math.Max(.001, Maximum - Minimum) * 270;
        var accent = TryFindResource("CubeAccent") as Brush ?? SystemColors.HighlightBrush;
        var border = TryFindResource("CubeKnobTrack") as Brush ?? SystemColors.WindowTextBrush;
        dc.PushOpacity(IsEnabled || ThemeService.Instance.IsHighContrast ? 1 : .45);
        dc.PushTransform(new RotateTransform(angle, center.X, center.Y));
        if (!ThemeService.Instance.IsHighContrast)
        {
            var artwork = ControlArtwork.KnobFace;
            var pivot = TryFindResource("CubeControlKnobPivotY") is double value ? value : .5;
            var extent = Math.Max(artwork.Width / 2, artwork.Height * Math.Max(pivot, 1 - pivot));
            var scale = (diameter / 2 - 2) / extent;
            dc.DrawImage(artwork, new Rect(center.X - artwork.Width * scale / 2, center.Y - artwork.Height * pivot * scale,
                artwork.Width * scale, artwork.Height * scale));
            if (TryFindResource("CubeControlExternalKnobMarker") is true)
                dc.DrawLine(new Pen(border, 2), new Point(center.X, face.Top + 1), new Point(center.X, face.Top + 5));
        }
        else
        {
            dc.DrawEllipse(SystemColors.WindowBrush, new Pen(border, 1), center, diameter / 2 - 1, diameter / 2 - 1);
            dc.DrawLine(new Pen(accent, 2), new Point(center.X, face.Top + 3), new Point(center.X, center.Y - 2));
        }
        dc.Pop(); dc.Pop();
        if (IsKeyboardFocused)
        {
            var brush = accent;
            dc.DrawEllipse(null, new Pen(brush, 1) { DashStyle = DashStyles.Dot }, center, diameter / 2 + 2, diameter / 2 + 2);
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (!IsEnabled) return;
        Focus();
        if (e.ClickCount == 2)
        {
            RunEdit(() => SetUserValue(DefaultValue));
        }
        else
        {
            _dragging = true;
            _lastY = e.GetPosition(this).Y;
            _dragValue = Value;
            CaptureMouse();
            RaiseEvent(new RoutedEventArgs(EditStartedEvent, this));
        }
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_dragging) return;
        var y = e.GetPosition(this).Y;
        var fine = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? .1 : 1;
        _dragValue = Math.Clamp(_dragValue + (_lastY - y) * (Maximum - Minimum) / 150 * fine, Minimum, Math.Max(Minimum, Maximum));
        _lastY = y;
        SetUserValue(_dragValue);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!_dragging) return;
        FinishDrag();
        e.Handled = true;
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        if (_dragging) FinishDrag();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        if (!IsEnabled || Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) return;
        Focus();
        RunEdit(() => SetUserValue(Value + Math.Sign(e.Delta) * KeyboardStep()));
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled) return;
        var value = e.Key switch
        {
            Key.Up or Key.Right => Value + KeyboardStep(),
            Key.Down or Key.Left => Value - KeyboardStep(),
            Key.Home => Minimum,
            Key.End => Maximum,
            _ => double.NaN
        };
        if (!double.IsNaN(value))
        {
            RunEdit(() => SetUserValue(value));
            e.Handled = true;
        }
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnGotKeyboardFocus(e); InvalidateVisual(); }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnLostKeyboardFocus(e); InvalidateVisual(); }

    private double KeyboardStep() => IsPan ? (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? 1 : 5) : (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? .1 : 1);
    private void SetUserValue(double value) => SetCurrentValue(ValueProperty, Math.Clamp(Math.Round(value, IsPan ? 0 : 1), Minimum, Math.Max(Minimum, Maximum)));
    private void RunEdit(Action action)
    {
        RaiseEvent(new RoutedEventArgs(EditStartedEvent, this));
        action();
        RaiseEvent(new RoutedEventArgs(EditCompletedEvent, this));
    }
    private void FinishDrag()
    {
        _dragging = false;
        if (IsMouseCaptured) ReleaseMouseCapture();
        RaiseEvent(new RoutedEventArgs(EditCompletedEvent, this));
    }

    public string FormatValue() => IsPan
        ? Value == 0 ? "C" : (Value < 0 ? "L " : "R ") + Math.Abs(Value).ToString("0", CultureInfo.CurrentCulture)
        : Value <= -61 ? "−∞ dB" : (Value > 0 ? "+" : "") + Value.ToString("0.#", CultureInfo.CurrentCulture) + " dB";

    private void UpdateTooltip()
    {
        if (ToolTip is null || Equals(ToolTip, _generatedTooltip))
        {
            _generatedTooltip = FormatValue() + " · " + LanguageService.T("arraste ↑↓ · Shift: fino · duplo clique: restaurar");
            SetCurrentValue(ToolTipProperty, _generatedTooltip);
        }
    }
    private static object CoerceValue(DependencyObject d, object value)
    {
        var knob = (KnobControl)d;
        var number = (double)value;
        return Math.Clamp(double.IsFinite(number) ? number : 0, knob.Minimum, Math.Max(knob.Minimum, knob.Maximum));
    }
    private static void BoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.CoerceValue(ValueProperty);
        ((KnobControl)d).InvalidateVisual();
    }
    private static void PresentationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((KnobControl)d).UpdateTooltip();
    private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var knob = (KnobControl)d;
        knob.UpdateTooltip();
        knob.RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, ValueChangedEvent));
        if (UIElementAutomationPeer.FromElement(knob) is KnobAutomationPeer peer)
            peer.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
    }
    private sealed class KnobAutomationPeer(KnobControl owner) : FrameworkElementAutomationPeer(owner), IRangeValueProvider
    {
        protected override string GetClassNameCore() => nameof(KnobControl);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;
        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            return string.IsNullOrEmpty(name) ? (owner.IsPan ? "Pan" : "Volume") : name;
        }
        public override object? GetPattern(PatternInterface patternInterface) => patternInterface == PatternInterface.RangeValue ? this : base.GetPattern(patternInterface);
        bool IRangeValueProvider.IsReadOnly => !owner.IsEnabled;
        double IRangeValueProvider.LargeChange => (owner.Maximum - owner.Minimum) / 10;
        double IRangeValueProvider.SmallChange => owner.IsPan ? 1 : .1;
        double IRangeValueProvider.Maximum => owner.Maximum;
        double IRangeValueProvider.Minimum => owner.Minimum;
        double IRangeValueProvider.Value => owner.Value;
        void IRangeValueProvider.SetValue(double value)
        {
            if (!owner.IsEnabled) throw new ElementNotEnabledException();
            if (value < owner.Minimum || value > owner.Maximum || !double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            owner.RunEdit(() => owner.SetUserValue(value));
        }
    }
}
