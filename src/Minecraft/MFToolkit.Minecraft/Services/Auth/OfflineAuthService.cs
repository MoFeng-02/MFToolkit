using System.Security.Cryptography;
using System.Text;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Auth.Interfaces;
using Microsoft.Extensions.Options;

namespace MFToolkit.Minecraft.Services.Auth;

/// <summary>
/// Minecraft离线认证服务实现
/// </summary>
public class OfflineAuthService : IOfflineAuthService
{
    private readonly OfflineAuthOptions _options;
    
    /// <summary>
    /// 初始化离线认证服务
    /// </summary>
    /// <param name="options">离线认证配置</param>
    public OfflineAuthService(IOptions<OfflineAuthOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }
    
    /// <summary>
    /// 登录Minecraft账号
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    public async Task<MinecraftAccount> LoginAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
        
        if (account.Type != AccountType.Offline)
            throw new InvalidOperationException("Account type must be Offline for offline authentication.");
        
        // 离线登录不需要网络请求，直接创建会话
        account.CurrentSession = await CreateSessionAsync(account, "OFFLINE_MODE", cancellationToken);
        account.LastUpdatedAt = DateTimeOffset.UtcNow;
        
        return account;
    }
    
    /// <summary>
    /// 刷新Minecraft账号令牌
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新后的Minecraft账号</returns>
    public Task<MinecraftAccount> RefreshTokenAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
        
        if (account.Type != AccountType.Offline)
            throw new InvalidOperationException("Account type must be Offline for offline authentication.");
        
        // 离线账号不需要刷新令牌
        account.LastUpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(account);
    }
    
    /// <summary>
    /// 验证Minecraft账号令牌
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>令牌是否有效</returns>
    public Task<bool> ValidateTokenAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
        
        if (account.Type != AccountType.Offline)
            return Task.FromResult(false);
        
        // 离线账号令牌始终有效
        return Task.FromResult(true);
    }
    
    /// <summary>
    /// 登出Minecraft账号
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否登出成功</returns>
    public Task<bool> LogoutAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
        
        if (account.Type != AccountType.Offline)
            return Task.FromResult(false);
        
        // 离线账号登出只需清除会话
        account.CurrentSession = null;
        account.LastUpdatedAt = DateTimeOffset.UtcNow;
        return Task.FromResult(true);
    }
    
    /// <summary>
    /// 创建Minecraft会话
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="serverId">服务器ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建的会话</returns>
    public Task<Session> CreateSessionAsync(MinecraftAccount account, string serverId, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
        
        if (string.IsNullOrEmpty(serverId))
            throw new ArgumentException("Server ID cannot be null or empty.", nameof(serverId));
        
        if (account.Type != AccountType.Offline)
            throw new InvalidOperationException("Account type must be Offline for offline authentication.");
        
        if (string.IsNullOrEmpty(account.PlayerName))
            throw new InvalidOperationException("Player name is required for offline authentication.");
        
        // 生成会话ID
        var sessionId = Guid.NewGuid().ToString("N");
        
        // 创建会话
        var session = new Session
        {
            ServerId = serverId,
            SessionId = sessionId,
            PlayerName = account.PlayerName,
            PlayerUuid = account.PlayerUuid,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(_options.SessionExpirationHours)
        };
        
        return Task.FromResult(session);
    }
    
    /// <summary>
    /// 验证Minecraft会话
    /// </summary>
    /// <param name="session">会话信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话是否有效</returns>
    public Task<bool> ValidateSessionAsync(Session session, CancellationToken cancellationToken = default)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));
        
        // 离线会话只需检查是否过期
        return Task.FromResult(!session.IsExpired);
    }
    
    /// <summary>
    /// 创建离线账号
    /// </summary>
    /// <param name="playerName">玩家名称</param>
    /// <param name="uuid">自定义UUID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建的离线账号</returns>
    public Task<MinecraftAccount> CreateOfflineAccountAsync(string playerName, Guid? uuid = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(playerName))
            throw new ArgumentException("Player name cannot be null or empty.", nameof(playerName));
        
        // 如果没有提供UUID，则生成一个
        var accountUuid = uuid ?? GenerateOfflineUuid(playerName, _options.DefaultUuidVersion);
        
        // 创建离线账号
        var account = new MinecraftAccount
        {
            Type = AccountType.Offline,
            PlayerName = playerName,
            PlayerUuid = accountUuid,
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
        
        return Task.FromResult(account);
    }
    
    /// <summary>
    /// 生成离线UUID
    /// </summary>
    /// <param name="playerName">玩家名称</param>
    /// <param name="version">UUID版本</param>
    /// <returns>生成的UUID</returns>
    public Guid GenerateOfflineUuid(string playerName, int version = 4)
    {
        if (string.IsNullOrEmpty(playerName))
            throw new ArgumentException("Player name cannot be null or empty.", nameof(playerName));
        
        if (version != 3 && version != 4)
            throw new ArgumentOutOfRangeException(nameof(version), "Only version 3 and 4 UUIDs are supported.");
        
        if (version == 3)
        {
            // 版本3 UUID（基于名称的MD5哈希）
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes($"OfflinePlayer:{playerName}");
            var hashBytes = md5.ComputeHash(inputBytes);
            
            // 设置UUID版本和变体
            hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30); // 版本3
            hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80); // RFC4122变体
            
            return new Guid(hashBytes);
        }
        else
        {
            // 版本4 UUID（随机生成）
            return Guid.NewGuid();
        }
    }
}
