using System.Text.Json.Serialization;
using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 游戏版本信息
/// </summary>
public class GameVersionDetail
{
     /// <summary>
    /// 版本ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 版本类型
    /// </summary>
    [JsonPropertyName("type")]
    public VersionType Type { get; set; } = VersionType.Release;

    /// <summary>
    /// 发布时间
    /// </summary>
    [JsonPropertyName("releaseTime")]
    public DateTimeOffset ReleaseTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [JsonPropertyName("time")]
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// 主类名
    /// </summary>
    [JsonPropertyName("mainClass")]
    public string MainClass { get; set; } = string.Empty;

    /// <summary>
    /// 资源版本
    /// </summary>
    [JsonPropertyName("assets")]
    public string Assets { get; set; } = "legacy";

    /// <summary>
    /// 最低启动器版本
    /// </summary>
    [JsonPropertyName("minimumLauncherVersion")]
    public int MinimumLauncherVersion { get; set; }

    /// <summary>
    /// 库列表
    /// </summary>
    [JsonPropertyName("libraries")]
    public List<Library> Libraries { get; set; } = new();

    /// <summary>
    /// 下载信息
    /// </summary>
    [JsonPropertyName("downloads")]
    public Downloads? Downloads { get; set; }

    /// <summary>
    /// 资源索引信息
    /// </summary>
    [JsonPropertyName("assetIndex")]
    public AssetIndex? AssetIndex { get; set; }

    /// <summary>
    /// 启动参数（新版本格式）
    /// </summary>
    [JsonPropertyName("arguments")]
    public Arguments? Arguments { get; set; }

    /// <summary>
    /// 启动参数（旧版本格式）
    /// </summary>
    [JsonPropertyName("minecraftArguments")]
    public string? MinecraftArguments { get; set; }

    /// <summary>
    /// Java版本信息
    /// </summary>
    [JsonPropertyName("javaVersion")]
    public JavaVersionInfo? JavaVersion { get; set; }

    /// <summary>
    /// 日志配置
    /// </summary>
    [JsonPropertyName("logging")]
    public LoggingConfig? Logging { get; set; }

    /// <summary>
    /// 继承的版本（Forge/Fabric等）
    /// </summary>
    [JsonPropertyName("inheritsFrom")]
    public string? InheritsFrom { get; set; }
}

/// <summary>
/// 常用占位符常量
/// </summary>
public static class CommonPlaceholders
{
    public const string VersionName = "version_name";
    public const string GameDirectory = "game_directory";
    public const string AssetsRoot = "assets_root";
    public const string AssetsIndexName = "assets_index_name";
    public const string AuthPlayerName = "auth_player_name";
    public const string AuthUUID = "auth_uuid";
    public const string AuthAccessToken = "auth_access_token";
    public const string UserType = "user_type";
    public const string UserProperties = "user_properties";
    public const string GameAssets = "game_assets";
    public const string Classpath = "classpath";
    public const string LibrariesPath = "libraries_path";
    public const string NativesDirectory = "natives_directory";
    public const string LauncherName = "launcher_name";
    public const string LauncherVersion = "launcher_version";

    /// <summary>
    /// 获取常用的占位符替换字典
    /// </summary>
    public static Dictionary<string, string> GetDefaultReplacements(string versionId, string gameDir, string assetsDir)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [VersionName] = versionId,
            [GameDirectory] = gameDir,
            [AssetsRoot] = assetsDir,
            [AssetsIndexName] = versionId,
            [NativesDirectory] = Path.Combine(gameDir, "natives"),
            [LibrariesPath] = Path.Combine(gameDir, "libraries"),
            [LauncherName] = "MFToolkit",
            [LauncherVersion] = "1.0.0"
        };
    }
}