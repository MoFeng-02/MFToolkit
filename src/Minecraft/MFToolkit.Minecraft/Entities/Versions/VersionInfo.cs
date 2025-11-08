using System;
using System.Text.Json.Serialization;
using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Entities.Versions;

/// <summary>
/// Minecraft版本信息
/// </summary>
public class VersionInfo
{
    /// <summary>
    /// 版本ID
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// 版本类型
    /// </summary>
    [JsonPropertyName("type")]
    public VersionType VersionType { get; set; }
    // public required string TypeString { get; set; }
    //
    // [JsonIgnore]
    // public VersionType? VersionType => TypeString.ToLower() switch
    // {
    //     "release" => Enums.VersionType.Release,
    //     "snapshot" => Enums.VersionType.Snapshot,
    //     "old_alpha" => Enums.VersionType.OldAlpha,
    //     "old_beta" => Enums.VersionType.OldBeta,
    //     _ => null
    // };
    /// <summary>
    /// 版本详情URL
    /// </summary>
    [JsonPropertyName("url")]
    public required Uri Url { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    [JsonPropertyName("time")]
    public required DateTimeOffset Time { get; set; }
    
    /// <summary>
    /// 发布时间
    /// </summary>
    [JsonPropertyName("releaseTime")]
    public required DateTimeOffset ReleaseTime { get; set; }
    
    /// <summary>
    /// 版本JSON文件的SHA1哈希值（仅v2版本有此属性）
    /// </summary>
    [JsonPropertyName("sha1")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sha1 { get; set; }
    
    /// <summary>
    /// 版本兼容性级别（仅v2版本有此属性）
    /// </summary>
    [JsonPropertyName("complianceLevel")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ComplianceLevel { get; set; }
}
