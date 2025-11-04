using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 规则操作类型
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RuleAction
{
    /// <summary>
    /// 允许
    /// </summary>
    Allow,
    
    /// <summary>
    /// 拒绝
    /// </summary>
    Disallow
}

/// <summary>
/// 规则信息
/// </summary>
public class Rule
{
    /// <summary>
    /// 规则操作
    /// </summary>
    [JsonPropertyName("action")]
    public required RuleAction Action { get; set; }
    
    /// <summary>
    /// 操作系统条件
    /// </summary>
    [JsonPropertyName("os")]
    public Os? Os { get; set; }
    
    /// <summary>
    /// 特性条件
    /// </summary>
    [JsonPropertyName("features")]
    public Dictionary<string, bool>? Features { get; set; }
    
    /// <summary>
    /// 检查规则是否是否适用于当前环境
    /// </summary>
    /// <param name="currentOs">当前操作系统</param>
    /// <param name="currentFeatures">当前特性</param>
    /// <returns>是否匹配</returns>
    public bool Matches(string currentOs, Dictionary<string, bool>? currentFeatures = null)
    {
        if (Os != null)
        {
            // 检查操作系统名称是否匹配
            if (!string.Equals(Os.Name, currentOs, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            // 检查操作系统版本是否匹配（如果指定了版本）
            if (!string.IsNullOrEmpty(Os.Version) && !Environment.OSVersion.VersionString.Contains(Os.Version))
            {
                return false;
            }
            
            // 检查架构是否匹配（如果指定了架构）
            if (!string.IsNullOrEmpty(Os.Arch))
            {
                string currentArch = Environment.Is64BitOperatingSystem ? "64" : "32";
                if (!string.Equals(Os.Arch, currentArch, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }
        
        if (Features != null && currentFeatures != null)
        {
            foreach (var feature in Features)
            {
                if (!currentFeatures.TryGetValue(feature.Key, out bool value) || value != feature.Value)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}
