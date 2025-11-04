using System;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Entities.Account;

namespace MFToolkit.Minecraft.Services.Auth.Interfaces;

/// <summary>
/// Minecraft第三方认证服务接口
/// </summary>
public interface IThirdPartyAuthService : IAuthService
{
    /// <summary>
    /// 使用第三方平台令牌登录
    /// </summary>
    /// <param name="platform">第三方平台名称</param>
    /// <param name="token">第三方平台令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    Task<MinecraftAccount> LoginWithThirdPartyTokenAsync(string platform, string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取支持的第三方平台列表
    /// </summary>
    /// <returns>支持的第三方平台列表</returns>
    string[] GetSupportedPlatforms();
    
    /// <summary>
    /// 检查平台是否支持
    /// </summary>
    /// <param name="platform">第三方平台名称</param>
    /// <returns>是否支持该平台</returns>
    bool IsPlatformSupported(string platform);
}
