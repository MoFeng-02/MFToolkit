using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Versions;

/// <summary>
/// Minecraft版本清单
/// </summary>
public class VersionManifest
{
    /// <summary>
    /// 最新版本信息
    /// </summary>
    [JsonPropertyName("latest")]
    public required LatestVersionInfo Latest { get; set; }
    
    /// <summary>
    /// 所有版本列表
    /// </summary>
    [JsonPropertyName("versions")]
    public required IReadOnlyList<VersionInfo> Versions { get; set; }
}
