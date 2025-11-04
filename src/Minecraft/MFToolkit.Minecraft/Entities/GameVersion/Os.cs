using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 操作系统信息
/// </summary>
public class Os
{
    /// <summary>
    /// 操作系统名称
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// 操作系统版本
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
    
    /// <summary>
    /// 架构
    /// </summary>
    [JsonPropertyName("arch")]
    public string? Arch { get; set; }
}
