using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cube.Desktop.Controls;

// Replace only the header visual; retain the native expander template and toggle binding.
public static class ExpanderChevron
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(ExpanderChevron), new PropertyMetadata(false, Changed));
    public static bool GetEnabled(DependencyObject value) => (bool)value.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject value, bool enabled) => value.SetValue(EnabledProperty, enabled);
    static void Changed(DependencyObject value, DependencyPropertyChangedEventArgs args)
    {
        if (value is not Control control) return;
        control.Loaded -= Loaded;
        if ((bool)args.NewValue)
        {
            control.Loaded += Loaded;
            if (control.IsLoaded) Apply(control);
        }
    }
    static void Loaded(object sender, RoutedEventArgs args) => Apply((Control)sender);
    static void Apply(Control control)
    {
        control.ApplyTemplate();
        var header = FindHeader(control, control);
        if (header != null)
            header.Template = (ControlTemplate)control.FindResource("DownUpExpanderHeader");
    }
    static ToggleButton? FindHeader(DependencyObject parent, Control owner)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ToggleButton button && button.TemplatedParent == owner) return button;
            if (FindHeader(child, owner) is { } found) return found;
        }
        return null;
    }
}
