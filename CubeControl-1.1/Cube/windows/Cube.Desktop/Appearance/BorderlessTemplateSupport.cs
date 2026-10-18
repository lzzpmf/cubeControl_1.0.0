using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Appearance;

// Local equivalents of the internal WPF UI 4.3.0 template helpers (MIT).
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// Templates in this assembly cannot instantiate internal types from Wpf.Ui.
public sealed class ExpansionOffsetConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not double height || values[1] is not double progress)
            return 0.0;
        return height * (parameter is "negative" ? -progress : progress);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class DialogButtonMatchConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is ContentDialogButton button && parameter is ContentDialogButton expected && button == expected;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public static class ExpansionAnimation
{
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.RegisterAttached(
        "Progress", typeof(double), typeof(ExpansionAnimation),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits));

    public static double GetProgress(DependencyObject element) => (double)element.GetValue(ProgressProperty);
    public static void SetProgress(DependencyObject element, double value) => element.SetValue(ProgressProperty, value);
}
