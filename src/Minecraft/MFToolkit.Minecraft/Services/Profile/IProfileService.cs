using System;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Profile;

namespace MFToolkit.Minecraft.Services.Profile;

/// <summary>
/// Minecraft档案服务接口
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// 获取玩家档案
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>玩家档案</returns>
    Task<PlayerProfile> GetProfileAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 通过UUID获取玩家档案
    /// </summary>
    /// <param name="uuid">玩家UUID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>玩家档案</returns>
    Task<PlayerProfile> GetProfileByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 通过名称获取玩家档案
    /// </summary>
    /// <param name="name">玩家名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>玩家档案</returns>
    Task<PlayerProfile> GetProfileByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更改玩家名称
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="newName">新玩家名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新后的玩家档案</returns>
    Task<PlayerProfile> ChangePlayerNameAsync(MinecraftAccount account, string newName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查名称是否可用
    /// </summary>
    /// <param name="name">玩家名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>名称是否可用</returns>
    Task<bool> CheckNameAvailabilityAsync(string name, CancellationToken cancellationToken = default);
}
