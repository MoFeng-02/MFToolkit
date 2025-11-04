using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Versions;

/// <summary>
/// 最新版本信息
/// </summary>
public class LatestVersionInfo
{
    /// <summary>
    /// 最新正式版本
    /// </summary>
    [JsonPropertyName("release")]
    public required string Release { get; set; }
    
    /// <summary>
    /// 最新快照版本
    /// </summary>
    [JsonPropertyName("snapshot")]
    public required string Snapshot { get; set; }
}
