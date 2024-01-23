using System.Globalization;
using Avalonia.Data.Converters;

namespace MFToolkit.Avaloniaui.Converters;

/// <summary>
/// bool 取反
/// </summary>
public class BoolToInverseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool reValue) return value;
        return !reValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}