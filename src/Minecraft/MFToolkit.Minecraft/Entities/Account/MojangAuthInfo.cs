using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account;

/// <summary>
/// Mojang认证信息
/// </summary>
public class MojangAuthInfo
{
    /// <summary>
    /// Minecraft访问令牌
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Minecraft刷新令牌
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
    public DateTimeOffset TokenIssuedAt { get; set; }
    
    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTimeOffset ExpiresAt => TokenIssuedAt.AddSeconds(ExpiresIn);
    
    /// <summary>
    /// 检查令牌是否已过期
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}

public class MojangAuthRequest
{
    [JsonPropertyName("identityToken")]
    public required string IdentityToken { get; set; }
}
public class MojangAuthRefreshRequest
{
    [JsonPropertyName("refreshToken")]
    public required string RefreshToken { get; set; }
}