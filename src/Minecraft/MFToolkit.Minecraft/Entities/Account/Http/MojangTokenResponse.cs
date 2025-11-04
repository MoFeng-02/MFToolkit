using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account.Http;
/// <summary>
/// Mojang令牌响应
/// </summary>
public class MojangTokenResponse
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
}

public class MojangLogingOut
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }
}