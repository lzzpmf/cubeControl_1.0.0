using Cube.Desktop.Localization;
using Microsoft.UI.Xaml.Data;
namespace Cube.WinUI.Platform;
internal sealed class LocalizedText : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is string text ? LanguageService.T(text) : value;
    public object ConvertBack(object value, Type targetType, object parameter, string language) => value;
}
