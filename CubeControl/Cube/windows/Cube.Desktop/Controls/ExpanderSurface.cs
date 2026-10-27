using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cube.Desktop.Controls;

// WPF UI 4.3 gives these two template borders their own thicknesses.
// Scope the adjustment to the standard Expander, retaining its native template,
// expansion directions, animation, automation peer and header bindings.
internal static class ExpanderSurface
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(ExpanderSurface), new PropertyMetadata(false, Changed));
    public static bool GetEnabled(DependencyObject value) => (bool)value.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject value, bool enabled) => value.SetValue(EnabledProperty, enabled);
    private static readonly DependencyPropertyDescriptor TemplateProperty =
        DependencyPropertyDescriptor.FromProperty(Control.TemplateProperty, typeof(Expander));

    private static void Changed(DependencyObject value, DependencyPropertyChangedEventArgs args)
    {
        if (value is not Expander control) return;
        control.Loaded -= Loaded;
        control.Unloaded -= Unloaded;
        TemplateProperty.RemoveValueChanged(control, Apply);
        if (!(bool)args.NewValue) return;
        control.Loaded += Loaded;
        control.Unloaded += Unloaded;
        if (control.IsLoaded) Loaded(control, new RoutedEventArgs());
    }

    private static void Loaded(object sender, RoutedEventArgs args)
    {
        TemplateProperty.RemoveValueChanged(sender, Apply);
        TemplateProperty.AddValueChanged(sender, Apply);
        Apply(sender, EventArgs.Empty);
    }
    private static void Unloaded(object sender, RoutedEventArgs args) => TemplateProperty.RemoveValueChanged(sender, Apply);
    private static void Apply(object? sender, EventArgs args)
    {
        if (sender is not Expander control) return;
        control.ApplyTemplate();
        foreach (string name in new[] { "ToggleButtonBorder", "ContentPresenterBorder" })
            if (control.Template?.FindName(name, control) is Border border)
                border.BorderThickness = new Thickness(0);
    }
}
