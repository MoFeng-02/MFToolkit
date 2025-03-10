using System.Globalization;
using Avalonia.Data.Converters;

namespace MFToolkit.Avaloniaui.Converters;

/// <summary>
/// bool 取反
/// </summary>
public class BoolToInverseConverter : IValueConverter
{
    /// <summary>
    /// bool 取反
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool reValue) return value;
        return !reValue;
    }

    /// <summary>
    /// bool 取反
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool reValue) return value;
        return !reValue;
    }
}