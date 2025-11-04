using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// Java版本信息
/// </summary>
public class JavaVersionInfo
{
    /// <summary>
    /// Java版本ID
    /// </summary>
    [JsonPropertyName("component")]
    public required string Component { get; set; }
    
    /// <summary>
    /// 主要版本号
    /// </summary>
    [JsonPropertyName("majorVersion")]
    public int MajorVersion { get; set; }
}
