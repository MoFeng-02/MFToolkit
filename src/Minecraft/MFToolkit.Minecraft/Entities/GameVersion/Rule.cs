using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 规则操作类型
/// </summary>
public enum RuleAction
{
    /// <summary>
    /// 允许
    /// </summary>
    [JsonPropertyName("allow")]
    Allow,

    /// <summary>
    /// 拒绝
    /// </summary>
    [JsonPropertyName("disallow")]
    Disallow
}

/// <summary>
/// 环境规则
/// </summary>
public class Rule
{
    /// <summary>
    /// 规则动作
    /// </summary>
    [JsonPropertyName("action")]
    public RuleAction Action { get; set; } = RuleAction.Allow;

    /// <summary>
    /// 操作系统条件
    /// </summary>
    [JsonPropertyName("os")]
    public OsRule? Os { get; set; }

    /// <summary>
    /// 特性条件
    /// </summary>
    [JsonPropertyName("features")]
    public Dictionary<string, bool>? Features { get; set; }

    /// <summary>
    /// 检查规则是否匹配环境
    /// </summary>
    public bool Matches(string currentOs, Dictionary<string, bool>? currentFeatures = null)
    {
        // 检查操作系统条件
        if (Os != null && !Os.Matches(currentOs))
            return false;

        // 检查特性条件
        if (Features != null)
        {
            if (currentFeatures == null)
                return false;

            foreach (var feature in Features)
            {
                if (!currentFeatures.TryGetValue(feature.Key, out var value) || value != feature.Value)
                    return false;
            }
        }

        return true;
    }
}

/// <summary>
/// 操作系统规则
/// </summary>
public class OsRule
{
    /// <summary>
    /// 操作系统名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 操作系统版本
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    [JsonPropertyName("arch")]
    public string? Arch { get; set; }

    /// <summary>
    /// 检查是否匹配操作系统
    /// </summary>
    public bool Matches(string currentOs)
    {
        return OsNameMapping.TryGetValue(Name, out var aliases) 
            ? aliases.Contains(currentOs, StringComparer.OrdinalIgnoreCase)
            : string.Equals(Name, currentOs, StringComparison.OrdinalIgnoreCase);
    }

    private static readonly Dictionary<string, string[]> OsNameMapping = new()
    {
        ["windows"] = new[] { "windows", "win" },
        ["linux"] = new[] { "linux", "unix" },
        ["osx"] = new[] { "osx", "mac", "macos", "darwin" }
    };
}