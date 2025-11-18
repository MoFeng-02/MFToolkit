using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 游戏库信息
/// </summary>
public class Library
{
    /// <summary>
    /// 库名称 (Maven坐标)
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 下载信息
    /// </summary>
    [JsonPropertyName("downloads")]
    public LibraryDownloads? Downloads { get; set; }

    /// <summary>
    /// 适用规则
    /// </summary>
    [JsonPropertyName("rules")]
    public List<Rule>? Rules { get; set; }

    /// <summary>
    /// 原生库映射
    /// </summary>
    [JsonPropertyName("natives")]
    public Dictionary<string, string>? Natives { get; set; }

    /// <summary>
    /// 提取规则
    /// </summary>
    [JsonPropertyName("extract")]
    public ExtractRules? Extract { get; set; }

    /// <summary>
    /// 是否包含 natives 库
    /// </summary>
    [JsonIgnore]
    public bool HasNatives => Natives != null && Natives.Count > 0;

    /// <summary>
    /// 检查库是否适用于指定环境
    /// <param name="os">系统</param>
    /// <param name="features">库不用，一般的新版参数下面的rules规则有</param>
    /// </summary>
    public bool IsApplicable(string os, Dictionary<string, bool>? features = null)
    {
        if (Rules == null || Rules.Count == 0)
            return true;

        foreach (var rule in Rules)
        {
            if (rule.Matches(os, features))
            {
                return rule.Action == RuleAction.Allow;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取原生库分类器
    /// </summary>
    public string? GetNativeClassifier(string os)
    {
        if (Natives == null || !Natives.TryGetValue(os, out var classifier))
            return null;

        // 替换架构占位符
        return classifier?.Replace("${arch}", GetCurrentArchitecture());
    }

    /// <summary>
    /// 获取库的所有下载项
    /// </summary>
    public IEnumerable<DownloadItem> GetDownloads(string os)
    {
        if (Downloads?.Artifact != null)
            yield return Downloads.Artifact;

        var nativeClassifier = GetNativeClassifier(os);
        if (!string.IsNullOrEmpty(nativeClassifier) &&
            Downloads?.Classifiers != null &&
            Downloads.Classifiers.TryGetValue(nativeClassifier, out var nativeDownload))
        {
            yield return nativeDownload;
        }
    }

    private static string GetCurrentArchitecture()
    {
        return Environment.Is64BitOperatingSystem ? "64" : "32";
    }
}

/// <summary>
/// 库下载信息
/// </summary>
public class LibraryDownloads
{
    /// <summary>
    /// 主要构件
    /// </summary>
    [JsonPropertyName("artifact")]
    public DownloadItem? Artifact { get; set; }

    /// <summary>
    /// 分类器构件
    /// </summary>
    [JsonPropertyName("classifiers")]
    public Dictionary<string, DownloadItem>? Classifiers { get; set; }
}

/// <summary>
/// 库路径简约信息，原生库和普通库均在里面
/// </summary>
public class LibrarySimple
{
    /// <summary>
    /// 名称（可能包含 Maven 坐标，如 group:artifact:version:natives-windows）
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// SHA1 校验值
    /// </summary>
    public required string Sha1 { get; set; }

    /// <summary>
    /// Maven 路径（如 org/lwjgl/lwjgl/natives-windows-x86_64/3.3.1/...）
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long Size { get; set; }
}