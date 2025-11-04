using System;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Entities.Account;

namespace MFToolkit.Minecraft.Services.Auth.Interfaces;

/// <summary>
/// Minecraft认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 登录Minecraft账号
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    Task<MinecraftAccount> LoginAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 刷新Minecraft账号令牌
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新后的Minecraft账号</returns>
    Task<MinecraftAccount> RefreshTokenAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证Minecraft账号令牌
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>令牌是否有效</returns>
    Task<bool> ValidateTokenAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 登出Minecraft账号
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否登出成功</returns>
    Task<bool> LogoutAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 创建Minecraft会话
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="serverId">服务器ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建的会话</returns>
    Task<Session> CreateSessionAsync(MinecraftAccount account, string serverId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证Minecraft会话
    /// </summary>
    /// <param name="session">会话信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>会话是否有效</returns>
    Task<bool> ValidateSessionAsync(Session session, CancellationToken cancellationToken = default);
}
