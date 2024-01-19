using System.Globalization;

namespace MFToolkit.Maui.Converters;
/// <summary>
/// bool 取反
/// </summary>
internal class BoolToInverseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool reValue) return value;
        return !reValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool reValue) return value;
        return !reValue;
    }
}
