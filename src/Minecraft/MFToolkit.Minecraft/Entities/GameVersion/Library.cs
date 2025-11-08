using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 库下载信息
/// </summary>
public class LibraryDownloads
{
    /// <summary>
    /// 工件下载信息
    /// </summary>
    [JsonPropertyName("artifact")]
    public Download? Artifact { get; set; }

    /// <summary>
    /// 分类器下载信息
    /// </summary>
    [JsonPropertyName("classifiers")]
    public Dictionary<string, Download>? Classifiers { get; set; }
}

/// <summary>
/// 库信息
/// </summary>
public class Library
{
    /// <summary>
    /// 库名称（groupId:artifactId:version）
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// 下载信息
    /// </summary>
    [JsonPropertyName("downloads")]
    public LibraryDownloads? Downloads { get; set; }

    /// <summary>
    /// 原生分类器
    /// </summary>
    [JsonPropertyName("natives")]
    public Dictionary<string, string>? Natives { get; set; }

    /// <summary>
    /// 提取规则
    /// </summary>
    [JsonPropertyName("extract")]
    public ExtractRules? Extract { get; set; }

    /// <summary>
    /// 规则列表
    /// </summary>
    [JsonPropertyName("rules")]
    public List<Rule>? Rules { get; set; }

    /// <summary>
    /// 库URL（旧版本格式）
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// 获取库的路径
    /// </summary>
    /// <returns>库的路径</returns>
    public string GetPath()
    {
        if (string.IsNullOrEmpty(Name))
            throw new InvalidOperationException("Library name is null or empty");

        // Maven 坐标必须是三段式：groupId:artifactId:version
        // 可选的四段式：groupId:artifactId:version:classifier
        // 可选的四段式带扩展名：groupId:artifactId:version:classifier@extension
        // 或五段式：groupId:artifactId:version:classifier:extension
        var parts = Name.Split(':');

        // Maven 坐标必须有至少3部分（groupId:artifactId:version）
        if (parts.Length < 3)
            throw new InvalidOperationException($"Invalid library name format: {Name}. Expected at least 3 parts.");

        var groupId = parts[0].Replace('.', '/');
        var artifactId = parts[1];
        var version = parts[2];
        string? classifier = null;
        string extension = "jar";

        // 处理分类器（第四部分）
        if (parts.Length >= 4 && !string.IsNullOrEmpty(parts[3]))
        {
            // 检查第四部分是否包含扩展名（classifier@extension 格式）
            if (parts[3].Contains('@'))
            {
                var classifierParts = parts[3].Split('@');
                classifier = classifierParts[0];
                extension = classifierParts.Length > 1 ? classifierParts[1] : "jar";
            }
            else
            {
                classifier = parts[3];
            }
        }

        // 处理显式的扩展名（第五部分）
        if (parts.Length >= 5 && !string.IsNullOrEmpty(parts[4]))
        {
            extension = parts[4];
        }

        // 构建标准的 Maven 仓库路径结构
        // 格式：groupId/artifactId/version/artifactId-version[-classifier].extension
        var fileName = $"{artifactId}-{version}";
        if (!string.IsNullOrEmpty(classifier))
        {
            fileName += $"-{classifier}";
        }
        fileName += $".{extension}";

        return $"{groupId}/{artifactId}/{version}/{fileName}";
    }

    /// <summary>
    /// 获取库的完整URL（旧版本格式）
    /// </summary>
    /// <returns>库的完整URL</returns>
    public string GetFullUrl()
    {
        if (string.IsNullOrEmpty(Url))
            throw new InvalidOperationException("Library URL is null or empty");

        return Url + GetPath();
    }

    /// <summary>
    /// 获取原生库
    /// </summary>
    /// <param name="os"></param>
    /// <returns></returns>
    public string? GetNativeClassifier(string os)
    {
        if (Natives == null || !Natives.TryGetValue(os, out string? classifierTemplate))
            return null;

        // 完整的架构检测和替换
        string arch = GetCurrentArchitectureForNative();
        return classifierTemplate.Replace("${arch}", arch);
    }

    /// <summary>
    /// 检查库是否适用于当前环境
    /// </summary>
    /// <param name="os">操作系统</param>
    /// <param name="features">特性</param>
    /// <returns>是否适用</returns>
    public bool IsApplicable(string os, Dictionary<string, bool>? features = null)
    {
        // 如果没有规则，则默认适用
        if (Rules == null || Rules.Count == 0)
            return true;

        // 检查规则
        foreach (var rule in Rules)
        {
            if (rule.Matches(os, features))
            {
                return rule.Action == RuleAction.Allow;
            }
        }

        // 默认拒绝
        return false;
    }

    /// <summary>
    /// 获取适用于当前环境的库下载信息
    /// </summary>
    /// <param name="os">操作系统</param>
    /// <param name="features">特性</param>
    /// <returns>下载信息列表</returns>
    public IEnumerable<Download> GetApplicableDownloads(string os, Dictionary<string, bool>? features = null)
    {
        if (!IsApplicable(os, features))
            yield break;

        // 新版本格式：使用downloads字段
        if (Downloads != null)
        {
            // 添加主要工件
            if (Downloads.Artifact != null)
                yield return Downloads.Artifact;

            // 添加原生库（如果有）
            string? nativeClassifier = GetNativeClassifier(os);
            if (!string.IsNullOrEmpty(nativeClassifier) && Downloads.Classifiers != null &&
                Downloads.Classifiers.TryGetValue(nativeClassifier, out Download? nativeDownload))
            {
                yield return nativeDownload;
            }
        }
        // 旧版本格式：使用url字段
        else if (!string.IsNullOrEmpty(Url))
        {
            // 为旧版本库创建下载信息
            yield return new Download
            {
                Url = GetFullUrl(),
                Sha1 = "", // 旧版本可能没有SHA1
                Size = 0   // 旧版本可能没有大小
            };
        }
    }

    /// <summary>
    /// 检查文件是否应该从原生库中提取
    /// </summary>
    public bool ShouldExtractFromNative(string filePath)
    {
        if (Extract == null)
            return true; // 没有提取规则，提取所有文件

        return !Extract.ShouldExclude(filePath);
    }

    #region Private Methods


    // 新增架构检测方法
    /// <summary>
    /// 获取当前架构用于原生库分类器
    /// </summary>
    /// <returns></returns>
    private string GetCurrentArchitectureForNative()
    {
        if (Environment.Is64BitOperatingSystem)
        {
            // 检测 ARM 架构
            if (IsArmArchitecture())
            {
                return "arm64";
            }
            return "64";
        }
        else
        {
            if (IsArmArchitecture())
            {
                return "arm32";
            }
            return "32";
        }
    }

    /// <summary>
    /// 是否为 ARM 架构
    /// </summary>
    /// <returns></returns>
    private bool IsArmArchitecture()
    {
        // 检测 ARM 架构的简单方法
        var arch = RuntimeInformation.ProcessArchitecture;
        return arch == Architecture.Arm || arch == Architecture.Arm64;
    }

    #endregion
}