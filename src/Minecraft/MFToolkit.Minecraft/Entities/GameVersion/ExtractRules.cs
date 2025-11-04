using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 提取规则
/// </summary>
public class ExtractRules
{
    /// <summary>
    /// 要排除的文件模式
    /// </summary>
    [JsonPropertyName("exclude")]
    public List<string>? Exclude { get; set; }
    
    /// <summary>
    /// 检查文件是否应该被排除
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否应该被排除</returns>
    public bool ShouldExclude(string filePath)
    {
        if (Exclude == null || Exclude.Count == 0)
            return false;
            
        foreach (string pattern in Exclude)
        {
            if (MatchesPattern(filePath, pattern))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查文件路径是否匹配模式
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="pattern">模式</param>
    /// <returns>是否匹配</returns>
    private bool MatchesPattern(string filePath, string pattern)
    {
        // 简单的通配符匹配实现
        string regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
            
        return Regex.IsMatch(filePath, regexPattern, RegexOptions.IgnoreCase);
    }
}
