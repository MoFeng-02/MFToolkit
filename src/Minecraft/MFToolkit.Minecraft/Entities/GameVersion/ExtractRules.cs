using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 提取规则
/// </summary>
public class ExtractRules
{
    /// <summary>
    /// 排除的文件模式
    /// </summary>
    [JsonPropertyName("exclude")]
    public List<string> Exclude { get; set; } = [];

    /// <summary>
    /// 检查文件是否应该被排除
    /// </summary>
    public bool ShouldExclude(string filePath)
    {
        if (Exclude.Count == 0)
            return false;

        return Exclude.Any(pattern => MatchesPattern(filePath, pattern));
    }

    private static bool MatchesPattern(string filePath, string pattern)
    {
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
            
        return Regex.IsMatch(filePath, regexPattern, RegexOptions.IgnoreCase);
    }
}