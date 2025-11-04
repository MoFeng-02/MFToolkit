using System;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Skin;

/// <summary>
/// Minecraft皮肤上传请求
/// </summary>
public class SkinUploadRequest
{
    /// <summary>
    /// 皮肤图片的Base64编码
    /// </summary>
    [JsonPropertyName("variant")]
    public string? Variant { get; set; } = "classic";
    
    /// <summary>
    /// 皮肤模型类型
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    /// <summary>
    /// 设置皮肤为经典模型
    /// </summary>
    public void SetClassicModel() => Variant = "classic";
    
    /// <summary>
    /// 设置皮肤为纤细模型
    /// </summary>
    public void SetSlimModel() => Variant = "slim";
}
