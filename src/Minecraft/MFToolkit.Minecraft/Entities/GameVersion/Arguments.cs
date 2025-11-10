// Arguments.cs
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 启动参数容器
/// </summary>
public class Arguments
{
    /// <summary>
    /// JVM 参数
    /// </summary>
    [JsonPropertyName("jvm")]
    public List<object> Jvm { get; set; } = [];

    /// <summary>
    /// 游戏参数
    /// </summary>
    [JsonPropertyName("game")]
    public List<object> Game { get; set; } = [];
}

/// <summary>
/// 参数列表转换器（处理字符串和规则参数混合的情况）
/// </summary>
public class ArgumentListConverter : JsonConverter<List<object>>
{
    public override List<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<object>();

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected array");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    list.Add(reader.GetString() ?? string.Empty);
                    break;

                case JsonTokenType.StartObject:
                    var argument = JsonSerializer.Deserialize<RuleBasedArgument>(ref reader, options);
                    if (argument != null)
                        list.Add(argument);
                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<object> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            switch (item)
            {
                case string str:
                    writer.WriteStringValue(str);
                    break;
                case RuleBasedArgument ruleArg:
                    JsonSerializer.Serialize(writer, ruleArg, options);
                    break;
            }
        }

        writer.WriteEndArray();
    }
}

/// <summary>
/// 基于规则的参数
/// </summary>
public class RuleBasedArgument
{
    /// <summary>
    /// 参数值
    /// </summary>
    [JsonPropertyName("value")]
    [JsonConverter(typeof(ArgumentValueConverter))]
    public object Value { get; set; } = string.Empty;

    /// <summary>
    /// 适用规则
    /// </summary>
    [JsonPropertyName("rules")]
    public List<Rule> Rules { get; set; } = new();

    /// <summary>
    /// 检查参数是否适用于环境
    /// </summary>
    public bool Matches(string os, Dictionary<string, bool>? features = null)
    {
        if (Rules.Count == 0)
            return true;

        foreach (var rule in Rules)
        {
            if (rule.Matches(os, features))
            {
                return rule.Action == RuleAction.Allow;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取参数值列表
    /// </summary>
    public IEnumerable<string> GetValues()
    {
        return Value switch
        {
            string str => new[] { str },
            List<string> list => list,
            _ => Array.Empty<string>()
        };
    }
}

/// <summary>
/// 参数值转换器（处理字符串和字符串数组）
/// </summary>
public class ArgumentValueConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? string.Empty,
            JsonTokenType.StartArray => ReadStringArray(ref reader),
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    private static List<string> ReadStringArray(ref Utf8JsonReader reader)
    {
        var list = new List<string>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            if (reader.TokenType == JsonTokenType.String)
                list.Add(reader.GetString() ?? string.Empty);
            else
                reader.Skip();
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case string str:
                writer.WriteStringValue(str);
                break;
            case List<string> list:
                writer.WriteStartArray();
                foreach (var item in list)
                    writer.WriteStringValue(item);
                writer.WriteEndArray();
                break;
            default:
                writer.WriteNullValue();
                break;
        }
    }
}