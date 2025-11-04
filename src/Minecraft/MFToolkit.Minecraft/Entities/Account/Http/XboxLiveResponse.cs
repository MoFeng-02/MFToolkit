using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account.Http;

/// <summary>
/// Xbox Live令牌响应
/// </summary>
public class XboxLiveTokenResponse
{
    /// <summary>
    /// 令牌
    /// </summary>
    [JsonPropertyName("Token")]
    public string? Token { get; set; }

    /// <summary>
    /// 令牌类型
    /// </summary>
    public string? TokenType { get; set; } = "JWT";

    /// <summary>
    /// 获取Xbox Live令牌的时间。
    /// </summary>
    [JsonPropertyName("IssueInstant")]
    public DateTimeOffset IssueInstant { get; set; }

    /// <summary>
    /// Xbox Live令牌过期时间。
    /// </summary>
    [JsonPropertyName("NotAfter")]
    public DateTimeOffset NotAfter { get; set; }

    /// <summary>
    /// 显示声明
    /// </summary>
    [JsonPropertyName("DisplayClaims")]
    public XboxDisplayClaimsResponse? DisplayClaims { get; set; }
}

/// <summary>
/// Xbox服务令牌响应
/// </summary>
public class XboxServiceTokenResponse
{
    /// <summary>
    /// 令牌
    /// </summary>
    [JsonPropertyName("Token")]
    public string? Token { get; set; }

    /// <summary>
    /// 令牌类型
    /// </summary>
    [JsonPropertyName("TokenType")]
    public string? TokenType { get; set; } = "JWT";

    /// <summary>
    /// 显示声明
    /// </summary>
    [JsonPropertyName("DisplayClaims")]
    public XboxDisplayClaimsResponse? DisplayClaims { get; set; }

    /// <summary>
    /// 不早于时间戳
    /// </summary>
    [JsonPropertyName("IssueInstant")]
    public DateTimeOffset IssueInstant { get; set; }

    /// <summary>
    /// 不晚于时间戳
    /// </summary>
    [JsonPropertyName("NotAfter")]
    public DateTimeOffset NotAfter { get; set; }
}

/// <summary>
/// Xbox显示声明响应
/// </summary>
public class XboxDisplayClaimsResponse
{
    /// <summary>
    /// Xbox用户信息
    /// </summary>
    [JsonPropertyName("xui")]
    public XboxUserInfoResponse[]? Xui { get; set; }
}

/// <summary>
/// Xbox用户信息响应
/// </summary>
public class XboxUserInfoResponse
{
    /// <summary>
    /// Xbox用户ID
    /// </summary>
    [JsonPropertyName("xid")]
    public string? Xid { get; set; }

    /// <summary>
    /// 玩家标签
    /// </summary>
    [JsonPropertyName("gtg")]
    public string? Gtg { get; set; }

    /// <summary>
    /// 玩家头像URL
    /// </summary>
    [JsonPropertyName("pgt")]
    public string? Pgt { get; set; }

    /// <summary>
    /// 用户哈希
    /// </summary>
    [JsonPropertyName("uhs")]
    public string? Uhs { get; set; }
}