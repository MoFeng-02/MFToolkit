using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 日志配置
/// </summary>
public class LoggingConfig
{
    /// <summary>
    /// 客户端日志配置
    /// </summary>
    [JsonPropertyName("client")]
    public LoggingEntry? Client { get; set; }
}

/// <summary>
/// 日志配置条目
/// </summary>
public class LoggingEntry
{
    /// <summary>
    /// 参数
    /// </summary>
    [JsonPropertyName("argument")]
    public required string Argument { get; set; }

    /// <summary>
    /// 日志文件信息
    /// </summary>
    [JsonPropertyName("file")]
    public required LoggingFile File { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}

/// <summary>
/// 日志文件信息
/// </summary>
public class LoggingFile
{
    /// <summary>
    /// 文件ID
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// SHA1哈希值
    /// </summary>
    [JsonPropertyName("sha1")]
    public required string Sha1 { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// 下载URL
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}