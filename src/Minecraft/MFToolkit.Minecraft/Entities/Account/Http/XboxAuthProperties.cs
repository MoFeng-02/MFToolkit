using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account.Http;

/// <summary>
/// Xbox认证属性配置模型
/// </summary>
public class XboxAuthProperties
{
    /// <summary>
    /// 认证方法
    /// </summary>
    /// <value>固定为"RPS"（Relying Party Suite）</value>
    [JsonPropertyName("AuthMethod")]
    public string AuthMethod { get; set; } = "RPS";

    /// <summary>
    /// 认证站点名称
    /// </summary>
    /// <value>固定为"user.auth.xboxlive.com"</value>
    [JsonPropertyName("SiteName")]
    public string SiteName { get; set; } = "user.auth.xboxlive.com";

    /// <summary>
    /// RPS认证票据
    /// </summary>
    /// <value>格式为"d={微软访问令牌}"</value>
    [JsonPropertyName("RpsTicket")]
    public string? RpsTicket { get; set; }
}

/// <summary>
/// XSTS 认证 Properties
/// </summary>
public class XboxXstsAuthProperties
{
    /// <summary>
    /// 认证方法
    /// </summary>
    /// <value>固定为 RETAIL</value>
    [JsonPropertyName("SandboxId")]
    public string SandboxId { get; set; } = "RETAIL";

    /// <summary>
    /// 上面得到的XBL令牌 <see cref="XboxAuthRequest.Token"/>
    /// <example>
    /// "UserTokens": [
    /// XboxAuthRequest.Token
    /// ]
    /// </example>
    /// </summary>
    [JsonPropertyName("UserTokens")]
    public List<string> UserTokens { get; set; } = [];
}