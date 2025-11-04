using System.Text.Json.Serialization;
using System.Text.Json;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 规则基于的参数
/// </summary>
public class RuleBasedArgument
{
    /// <summary>
    /// 参数值
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; set; }
    
    /// <summary>
    /// 规则列表
    /// </summary>
    [JsonPropertyName("rules")]
    public required List<Rule> Rules { get; set; }
    
    /// <summary>
    /// 检查参数是否适用于当前环境
    /// </summary>
    /// <param name="os">操作系统</param>
    /// <param name="features">特性</param>
    /// <returns>是否适用</returns>
    public bool Matches(string os, Dictionary<string, bool>? features = null)
    {
        foreach (var rule in Rules)
        {
            if (rule.Matches(os, features))
            {
                return rule.Action == RuleAction.Allow;
            }
        }
        
        // 默认拒绝
        return false;
    }
}

/// <summary>
/// 参数列表
/// </summary>
public class Arguments
{
    /// <summary>
    /// JVM参数列表
    /// </summary>
    [JsonPropertyName("jvm")]
    [JsonConverter(typeof(ArgumentListConverter))]
    public required List<object> Jvm { get; set; }
    
    /// <summary>
    /// 游戏参数列表
    /// </summary>
    [JsonPropertyName("game")]
    [JsonConverter(typeof(ArgumentListConverter))]
    public required List<object> Game { get; set; }
}

/// <summary>
/// 参数列表转换器
/// </summary>
public class ArgumentListConverter : JsonConverter<List<object>>
{
    /// <inheritdoc />
    public override List<object> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new List<object>();
        
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array");
            
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;
                
            if (reader.TokenType == JsonTokenType.String)
            {
                result.Add(reader.GetString()!);
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                var ruleArg = JsonSerializer.Deserialize<RuleBasedArgument>(ref reader, options);
                if (ruleArg != null)
                    result.Add(ruleArg);
            }
        }
        
        return result;
    }
    
    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, List<object> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        
        foreach (var item in value)
        {
            if (item is string str)
            {
                writer.WriteStringValue(str);
            }
            else if (item is RuleBasedArgument ruleArg)
            {
                JsonSerializer.Serialize(writer, ruleArg, options);
            }
        }
        
        writer.WriteEndArray();
    }
}
