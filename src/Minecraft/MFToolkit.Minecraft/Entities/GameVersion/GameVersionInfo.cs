using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 游戏版本信息
/// </summary>
public class GameVersionInfo
{
    /// <summary>
    /// 版本ID
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// 版本类型
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    
    /// <summary>
    /// 版本名称
    /// </summary>
    [JsonPropertyName("releaseTime")]
    public required DateTimeOffset ReleaseTime { get; set; }
    
    /// <summary>
    /// 上次更新时间
    /// </summary>
    [JsonPropertyName("time")]
    public required DateTimeOffset Time { get; set; }
    
    /// <summary>
    /// 最低启动器版本
    /// </summary>
    [JsonPropertyName("minimumLauncherVersion")]
    public int MinimumLauncherVersion { get; set; }
    
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
    /// 资源版本
    /// </summary>
    [JsonPropertyName("assets")]
    public required string Assets { get; set; }
    
    /// <summary>
    /// 库列表
    /// </summary>
    [JsonPropertyName("libraries")]
    public required List<Library> Libraries { get; set; }
    
    /// <summary>
    /// 主类名称
    /// </summary>
    [JsonPropertyName("mainClass")]
    public required string MainClass { get; set; }
    
    /// <summary>
    /// 游戏参数
    /// </summary>
    [JsonPropertyName("arguments")]
    public Arguments? Arguments { get; set; }
    
    /// <summary>
    /// Java版本信息（适用于新版）
    /// </summary>
    [JsonPropertyName("javaVersion")]
    public JavaVersionInfo? JavaVersion { get; set; }

    /// <summary>
    /// 继承的版本，加载器专用，看哪些加载器需要
    /// </summary>
    [JsonPropertyName("inheritsFrom")]
    public string? InheritsFrom { get; set; }
    
    /// <summary>
    /// 获取适用于当前环境的库下载信息
    /// </summary>
    /// <param name="os">操作系统</param>
    /// <param name="features">特性</param>
    /// <returns>下载信息列表</returns>
    public IEnumerable<Download> GetApplicableLibraryDownloads(string os, Dictionary<string, bool>? features = null)
    {
        foreach (var library in Libraries)
        {
            foreach (var download in library.GetApplicableDownloads(os, features))
            {
                yield return download;
            }
        }
    }
    
    /// <summary>
    /// 获取适用于当前环境的原生库下载信息
    /// </summary>
    /// <param name="os">操作系统</param>
    /// <param name="features">特性</param>
    /// <returns>原生库下载信息列表</returns>
    public IEnumerable<(Download Download, ExtractRules? ExtractRules)> GetApplicableNativeDownloads(string os, Dictionary<string, bool>? features = null)
    {
        foreach (var library in Libraries)
        {
            if (!library.IsApplicable(os, features))
                continue;
                
            string? nativeClassifier = library.GetNativeClassifier(os);
            if (!string.IsNullOrEmpty(nativeClassifier) && library.Downloads?.Classifiers != null && 
                library.Downloads.Classifiers.TryGetValue(nativeClassifier, out Download? nativeDownload))
            {
                yield return (nativeDownload, library.Extract);
            }
        }
    }
    
    /// <summary>
    /// 获取游戏启动参数
    /// </summary>
    /// <param name="os">操作系统</param>
    /// <param name="features">特性</param>
    /// <param name="replacements">参数替换字典</param>
    /// <returns>启动参数列表</returns>
    public IEnumerable<string> GetLaunchArguments(string os, Dictionary<string, bool>? features = null, Dictionary<string, string>? replacements = null)
    {
        if (Arguments == null)
            yield break;
            
        foreach (var arg in Arguments.Game)
        {
            if (arg is string strArg)
            {
                yield return ReplacePlaceholders(strArg, replacements);
            }
            else if (arg is RuleBasedArgument ruleArg)
            {
                if (ruleArg.Matches(os, features))
                {
                    yield return ReplacePlaceholders(ruleArg.Value, replacements);
                }
            }
        }
    }
    
    /// <summary>
    /// 获取JVM参数
    /// </summary>
    /// <param name="os">操作系统</param>
    /// <param name="features">特性</param>
    /// <param name="replacements">参数替换字典</param>
    /// <returns>JVM参数列表</returns>
    public IEnumerable<string> GetJvmArguments(string os, Dictionary<string, bool>? features = null, Dictionary<string, string>? replacements = null)
    {
        if (Arguments == null)
        {
            yield return ReplacePlaceholders("-cp", replacements);
            yield break;
        }
            
        foreach (var arg in Arguments.Jvm)
        {
            if (arg is string strArg)
            {
                yield return ReplacePlaceholders(strArg, replacements);
            }
            else if (arg is RuleBasedArgument ruleArg)
            {
                if (ruleArg.Matches(os, features))
                {
                    yield return ReplacePlaceholders(ruleArg.Value, replacements);
                }
            }
        }
    }
    
    /// <summary>
    /// 替换参数中的占位符
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="replacements">替换字典</param>
    /// <returns>替换后的字符串</returns>
    private string ReplacePlaceholders(string input, Dictionary<string, string>? replacements)
    {
        if (replacements == null || replacements.Count == 0)
            return input;
            
        string result = input;
        foreach (var replacement in replacements)
        {
            result = result.Replace("${" + replacement.Key + "}", replacement.Value);
        }
        return result;
    }
}
