using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Enums;

/// <summary>
/// Minecraft版本类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VersionType
{
    /// <summary>
    /// 正式版本
    /// </summary>
    Release,
    
    /// <summary>
    /// 快照版本
    /// </summary>
    Snapshot,
    
    /// <summary>
    /// 旧测试版
    /// </summary>
    [JsonPropertyName("old_beta")]
    OldBeta,
    
    /// <summary>
    /// 旧阿尔法版
    /// </summary>
    [JsonPropertyName("old_alpha")]
    OldAlpha
}
