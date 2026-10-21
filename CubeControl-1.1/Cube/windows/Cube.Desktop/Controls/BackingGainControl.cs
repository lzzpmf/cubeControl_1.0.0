using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Cube.Desktop.Controls;

public sealed class BackingGainControl : StackPanel
{
    public static readonly DependencyProperty GainDbProperty = DependencyProperty.Register(nameof(GainDb), typeof(double), typeof(BackingGainControl),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((BackingGainControl)d).Refresh()));
    public double GainDb { get => (double)GetValue(GainDbProperty); set => SetValue(GainDbProperty, value); }
    public static readonly DependencyProperty VerticalProperty = DependencyProperty.Register(nameof(Vertical), typeof(bool), typeof(BackingGainControl),
        new PropertyMetadata(false, (d, _) => ((BackingGainControl)d).Layout()));
    public bool Vertical { get => (bool)GetValue(VerticalProperty); set => SetValue(VerticalProperty, value); }
    private readonly BackingFader fader = new() { Minimum = -61, Maximum = 6, SmallChange = .1, LargeChange = 1, TickFrequency = 6 };
    private readonly Grid faderHost = new();
    private readonly TextBlock unity = new() { Text = "0", IsHitTestVisible = false, FontSize = 10, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, 18, 0, 0) };
    private readonly TextBox number = new() { IsReadOnly = true, TextAlignment = TextAlignment.Center, MinWidth = 68, MinHeight = 0, Height = 24, Padding = new Thickness(2), BorderThickness = new Thickness(0) };
    public BackingGainControl()
    {
        fader.SetBinding(BackingFader.ValueProperty, new Binding(nameof(GainDb)) { Source = this, Mode = BindingMode.TwoWay });
        fader.PreviewMouseLeftButtonDown += (_, e) => { if (e.ClickCount == 2 || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) { SetCurrentValue(GainDbProperty, 0d); e.Handled = true; } };
        number.MouseDoubleClick += (_, e) => { number.IsReadOnly = false; number.Text = GainDb <= -61 ? "-∞" : GainDb.ToString("0.0", CultureInfo.CurrentCulture); number.SelectAll(); e.Handled = true; };
        number.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) { Commit(); e.Handled = true; }
            else if (e.Key == Key.Escape) { number.IsReadOnly = true; Refresh(); e.Handled = true; }
            else if (e.Key == Key.F2) { number.IsReadOnly = false; number.SelectAll(); }
        };
        number.LostKeyboardFocus += (_, _) => { Commit(); number.IsReadOnly = true; Refresh(); };
        unity.SetResourceReference(TextBlock.ForegroundProperty, "CubeText");
        faderHost.Children.Add(fader); faderHost.Children.Add(unity);
        Children.Add(faderHost); Children.Add(number); Layout(); Refresh();
        Loaded += (_, _) => { string name = AutomationProperties.GetName(this); AutomationProperties.SetName(fader, name); AutomationProperties.SetName(number, name + " dB"); };
    }
    private void Commit()
    {
        if (number.IsReadOnly) return;
        string text = number.Text.Replace("dB", "").Trim();
        if (text is "-∞" or "−∞") SetCurrentValue(GainDbProperty, -61d);
        else if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out double value) && double.IsFinite(value) && value >= -60 && value <= 6)
            SetCurrentValue(GainDbProperty, value);
        else { number.SelectAll(); return; }
        number.IsReadOnly = true; Refresh();
    }
    private void Refresh() { if (number.IsReadOnly) number.Text = GainDb <= -61 ? "−∞ dB" : GainDb.ToString("0.0", CultureInfo.CurrentCulture) + " dB"; }
    private void Layout()
    {
        Orientation = Vertical ? Orientation.Vertical : Orientation.Horizontal;
        fader.Orientation = Vertical ? Orientation.Vertical : Orientation.Horizontal;
        fader.Height = Vertical ? 160 : 28; fader.Width = Vertical ? 48 : 90;
        faderHost.Width = Vertical ? 68 : 90;
        unity.Visibility = Vertical ? Visibility.Visible : Visibility.Collapsed;
        number.VerticalAlignment = VerticalAlignment.Center;
    }
}
