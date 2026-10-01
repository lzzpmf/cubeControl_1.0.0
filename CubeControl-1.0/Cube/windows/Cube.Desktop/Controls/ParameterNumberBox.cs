using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Controls;

// Keep the WPF UI 4.3.0 template and commit semantics, but reject invalid hardware values
// instead of silently clamping them. Text edits never bind directly to the device.
public sealed class ParameterNumberBox : NumberBox
{
    public static readonly DependencyProperty InputErrorProperty = DependencyProperty.Register(
        nameof(InputError), typeof(string), typeof(ParameterNumberBox), new PropertyMetadata(""));
    public string InputError { get => (string)GetValue(InputErrorProperty); private set => SetValue(InputErrorProperty, value); }
    private readonly DispatcherTimer idle = new() { Interval = TimeSpan.FromMilliseconds(400) };
    private bool gesture;

    public ParameterNumberBox()
    {
        SetResourceReference(StyleProperty, "ParameterNumberStyle");
        AcceptsExpression = false; MaxDecimalPlaces = 0; SmallChange = 1; LargeChange = 5;
        SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Hidden;
        idle.Tick += (_, _) => EndGesture();
        ValueChanged += (_, _) => { if (IsKeyboardFocusWithin) BeginGesture(); };
        Unloaded += (_, _) => EndGesture();
    }

    internal bool ValidateDraft()
    {
        if (double.TryParse(Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var value)
            && double.IsFinite(value) && value == Math.Truncate(value) && value >= Minimum && value <= Maximum)
        { InputError = ""; return true; }
        InputError = $"Faixa válida: {Minimum:0}–{Maximum:0} (inteiros).";
        SetCurrentValue(TextProperty, Value?.ToString("0", CultureInfo.CurrentCulture) ?? Minimum.ToString("0", CultureInfo.CurrentCulture));
        return false;
    }

    private void BeginGesture()
    {
        if (!gesture) { gesture = true; RaiseEvent(new RoutedEventArgs(Knob.GestureStartedEvent, this)); }
        idle.Stop(); idle.Start();
    }
    private void EndGesture()
    {
        idle.Stop();
        if (gesture) { gesture = false; RaiseEvent(new RoutedEventArgs(Knob.GestureEndedEvent, this)); }
    }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Up or Key.Down or Key.PageUp or Key.PageDown)
        {
            if (!ValidateDraft()) { e.Handled = true; return; }
            BeginGesture();
        }
        base.OnPreviewKeyDown(e);
    }
    protected override void OnPreviewKeyUp(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (!ValidateDraft()) { e.Handled = true; return; }
            BeginGesture();
        }
        if (e.Key == Key.Escape) InputError = "";
        base.OnPreviewKeyUp(e);
    }
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        for (DependencyObject? node = e.OriginalSource as DependencyObject; node != null && node != this; node = VisualTreeHelper.GetParent(node))
        {
            if (node is not ButtonBase) continue;
            if (!ValidateDraft()) { e.Handled = true; return; }
            BeginGesture(); break;
        }
        base.OnPreviewMouseDown(e);
    }
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        ValidateDraft(); BeginGesture();
        base.OnLostFocus(e);
        EndGesture();
    }
    // NumberBox 4.3.0 validates the previous draft on paste. Pasting is an edit,
    // not a commit: allow the standard TextBox paste and validate on Enter/blur.
    protected override void OnClipboardPaste(object sender, DataObjectPastingEventArgs e) { }
}
