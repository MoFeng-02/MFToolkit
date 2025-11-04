using System;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account;

/// <summary>
/// Minecraft账号属性
/// </summary>
public class ProfileProperty
{
    /// <summary>
    /// 属性名称
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// 属性值
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
    
    /// <summary>
    /// 属性签名
    /// </summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}
