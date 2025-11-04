#nullable disable
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account.Http;

/// <summary>
/// Xbox Live认证请求负载模型
/// </summary>
/// <remarks>
/// 遵循Xbox Live认证协议规范，参考：
/// https://learn.microsoft.com/xbox-live/authentication/oauth-authorize
/// </remarks>
public class XboxAuthRequest
{
    /// <summary>
    /// 认证属性配置
    /// </summary>
    /// <value>包含认证方法和票据的配置对象</value>
    [JsonPropertyName("Properties")]
    public XboxAuthProperties Properties { get; set; }

    /// <summary>
    /// 依赖方标识
    /// </summary>
    /// <value>固定为"http://auth.xboxlive.com"</value>
    [JsonPropertyName("RelyingParty")]
    public string RelyingParty { get; set; } = "http://auth.xboxlive.com";

    /// <summary>
    /// 令牌类型
    /// </summary>
    /// <value>固定为"JWT"格式</value>
    [JsonPropertyName("TokenType")]
    public string TokenType { get; set; } = "JWT";
}

/// <summary>
/// Xbox XSTS认证请求负载模型
/// </summary>
public class XboxXstsAuthRequest
{
    /// <summary>
    /// 认证属性配置
    /// </summary>
    /// <value>包含认证方法和票据的配置对象</value>
    [JsonPropertyName("Properties")]
    public XboxXstsAuthProperties Properties { get; set; }

    /// <summary>
    /// 依赖方标识
    /// </summary>
    /// <value>固定为"rp://api.minecraftservices.com/"</value>
    [JsonPropertyName("RelyingParty")]
    public string RelyingParty { get; set; } = "rp://api.minecraftservices.com/";

    /// <summary>
    /// 令牌类型
    /// </summary>
    /// <value>固定为"JWT"格式</value>
    [JsonPropertyName("TokenType")]
    public string TokenType { get; set; } = "JWT";
}