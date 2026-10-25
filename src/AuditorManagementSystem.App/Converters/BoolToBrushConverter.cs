using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AuditorManagementSystem.App.Converters;

/// <summary>
/// Converts a boolean to a Brush: true -> white (visible/active), false -> gray (hidden/disabled).
/// </summary>
public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool visible = value is bool b && b;
        return visible ? Brushes.White : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
