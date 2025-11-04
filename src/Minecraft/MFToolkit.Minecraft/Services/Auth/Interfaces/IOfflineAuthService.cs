using System;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Entities.Account;

namespace MFToolkit.Minecraft.Services.Auth.Interfaces;

/// <summary>
/// Minecraft离线认证服务接口
/// </summary>
public interface IOfflineAuthService : IAuthService
{
    /// <summary>
    /// 创建离线账号
    /// </summary>
    /// <param name="playerName">玩家名称</param>
    /// <param name="uuid">自定义UUID（可选）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>创建的离线账号</returns>
    Task<MinecraftAccount> CreateOfflineAccountAsync(string playerName, Guid? uuid = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 生成离线UUID
    /// </summary>
    /// <param name="playerName">玩家名称</param>
    /// <param name="version">UUID版本</param>
    /// <returns>生成的UUID</returns>
    Guid GenerateOfflineUuid(string playerName, int version = 4);
}
