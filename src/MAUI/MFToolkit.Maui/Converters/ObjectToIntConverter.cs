using System.Globalization;
using System.Text.RegularExpressions;
using MFToolkit.Extensions;

namespace MFToolkit.Maui.Converters;
public partial class ObjectToIntConverter : IValueConverter
{

    [GeneratedRegex("[^0-9]", RegexOptions.None)]
    private static partial Regex MathRegex();
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return 0;
        string reValue = value.ToString() ?? "0";
        try
        {
            var result = MathRegex().Replace(reValue, "");
            return result.ToInt32();
        }
        catch
        {
            return 0;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
