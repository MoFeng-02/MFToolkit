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
    [JsonPropertyName("allow")]
    Allow,

    /// <summary>
    /// 拒绝
    /// </summary>
    [JsonPropertyName("disallow")]
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
            // 检查操作系统名称是否匹配（支持别名映射）
            if (!IsOsMatch(Os.Name, currentOs))
            {
                return false;
            }

            // 检查操作系统版本是否匹配（如果指定了版本）
            if (!string.IsNullOrEmpty(Os.Version))
            {
                if (!IsVersionMatch(Os.Version, GetCurrentOsVersion()))
                {
                    return false;
                }
            }

            // 检查架构是否匹配（如果指定了架构）
            if (!string.IsNullOrEmpty(Os.Arch))
            {
                string currentArch = GetCurrentArchitecture();
                if (!string.Equals(Os.Arch, currentArch, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }

        if (Features != null)
        {
            if (currentFeatures == null)
                return false;

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

    // 新增辅助方法
    private bool IsOsMatch(string expectedOs, string currentOs)
    {
        // 操作系统名称映射
        var osMappings = new Dictionary<string, string[]>
        {
            ["windows"] = ["windows", "win"],
            ["linux"] = ["linux", "unix"],
            ["osx"] = ["osx", "mac", "macos", "darwin"]
        };

        if (osMappings.TryGetValue(expectedOs.ToLower(), out var aliases))
        {
            return aliases.Any(alias => string.Equals(alias, currentOs, StringComparison.OrdinalIgnoreCase));
        }

        return string.Equals(expectedOs, currentOs, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsVersionMatch(string expectedVersion, string currentVersion)
    {
        try
        {
            // 尝试解析版本号进行精确比较
            var expected = new Version(expectedVersion);
            var current = new Version(currentVersion);
            return current >= expected; // 通常检查当前版本是否 >= 期望版本
        }
        catch
        {
            // 如果版本解析失败，回退到字符串包含检查
            return currentVersion.Contains(expectedVersion);
        }
    }

    private string GetCurrentOsVersion()
    {
        return Environment.OSVersion.Version.ToString();
    }

    private string GetCurrentArchitecture()
    {
        return Environment.Is64BitOperatingSystem ? "x64" : "x86";
    }
}