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
    /// 获取库的路径
    /// </summary>
    /// <returns>库的路径</returns>
    public string GetPath()
    {
        if (string.IsNullOrEmpty(Name))
            throw new InvalidOperationException("Library name is null or empty");
            
        string[] parts = Name.Split(':');
        if (parts.Length != 3)
            throw new InvalidOperationException($"Invalid library name format: {Name}");
            
        string groupId = parts[0].Replace('.', '/');
        string artifactId = parts[1];
        string version = parts[2];
        
        return $"{groupId}/{artifactId}/{version}/{artifactId}-{version}.jar";
    }
    
    /// <summary>
    /// 获取原生库的分类器
    /// </summary>
    /// <param name="os">操作系统</param>
    /// <returns>原生库的分类器</returns>
    public string? GetNativeClassifier(string os)
    {
        if (Natives == null || !Natives.TryGetValue(os, out string? classifierTemplate))
            return null;
            
        // 替换模板中的变量
        string arch = Environment.Is64BitOperatingSystem ? "64" : "32";
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
            
        // 添加主要工件
        if (Downloads?.Artifact != null)
            yield return Downloads.Artifact;
            
        // 添加原生库（如果有）
        string? nativeClassifier = GetNativeClassifier(os);
        if (!string.IsNullOrEmpty(nativeClassifier) && Downloads?.Classifiers != null &&
            Downloads.Classifiers.TryGetValue(nativeClassifier, out Download? nativeDownload))
        {
            yield return nativeDownload;
        }
    }
}
