using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Skin;

namespace MFToolkit.Minecraft.Services.Skin;

/// <summary>
/// Minecraft皮肤服务接口
/// </summary>
public interface ISkinService
{
    /// <summary>
    /// 获取玩家皮肤
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>皮肤信息</returns>
    Task<SkinInfo> GetSkinAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 通过UUID获取玩家皮肤
    /// </summary>
    /// <param name="uuid">玩家UUID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>皮肤信息</returns>
    Task<SkinInfo> GetSkinByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 上传皮肤
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="skinStream">皮肤图片流</param>
    /// <param name="contentType">图片内容类型</param>
    /// <param name="isSlim">是否为纤细模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传后的皮肤信息</returns>
    Task<SkinInfo> UploadSkinAsync(MinecraftAccount account, Stream skinStream, string contentType, bool isSlim = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 通过URL上传皮肤
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="skinUrl">皮肤图片URL</param>
    /// <param name="isSlim">是否为纤细模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传后的皮肤信息</returns>
    Task<SkinInfo> UploadSkinFromUrlAsync(MinecraftAccount account, string skinUrl, bool isSlim = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 重置皮肤为默认值
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否重置成功</returns>
    Task<bool> ResetSkinAsync(MinecraftAccount account, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证皮肤文件
    /// </summary>
    /// <param name="skinStream">皮肤图片流</param>
    /// <param name="contentType">图片内容类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证结果</returns>
    Task<bool> ValidateSkinFileAsync(Stream skinStream, string contentType, CancellationToken cancellationToken = default);
}
