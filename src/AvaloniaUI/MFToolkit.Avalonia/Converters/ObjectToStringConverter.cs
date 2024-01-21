using System.Globalization;
using Avalonia.Data.Converters;

namespace MFToolkit.Avaloniaui.Converters;

public class ObjectToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            return value?.ToString();
        }
        catch (Exception e)
        {
            return value;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}