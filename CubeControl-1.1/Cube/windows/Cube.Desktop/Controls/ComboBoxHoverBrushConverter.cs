using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cube.Desktop.Controls;

// Explicit musical finishes belong to the closed field, not to the popup palette.
public sealed class ComboBoxHoverBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is not ComboBox owner) return DependencyProperty.UnsetValue;
        return owner.ReadLocalValue(Control.BackgroundProperty) != DependencyProperty.UnsetValue
            ? owner.Background
            : owner.TryFindResource("CubeComboFieldHover") ?? owner.Background;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
