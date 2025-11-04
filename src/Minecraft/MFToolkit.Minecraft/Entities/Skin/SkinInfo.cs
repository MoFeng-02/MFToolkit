using System;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Skin;

/// <summary>
/// Minecraft皮肤信息
/// </summary>
public class SkinInfo
{
    /// <summary>
    /// 皮肤ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// 皮肤状态
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }
    
    /// <summary>
    /// 皮肤URL
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    /// <summary>
    /// 皮肤变体
    /// </summary>
    [JsonPropertyName("variant")]
    public string? Variant { get; set; }
    
    /// <summary>
    /// 皮肤上传时间
    /// </summary>
    [JsonPropertyName("upload_time")]
    public DateTimeOffset? UploadTime { get; set; }
    
    /// <summary>
    /// 检查皮肤是否为经典模型
    /// </summary>
    public bool IsClassicModel => Variant?.Equals("classic", StringComparison.OrdinalIgnoreCase) ?? false;
    
    /// <summary>
    /// 检查皮肤是否为纤细模型
    /// </summary>
    public bool IsSlimModel => Variant?.Equals("slim", StringComparison.OrdinalIgnoreCase) ?? false;
}
