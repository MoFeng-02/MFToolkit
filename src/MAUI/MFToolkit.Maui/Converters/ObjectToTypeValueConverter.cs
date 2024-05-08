using System.Globalization;

namespace MFToolkit.Maui.Converters;
public class ObjectToTypeValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var result = System.Convert.ChangeType(value, targetType);
            return result;
        }
        catch (Exception)
        {
            return value;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
