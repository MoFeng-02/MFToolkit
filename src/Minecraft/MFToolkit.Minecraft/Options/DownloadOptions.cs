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
    /// 将URL转换为当前镜像源的URL
    /// </summary>
    /// <param name="originalUrl">原始URL</param>
    /// <returns>转换后的URL</returns>
    public string ConvertToMirrorUrl(string originalUrl)
    {
        return Mirrors.ConvertToMirrorUrl(originalUrl);
    }

    /// <summary>
    /// 获取当前使用的镜像源URL
    /// </summary>
    public string GetCurrentMirrorUrl()
    {
        return Mirrors.GetCurrentMirrorUrl();
    }

    /// <summary>
    /// 设置当前镜像源
    /// </summary>
    /// <param name="mirrorKey">镜像源键值</param>
    public void SetCurrentMirror(string mirrorKey)
    {
        Mirrors.CurrentMirror = mirrorKey;
    }
}

/// <summary>
/// 镜像源配置
/// </summary>
public class MirrorConfiguration
{
    /// <summary>
    /// 当前使用的镜像源
    /// </summary>
    [JsonPropertyName("currentMirror")]
    public string CurrentMirror { get; set; } = "bmclapi";

    /// <summary>
    /// 镜像源配置字典
    /// </summary>
    [JsonPropertyName("mirrorConfigs")]
    public Dictionary<string, MirrorConfig> MirrorConfigs { get; set; } = new()
    {
        ["official"] = new MirrorConfig
        {
            BaseUrl = "",
            UrlMappings = new Dictionary<string, string>
            {
                ["https://launcher.mojang.com"] = "https://launcher.mojang.com",
                ["https://libraries.minecraft.net"] = "https://libraries.minecraft.net",
                ["https://resources.download.minecraft.net"] = "https://resources.download.minecraft.net",
                ["https://piston-meta.mojang.com"] = "https://piston-meta.mojang.com",
                ["https://piston-data.mojang.com"] = "https://piston-data.mojang.com",
                ["https://launchermeta.mojang.com"] = "https://launchermeta.mojang.com",
                ["https://files.minecraftforge.net/maven"] = "https://files.minecraftforge.net/maven",
                ["https://maven.minecraftforge.net"] = "https://maven.minecraftforge.net",
                ["https://maven.fabricmc.net"] = "https://maven.fabricmc.net",
                ["https://maven.quiltmc.org"] = "https://maven.quiltmc.org",
                ["https://maven.neoforged.net"] = "https://maven.neoforged.net"
            }
        },
        ["bmclapi"] = new MirrorConfig
        {
            BaseUrl = "https://bmclapi2.bangbang93.com",
            UrlMappings = new Dictionary<string, string>
            {
                ["https://launcher.mojang.com"] = "",
                ["https://libraries.minecraft.net"] = "/libraries",
                ["https://resources.download.minecraft.net"] = "/assets",
                ["https://piston-meta.mojang.com"] = "",
                ["https://piston-data.mojang.com"] = "",
                ["https://launchermeta.mojang.com"] = "",
                ["https://files.minecraftforge.net/maven"] = "/forge/maven",
                ["https://maven.minecraftforge.net"] = "/maven",
                ["https://maven.fabricmc.net"] = "/fabric-maven",
                ["https://maven.quiltmc.org"] = "/quilt-maven",
                ["https://maven.neoforged.net"] = "/neoforge-maven"
            }
        },
        ["mcbbs"] = new MirrorConfig
        {
            BaseUrl = "https://download.mcbbs.net",
            UrlMappings = new Dictionary<string, string>
            {
                ["https://launcher.mojang.com"] = "",
                ["https://libraries.minecraft.net"] = "/libraries",
                ["https://resources.download.minecraft.net"] = "/assets",
                ["https://piston-meta.mojang.com"] = "",
                ["https://piston-data.mojang.com"] = "",
                ["https://launchermeta.mojang.com"] = "",
                ["https://files.minecraftforge.net/maven"] = "/forge/maven",
                ["https://maven.minecraftforge.net"] = "/maven",
                ["https://maven.fabricmc.net"] = "/fabric-maven",
                ["https://maven.quiltmc.org"] = "/quilt-maven",
                ["https://maven.neoforged.net"] = "/neoforge-maven"
            }
        }
    };

    /// <summary>
    /// 获取当前镜像源URL
    /// </summary>
    public string GetCurrentMirrorUrl()
    {
        if (MirrorConfigs.TryGetValue(CurrentMirror, out var config))
        {
            return config.BaseUrl;
        }
        return "";
    }

    /// <summary>
    /// 将URL转换为当前镜像源的URL
    /// </summary>
    /// <param name="originalUrl">原始URL</param>
    /// <returns>转换后的URL</returns>
    public string ConvertToMirrorUrl(string originalUrl)
    {
        if (string.IsNullOrEmpty(originalUrl))
            return originalUrl;

        // 获取当前镜像源配置
        if (!MirrorConfigs.TryGetValue(CurrentMirror, out var config))
            return originalUrl;

        // 遍历映射规则
        foreach (var mapping in config.UrlMappings)
        {
            if (originalUrl.StartsWith(mapping.Key))
            {
                var path = originalUrl[mapping.Key.Length..];

                // 如果是官方源，直接使用映射值（完整URL）
                if (CurrentMirror == "official")
                {
                    return $"{mapping.Value}{path}";
                }
                // 其他镜像源使用基础URL + 映射路径
                else
                {
                    return $"{config.BaseUrl}{mapping.Value}{path}";
                }
            }
        }

        // 没有匹配的映射，返回原URL
        return originalUrl;
    }

    /// <summary>
    /// 获取所有可用的镜像源
    /// </summary>
    public Dictionary<string, string> GetAvailableMirrors()
    {
        return MirrorConfigs.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.BaseUrl
        );
    }

    /// <summary>
    /// 添加自定义镜像源
    /// </summary>
    public void AddCustomMirror(string key, string baseUrl, Dictionary<string, string>? urlMappings = null)
    {
        MirrorConfigs[key] = new MirrorConfig
        {
            BaseUrl = baseUrl,
            UrlMappings = urlMappings ?? new Dictionary<string, string>()
        };
    }
}

/// <summary>
/// 镜像源配置
/// </summary>
public class MirrorConfig
{
    /// <summary>
    /// 基础URL
    /// </summary>
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL映射规则
    /// </summary>
    [JsonPropertyName("urlMappings")]
    public Dictionary<string, string> UrlMappings { get; set; } = new();
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
    public int MaxDownloadThreads { get; set; } = 5;

    /// <summary>
    /// 最大重试次数
    /// </summary>
    [JsonPropertyName("maxRetryCount")]
    public int MaxRetryCount { get; set; } = 2;

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