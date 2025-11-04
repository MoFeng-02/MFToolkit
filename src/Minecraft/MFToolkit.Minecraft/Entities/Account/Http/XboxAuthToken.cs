#nullable disable
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account.Http;

/// <summary>
/// Xbox Live认证响应模型
/// <para>需要保存这里的Token和uhs。uhs是user hash的缩写，为用户信息哈希值。</para>
/// </summary>
public class XboxAuthToken
{
    /// <summary>
    /// 获取Xbox Live令牌的时间。
    /// </summary>
    public long IssueInstant { get; set; }

    /// <summary>
    /// Xbox Live令牌过期时间。
    /// </summary>
    public long NotAfter { get; set; }

    /// <summary>
    /// Xbox Live认证令牌
    /// </summary>
    /// <value>用于后续Minecraft认证的令牌字符串</value>
    [JsonPropertyName("Token")]
    public string Token { get; set; }

    /// <summary>
    /// 用户声明信息
    /// </summary>
    /// <value>包含用户哈希值的字典结构</value>
    [JsonPropertyName("DisplayClaims")]
    public Dictionary<string, List<XboxUserClaim>> DisplayClaims { get; set; }
}

/// <summary>
/// Xbox用户声明信息模型
/// </summary>
public class XboxUserClaim
{
    /// <summary>
    /// 用户哈希值（User Hash）
    /// </summary>
    /// <value>用于构造Minecraft认证令牌的唯一标识</value>
    [JsonPropertyName("uhs")]
    public string UserHash { get; set; }
}