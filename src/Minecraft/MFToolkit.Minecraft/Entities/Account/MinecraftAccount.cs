namespace MFToolkit.Minecraft.Entities.Account;

/// <summary>
/// Minecraft统一账号实体
/// </summary>
public class MinecraftAccount
{
    /// <summary>
    /// 账号ID
    /// </summary>
    public Guid AccountId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 账号类型
    /// </summary>
    public AccountType Type { get; set; } = AccountType.Unknown;
    
    /// <summary>
    /// 玩家名称
    /// </summary>
    public string? PlayerName { get; set; }
    
    /// <summary>
    /// 玩家UUID
    /// </summary>
    public Guid PlayerUuid { get; set; }
    
    /// <summary>
    /// 微软认证信息
    /// </summary>
    public MicrosoftAuthInfo? MicrosoftAuthInfo { get; set; }
    
    /// <summary>
    /// Xbox认证信息
    /// </summary>
    public XboxAuthInfo? XboxAuthInfo { get; set; }
    
    /// <summary>
    /// Mojang认证信息
    /// </summary>
    public MojangAuthInfo? MojangAuthInfo { get; set; }
    
    /// <summary>
    /// 当前会话信息
    /// </summary>
    public Session? CurrentSession { get; set; }
    
    /// <summary>
    /// 账号属性列表
    /// </summary>
    public List<ProfileProperty>? Properties { get; set; }
    
    /// <summary>
    /// 账号创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// 账号最后更新时间
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// 检查账号是否已过期
    /// </summary>
    public bool IsExpired => Type switch
    {
        AccountType.Microsoft => MicrosoftAuthInfo?.IsExpired ?? true,
        AccountType.Xbox => XboxAuthInfo?.IsExpired ?? true,
        AccountType.Mojang => MojangAuthInfo?.IsExpired ?? true,
        AccountType.Offline => false,
        _ => true
    };
}

/// <summary>
/// Minecraft账号类型
/// </summary>
public enum AccountType
{
    /// <summary>
    /// 未知类型
    /// </summary>
    Unknown,
    
    /// <summary>
    /// 离线账号
    /// </summary>
    Offline,
    
    /// <summary>
    /// Mojang账号
    /// </summary>
    Mojang,
    
    /// <summary>
    /// 微软账号
    /// </summary>
    Microsoft,
    
    /// <summary>
    /// Xbox账号
    /// </summary>
    Xbox,
    
    /// <summary>
    /// 第三方账号
    /// </summary>
    ThirdParty
}
