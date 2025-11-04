using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account;

/// <summary>
/// 微软认证信息
/// </summary>
public class MicrosoftAuthInfo
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// 刷新令牌
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// 令牌类型
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    /// <summary>
    /// 过期时间（秒）
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// 令牌颁发时间
    /// </summary>
    [JsonPropertyName("issued_at")]
    public DateTimeOffset IssuedAt { get; set; }
    
    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTimeOffset ExpiresAt => IssuedAt.AddSeconds(ExpiresIn);
    
    /// <summary>
    /// 检查令牌是否已过期
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}
