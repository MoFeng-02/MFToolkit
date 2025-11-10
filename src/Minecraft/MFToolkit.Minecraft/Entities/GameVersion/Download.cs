using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;
/// <summary>
/// 下载信息集合
/// </summary>
public class Downloads
{
    /// <summary>
    /// 客户端下载
    /// </summary>
    [JsonPropertyName("client")]
    public DownloadItem? Client { get; set; }

    /// <summary>
    /// 服务端下载
    /// </summary>
    [JsonPropertyName("server")]
    public DownloadItem? Server { get; set; }

    /// <summary>
    /// 客户端映射文件
    /// </summary>
    [JsonPropertyName("client_mappings")]
    public DownloadItem? ClientMappings { get; set; }

    /// <summary>
    /// 服务端映射文件
    /// </summary>
    [JsonPropertyName("server_mappings")]
    public DownloadItem? ServerMappings { get; set; }
}

/// <summary>
/// 下载项信息
/// </summary>
public class DownloadItem
{
    /// <summary>
    /// 下载URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// SHA1哈希
    /// </summary>
    [JsonPropertyName("sha1")]
    public string Sha1 { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// 文件路径（仅在某些特定情况下使用）
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }
}