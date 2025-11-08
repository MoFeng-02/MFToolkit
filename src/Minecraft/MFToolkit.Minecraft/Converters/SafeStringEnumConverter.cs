using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Converters;

public class SafeStringEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    // 缓存：字符串（忽略大小写）→ 枚举值的映射
    private readonly Dictionary<string, T> _stringToEnumCache;
    // 缓存：枚举值 → 序列化字符串（优先用JsonPropertyName）
    private readonly Dictionary<T, string> _enumToStringCache;

    public SafeStringEnumConverter()
    {
        var enumType = typeof(T);
        var members = enumType.GetMembers(BindingFlags.Public | BindingFlags.Static);

        _stringToEnumCache = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        _enumToStringCache = new Dictionary<T, string>();

        foreach (var member in members)
        {
            var jsonProperty = member.GetCustomAttribute<JsonPropertyNameAttribute>();
            var stringValue = jsonProperty?.Name ?? member.Name;

            if (Enum.TryParse(enumType, member.Name, out var enumValue))
            {
                var value = (T)enumValue;
                _stringToEnumCache.TryAdd(stringValue, value);
                _enumToStringCache.TryAdd(value, stringValue);
            }
        }
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 处理null值（JSON中显式为null）
        if (reader.TokenType == JsonTokenType.Null)
        {
            // 枚举是值类型，无法为null，返回默认值
            return default;
        }

        // 处理字符串类型（包括空字符串）
        if (reader.TokenType == JsonTokenType.String)
        {
            var enumString = reader.GetString();
            // 防御空字符串（避免传入""导致的查找失败）
            if (!string.IsNullOrEmpty(enumString) && 
                _stringToEnumCache.TryGetValue(enumString, out var result))
            {
                return result;
            }
        }

        // 非字符串类型（如数字、对象等）或无法匹配时，返回默认值
        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // 枚举是值类型，不可能为null，直接从缓存获取字符串
        if (_enumToStringCache.TryGetValue(value, out var stringValue))
        {
            writer.WriteStringValue(stringValue);
        }
        else
        {
            // 兜底：理论上不会触发（枚举值必在缓存中）
            writer.WriteStringValue(value.ToString());
        }
    }
}