using System;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Cape;

/// <summary>
/// Minecraft披风信息
/// </summary>
public class CapeInfo
{
    /// <summary>
    /// 披风ID
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    /// <summary>
    /// 披风状态
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }
    
    /// <summary>
    /// 披风URL
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
    
    /// <summary>
    /// 披风上传时间
    /// </summary>
    [JsonPropertyName("upload_time")]
    public DateTimeOffset? UploadTime { get; set; }
    
    /// <summary>
    /// 披风类型
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
