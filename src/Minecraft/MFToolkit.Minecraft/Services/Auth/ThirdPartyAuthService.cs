using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Services.Auth.Interfaces;

namespace MFToolkit.Minecraft.Services.Auth;

/// <summary>
/// Minecraft第三方认证服务实现
/// </summary>
public class ThirdPartyAuthService : IThirdPartyAuthService
{
    private readonly Dictionary<string, IThirdPartyAuthProvider> _providers;
    
    /// <summary>
    /// 初始化第三方认证服务
    /// </summary>
    /// <param name="providers">第三方认证提供程序</param>
    public ThirdPartyAuthService(IEnumerable<IThirdPartyAuthProvider> providers)
    {
        _providers = new Dictionary<string, IThirdPartyAuthProvider>(StringComparer.OrdinalIgnoreCase);
        
        if (providers != null)
        {
            foreach (var provider in providers)
            {
                if (string.IsNullOrEmpty(provider.PlatformName))
                    throw new ArgumentException("Third party auth provider must have a non-empty platform name.");
                
                _providers[provider.PlatformName] = provider;
            }
        }
    }
    
    /// <summary>
    /// 使用第三方平台令牌登录
    /// </summary>
    /// <param name="platform">第三方平台名称</param>
    /// <param name="token">第三方平台令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    public async Task<MinecraftAccount> LoginWithThirdPartyTokenAsync(string platform, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(platform))
            throw new ArgumentException("Platform cannot be null or empty.", nameof(platform));
        
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        
        if (!_providers.TryGetValue(platform, out var provider))
            throw new NotSupportedException($"Third party authentication for platform '{platform}' is not supported.");
        
        // 使用第三方提供程序验证令牌并获取用户信息
        var userInfo = await provider.ValidateTokenAsync(token, cancellationToken);
        
        // 创建Minecraft账号
        var account = new MinecraftAccount
        {
            Type = AccountType.ThirdParty,
            PlayerName = userInfo.PlayerName,
            PlayerUuid = userInfo.PlayerUuid,
            Properties = userInfo.Properties,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
        
        // 创建会话
        account.CurrentSession = await CreateSessionAsync(account, $"THIRD_PARTY_{platform.ToUpper()}", cancellationToken);
        
        return account;
    }
    
    /// <summary>
    /// 获取支持的第三方平台列表
    /// </summary>
    /// <returns>支持的第三方平台列表</returns>
    public string[] GetSupportedPlatforms()
    {
        return new List<string>(_providers.Keys).ToArray();
    }
    
    /// <summary>
    /// 检查平台是否支持
    /// </summary>
    /// <param name="platform">第三方平台名称</param>
    /// <returns>是否支持该平台</returns>
    public bool IsPlatformSupported(string platform)
    {
        if (string.IsNullOrEmpty(platform))
            return false;
        
        return _providers.ContainsKey(platform);
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
        
        if (account.Type != AccountType.ThirdParty)
            throw new InvalidOperationException("Account type must be ThirdParty for third party authentication.");
        
        // 第三方登录只需创建会话
        account.CurrentSession = await CreateSessionAsync(account, "THIRD_PARTY_MODE", cancellationToken);
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
        
        if (account.Type != AccountType.ThirdParty)
            throw new InvalidOperationException("Account type must be ThirdParty for third party authentication.");
        
        // 第三方账号刷新令牌需要具体实现
        throw new NotImplementedException("Third party token refresh is not implemented.");
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
        
        if (account.Type != AccountType.ThirdParty)
            return Task.FromResult(false);
        
        // 第三方账号令牌验证需要具体实现
        throw new NotImplementedException("Third party token validation is not implemented.");
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
        
        if (account.Type != AccountType.ThirdParty)
            return Task.FromResult(false);
        
        // 第三方账号登出只需清除会话
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
        
        if (account.Type != AccountType.ThirdParty)
            throw new InvalidOperationException("Account type must be ThirdParty for third party authentication.");
        
        if (string.IsNullOrEmpty(account.PlayerName))
            throw new InvalidOperationException("Player name is required for third party authentication.");
        
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
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24) // 假设第三方会话有效期为24小时
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
        
        // 第三方会话只需检查是否过期
        return Task.FromResult(!session.IsExpired);
    }
}

/// <summary>
/// 第三方认证提供程序接口
/// </summary>
public interface IThirdPartyAuthProvider
{
    /// <summary>
    /// 平台名称
    /// </summary>
    string PlatformName { get; }
    
    /// <summary>
    /// 验证第三方令牌
    /// </summary>
    /// <param name="token">第三方令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户信息</returns>
    Task<ThirdPartyUserInfo> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}

/// <summary>
/// 第三方用户信息
/// </summary>
public class ThirdPartyUserInfo
{
    /// <summary>
    /// 玩家名称
    /// </summary>
    public string? PlayerName { get; set; }
    
    /// <summary>
    /// 玩家UUID
    /// </summary>
    public Guid PlayerUuid { get; set; }
    
    /// <summary>
    /// 账号属性列表
    /// </summary>
    public List<ProfileProperty>? Properties { get; set; }
}
