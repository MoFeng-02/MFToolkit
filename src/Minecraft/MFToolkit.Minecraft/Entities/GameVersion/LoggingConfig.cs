// LoggingConfig.cs
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
    public string Argument { get; set; } = string.Empty;

    /// <summary>
    /// 日志文件配置
    /// </summary>
    [JsonPropertyName("file")]
    public LoggingFile File { get; set; } = new();

    /// <summary>
    /// 日志类型
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// 日志文件配置
/// </summary>
public class LoggingFile
{
    /// <summary>
    /// 文件ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

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
    /// 下载URL
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}