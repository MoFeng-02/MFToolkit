using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 下载信息
/// </summary>
public class Download
{
    /// <summary>
    /// 下载URL
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }
    
    /// <summary>
    /// SHA1哈希值
    /// </summary>
    [JsonPropertyName("sha1")]
    public required string Sha1 { get; set; }
    
    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }
}

/// <summary>
/// 下载集合
/// </summary>
public class Downloads
{
    /// <summary>
    /// 客户端下载信息
    /// </summary>
    [JsonPropertyName("client")]
    public Download? Client { get; set; }
    
    /// <summary>
    /// 服务器下载信息
    /// </summary>
    [JsonPropertyName("server")]
    public Download? Server { get; set; }
    
    /// <summary>
    /// 客户端映射文件下载信息
    /// </summary>
    [JsonPropertyName("client_mappings")]
    public Download? ClientMappings { get; set; }
    
    /// <summary>
    /// 服务器映射文件下载信息
    /// </summary>
    [JsonPropertyName("server_mappings")]
    public Download? ServerMappings { get; set; }
}
