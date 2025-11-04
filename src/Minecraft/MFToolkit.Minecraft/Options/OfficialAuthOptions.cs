using System.ComponentModel.DataAnnotations;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// Minecraft官方认证认证配置
/// </summary>
public class OfficialAuthOptions
{
    /// <summary>
    /// 配置项名称
    /// </summary>
    public const string OfficialAuth = "Minecraft:OfficialAuth";
    
    /// <summary>
    /// Microsoft客户端ID
    /// </summary>
    public string? ClientId { get; set; }
    
    /// <summary>
    /// 客户端密钥
    /// </summary>
    public string? ClientSecret { get; set; }
    
    /// <summary>
    /// 重定向URI
    /// </summary>
    public string? RedirectUri { get; set; }
    
    /// <summary>
    /// 授权范围
    /// </summary>
    public string[]? Scopes { get; set; }
    
    /// <summary>
    /// 认证超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// 最大重试次数
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;
    
    /// <summary>
    /// 重试间隔（毫秒）
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;
}
