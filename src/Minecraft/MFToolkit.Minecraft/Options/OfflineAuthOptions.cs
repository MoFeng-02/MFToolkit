using System;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// Minecraftraft离线认证配置
/// </summary>
public class OfflineAuthOptions
{
    /// <summary>
    /// 配置项名称
    /// </summary>
    public const string OfflineAuth = "Minecraft:OfflineAuth";
    
    /// <summary>
    /// 默认玩家名称
    /// </summary>
    public string DefaultPlayerName { get; set; } = "Player";
    
    /// <summary>
    /// 是否允许自定义UUID
    /// </summary>
    public bool AllowCustomUuid { get; set; } = true;
    
    /// <summary>
    /// 默认UUID版本
    /// </summary>
    public int DefaultUuidVersion { get; set; } = 4;
    
    /// <summary>
    /// 会话过期时间（小时）
    /// </summary>
    public int SessionExpirationHours { get; set; } = 24;
    
    /// <summary>
    /// 是否自动生成皮肤
    /// </summary>
    public bool AutoGenerateSkin { get; set; } = true;
    
    /// <summary>
    /// 默认皮肤类型
    /// </summary>
    public string DefaultSkinType { get; set; } = "classic";
}
