using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace MFToolkit.Avaloniaui.Converters;

public class ValueToClassesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var reValue = value?.ToString();
            Classes classes = new();
            if (!string.IsNullOrEmpty(reValue)) classes.Add(reValue!);
            return classes;
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