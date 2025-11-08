using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Enums;

/// <summary>
/// Minecraft版本类型
/// </summary>
public enum VersionType
{
    /// <summary>
    /// 非支持的版本
    /// </summary>
    None,
    /// <summary>
    /// 正式版
    /// </summary>
    [JsonPropertyName("release")]
    Release,

    /// <summary>
    /// 快照版
    /// </summary>
    [JsonPropertyName("snapshot")]
    Snapshot,

    /// <summary>
    /// 旧Alpha版
    /// </summary>
    [JsonPropertyName("old_alpha")]
    OldAlpha,

    /// <summary>
    /// 旧Beta版
    /// </summary>
    [JsonPropertyName("old_beta")]
    OldBeta,
}
