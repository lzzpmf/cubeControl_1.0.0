using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System.Globalization;
namespace Cube.WinUI.Platform;
internal sealed class FiniteNumber : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not double number || !double.IsFinite(number)) return DependencyProperty.UnsetValue;
        var type = Nullable.GetUnderlyingType(targetType) ?? targetType;
        return type == typeof(object) ? number : System.Convert.ChangeType(number, type, CultureInfo.InvariantCulture);
    }
}
