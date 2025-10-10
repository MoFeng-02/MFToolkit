using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MFToolkit.Json.Converters;
public class HighPrecisionDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 直接解析字符串，兼容任何精度
        return DateTimeOffset.Parse(reader.GetString()!, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        // 保持原始精度输出
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
    }
}
