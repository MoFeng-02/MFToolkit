using System;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account;

/// <summary>
/// Xbox认证信息
/// </summary>
public class XboxAuthInfo
{
    /// <summary>
    /// Xbox用户哈希
    /// </summary>
    [JsonPropertyName("uhs")]
    public string? UserHash { get; set; }
    
    /// <summary>
    /// Xbox访问令牌
    /// </summary>
    [JsonPropertyName("AccessToken")]
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Xbox令牌类型
    /// </summary>
    [JsonPropertyName("TokenType")]
    public string? TokenType { get; set; }
    
    /// <summary>
    /// Xbox显示名称
    /// </summary>
    [JsonPropertyName("DisplayClaims")]
    public XboxDisplayClaims? DisplayClaims { get; set; }
    
    /// <summary>
    /// 令牌颁发时间
    /// </summary>
    public DateTimeOffset TokenIssuedAt { get; set; }
    
    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
    
    /// <summary>
    /// 检查令牌是否已过期
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}

/// <summary>
/// Xbox显示声明
/// </summary>
public class XboxDisplayClaims
{
    /// <summary>
    /// Xbox用户信息
    /// </summary>
    [JsonPropertyName("xui")]
    public XboxUserInfo[]? UserInfo { get; set; }
}

/// <summary>
/// Xbox用户信息
/// </summary>
public class XboxUserInfo
{
    /// <summary>
    /// Xbox用户ID
    /// </summary>
    [JsonPropertyName("xid")]
    public string? XboxId { get; set; }
    
    /// <summary>
    /// 玩家标签
    /// </summary>
    [JsonPropertyName("gtg")]
    public string? GamerTag { get; set; }
    
    /// <summary>
    /// 玩家头像URL
    /// </summary>
    [JsonPropertyName("pgt")]
    public string? GamerPictureUrl { get; set; }
}
