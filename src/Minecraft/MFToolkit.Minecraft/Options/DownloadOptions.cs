// DownloadOptions.cs
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// 下载配置选项
/// </summary>
public class DownloadOptions() : BaseOptions("download_options.json")
{
    /// <summary>
    /// 镜像源配置
    /// </summary>
    [JsonPropertyName("mirrors")]
    public MirrorConfiguration Mirrors { get; set; } = new();

    /// <summary>
    /// 下载设置
    /// </summary>
    [JsonPropertyName("settings")]
    public DownloadSettings Settings { get; set; } = new();

    /// <summary>
    /// 获取当前使用的镜像源
    /// </summary>
    public string GetCurrentMirror()
    {
        return Mirrors.GetCurrentMirror();
    }
}

/// <summary>
/// 镜像源配置
/// </summary>
public class MirrorConfiguration
{
    /// <summary>
    /// 可用镜像源列表
    /// </summary>
    [JsonPropertyName("availableMirrors")]
    public Dictionary<string, string> AvailableMirrors { get; set; } = new()
    {
        ["official"] = "https://launcher.mojang.com",
        ["bmclapi"] = "https://bmclapi2.bangbang93.com",
        ["mcbbs"] = "https://download.mcbbs.net"
    };

    /// <summary>
    /// 当前使用的镜像源
    /// </summary>
    [JsonPropertyName("currentMirror")]
    public string CurrentMirror { get; set; } = "official";

    /// <summary>
    /// 获取当前镜像源URL
    /// </summary>
    public string GetCurrentMirror()
    {
        return AvailableMirrors.TryGetValue(CurrentMirror, out var url)
            ? url
            : AvailableMirrors["official"];
    }
}

/// <summary>
/// 下载设置
/// </summary>
public class DownloadSettings
{
    /// <summary>
    /// 最大下载线程数
    /// </summary>
    [JsonPropertyName("maxDownloadThreads")]
    public int MaxDownloadThreads { get; set; } = 4;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    [JsonPropertyName("maxRetryCount")]
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// 重试延迟（毫秒）
    /// </summary>
    [JsonPropertyName("retryDelayMs")]
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// 下载超时（秒）
    /// </summary>
    [JsonPropertyName("downloadTimeoutSeconds")]
    public int DownloadTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// 启用断点续传
    /// </summary>
    [JsonPropertyName("enableResume")]
    public bool EnableResume { get; set; } = true;

    /// <summary>
    /// 启用并行下载
    /// </summary>
    [JsonPropertyName("enableParallelDownload")]
    public bool EnableParallelDownload { get; set; } = true;

    /// <summary>
    /// 下载速度限制（KB/s），0表示无限制
    /// </summary>
    [JsonPropertyName("speedLimitKb")]
    public int SpeedLimitKb { get; set; } = 0;
}