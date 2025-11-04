using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Cape;

namespace MFToolkit.Minecraft.Services.Cape;

/// <summary>
/// Minecraft披风服务接口
/// </summary>
public interface ICapeService
{
    /// <summary>
    /// 获取玩家披风
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>披风信息</returns>
    Task<CapeInfo> GetCapeAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 通过UUID获取玩家披风
    /// </summary>
    /// <param name="uuid">玩家UUID</param>
    /// <param name="cancellationToken">取消令牌令牌</param>
    /// <returns>披风信息</returns>
    Task<CapeInfo> GetCapeByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 上传披风
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="capeStream">披风图片流</param>
    /// <param name="contentType">图片内容类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传后的披风信息</returns>
    Task<CapeInfo> UploadCapeAsync(MinecraftAccount account, Stream capeStream, string contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 通过URL上传披风
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="capeUrl">披风图片URL</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传后的披风信息</returns>
    Task<CapeInfo> UploadCapeFromUrlAsync(MinecraftAccount account, string capeUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 重置披为默认值
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否重置成功</returns>
    Task<bool> ResetCapeAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证披风文件
    /// </summary>
    /// <param name="capeStream">披风图片流</param>
    /// <param name="contentType">图片内容类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证结果</returns>
    Task<bool> ValidateCapeFileAsync(Stream capeStream, string contentType, CancellationToken cancellationToken = default);
}
