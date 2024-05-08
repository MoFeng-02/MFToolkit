using System.Globalization;
using System.Text.RegularExpressions;

namespace MFToolkit.Maui.Converters;
public partial class StringToDoubleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return null;
        var match = MathRegex().Match(str);
        if (double.TryParse(match.Success ? match.Value : str, out double number))
        {
            return number;
        }
        return null; // 或返回 default(double?)
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double number)
            return number.ToString();

        return null;
    }

    //[GeneratedRegex("[^0-9]", RegexOptions.None)]
    [GeneratedRegex(@"^-?[0-9]*(?:\.[0-9]*)?$", RegexOptions.None)]
    private static partial Regex MathRegex();
}
