using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Account.Http;

namespace MFToolkit.Minecraft.Services.Auth.Interfaces;

/// <summary>
/// Minecraft官方认证服务接口
/// </summary>
public interface IOfficialAuthService : IAuthService
{
    /// <summary>
    /// 获取微软授权URL
    /// </summary>
    /// <param name="state">状态参数</param>
    /// <param name="scope">权限范围</param>
    /// <returns>微软授权URL</returns>
    string GetMicrosoftAuthorizationUrl(string state, string[]? scope = null);
    
    /// <summary>
    /// 使用微软授权码登录
    /// </summary>
    /// <param name="code">微软授权码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    Task<MinecraftAccount> LoginWithMicrosoftCodeAsync(string code, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 使用微软刷新令牌登录
    /// </summary>
    /// <param name="refreshToken">微软刷新令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    Task<MinecraftAccount> LoginWithMicrosoftRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 使用Xbox令牌登录
    /// </summary>
    /// <param name="xboxToken">Xbox令牌</param>
    /// <param name="userHash">Xbox用户哈希</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    Task<MinecraftAccount> LoginWithXboxTokenAsync(string xboxToken, string userHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取微软设备代码
    /// </summary>
    /// <param name="scope">权限范围</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>设备代码结果</returns>
    Task<MicrosoftDeviceCodeResult> GetMicrosoftDeviceCodeAsync(string[]? scope = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用微软设备代码登录
    /// </summary>
    /// <param name="deviceCode">设备代码</param>
    /// <param name="interval">轮询间隔（秒）</param>
    /// <param name="expiresAt">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    Task<MinecraftAccount> LoginWithMicrosoftDeviceCodeAsync(string deviceCode, int interval, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用微软设备代码结果登录
    /// </summary>
    /// <param name="deviceCodeResult">设备代码结果</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    Task<MinecraftAccount> LoginWithMicrosoftDeviceCodeResultAsync(MicrosoftDeviceCodeResult deviceCodeResult, CancellationToken cancellationToken = default);
}
