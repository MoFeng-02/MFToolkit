using System;
using System.Text.Json.Serialization;

namespace MFToolkit.Minecraft.Entities.Account;

/// <summary>
/// Minecraft会话信息
/// </summary>
public class Session
{
    /// <summary>
    /// 服务器ID
    /// </summary>
    public string? ServerId { get; set; }
    
    /// <summary>
    /// 会话ID
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// 玩家名称
    /// </summary>
    public string? PlayerName { get; set; }
    
    /// <summary>
    /// 玩家UUID
    /// </summary>
    public Guid PlayerUuid { get; set; }
    
    /// <summary>
    /// 会话创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// 会话过期时间
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
    
    /// <summary>
    /// 检查会话是否已过期
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
}
