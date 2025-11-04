using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Account.Http;
using MFToolkit.Minecraft.Exceptions.Auths;
using MFToolkit.Minecraft.Internal.Auth;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Auth.Interfaces;
using Microsoft.Extensions.Options;

namespace MFToolkit.Minecraft.Services.Auth;

/// <summary>
/// Minecraft官方认证服务实现
/// </summary>
public class OfficialAuthService : IOfficialAuthService
{
    private readonly MicrosoftAuthHandler _microsoftAuthHandler;
    private readonly XboxAuthHandler _xboxAuthHandler;
    private readonly MojangAuthHandler _mojangAuthHandler;
    private readonly OfficialAuthOptions _options;

    /// <summary>
    /// 初始化官方认证服务
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="options">官方认证配置</param>
    public OfficialAuthService(HttpClient httpClient, IOptions<OfficialAuthOptions>? options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _microsoftAuthHandler = new MicrosoftAuthHandler(httpClient ?? throw new ArgumentNullException(nameof(httpClient)), options);
        _xboxAuthHandler = new XboxAuthHandler(httpClient);
        _mojangAuthHandler = new MojangAuthHandler(httpClient);
    }

    /// <summary>
    /// 获取微软授权URL
    /// </summary>
    /// <param name="state">状态参数</param>
    /// <param name="scope">权限范围</param>
    /// <returns>微软授权URL</returns>
    public string GetMicrosoftAuthorizationUrl(string state, string[]? scope = null)
    {
        return _microsoftAuthHandler.GetAuthorizationUrl(state, scope);
    }
    /// <summary>
    /// 获取微软设备代码
    /// </summary>
    /// <param name="scope">权限范围</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>设备代码结果</returns>
    public async Task<MicrosoftDeviceCodeResult> GetMicrosoftDeviceCodeAsync(string[]? scope = null, CancellationToken cancellationToken = default)
    {
        return await _microsoftAuthHandler.GetDeviceCodeAsync(scope, cancellationToken);
    }

    /// <summary>
    /// 使用微软设备代码登录
    /// </summary>
    /// <param name="deviceCode">设备代码</param>
    /// <param name="interval">轮询间隔（秒）</param>
    /// <param name="expiresAt">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    public async Task<MinecraftAccount> LoginWithMicrosoftDeviceCodeAsync(string deviceCode, int interval, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
    {
        MicrosoftAuthInfo? microsoftAuthInfo = null;

        // 循环轮询直到获取到令牌或超时
        while (!cancellationToken.IsCancellationRequested && DateTimeOffset.UtcNow < expiresAt)
        {
            try
            {
                // 尝试使用设备代码获取令牌
                microsoftAuthInfo = await _microsoftAuthHandler.GetTokenByDeviceCodeAsync(deviceCode, cancellationToken);
                break;
            }
            catch (AuthorizationPendingException)
            {
                // 授权仍在处理中，等待指定间隔后重试
                await Task.Delay(TimeSpan.FromSeconds(interval), cancellationToken);
            }
            catch (ExpiredTokenException)
            {
                // 设备代码已过期
                throw new InvalidOperationException("The device code has expired. Please request a new one.");
            }
            catch (AuthorizationDeclinedException)
            {
                // 用户拒绝授权
                throw new InvalidOperationException("The user has declined the authorization request.");
            }
            catch (BadVerificationCodeException)
            {
                // 验证代码无效
                throw new InvalidOperationException("The verification code is invalid.");
            }
        }

        if (microsoftAuthInfo == null)
            throw new InvalidOperationException("Failed to obtain Microsoft token using device code.");

        // 2. 使用微软令牌获取Xbox Live令牌
        var xboxLiveAuthInfo = await _xboxAuthHandler.GetXboxLiveTokenAsync(microsoftAuthInfo.AccessToken!, cancellationToken);

        // 3. 使用Xbox Live令牌获取Xbox服务令牌
        var xboxServiceAuthInfo = await _xboxAuthHandler.GetXboxServiceTokenAsync(
            xboxLiveAuthInfo.AccessToken!,
            xboxLiveAuthInfo.UserHash!,
            cancellationToken);

        // 4. 使用Xbox服务令牌登录Mojang
        var mojangAuthInfo = await _mojangAuthHandler.LoginWithXboxAsync(
            xboxServiceAuthInfo.AccessToken!,
            xboxServiceAuthInfo.UserHash!,
            cancellationToken);

        // 5. 创建Minecraft账号
        var account = new MinecraftAccount
        {
            Type = AccountType.Microsoft,
            PlayerName = xboxLiveAuthInfo.DisplayClaims?.UserInfo?[0]?.GamerTag,
            MicrosoftAuthInfo = microsoftAuthInfo,
            XboxAuthInfo = xboxServiceAuthInfo,
            MojangAuthInfo = mojangAuthInfo,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        // 6. 创建会话
        account.CurrentSession = await CreateSessionAsync(account, "OFFICIAL_MODE", cancellationToken);

        return account;
    }

    /// <summary>
    /// 使用微软设备代码结果登录
    /// </summary>
    /// <param name="deviceCodeResult">设备代码结果</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    public async Task<MinecraftAccount> LoginWithMicrosoftDeviceCodeResultAsync(MicrosoftDeviceCodeResult deviceCodeResult, CancellationToken cancellationToken = default)
    {
        if (deviceCodeResult == null)
            throw new ArgumentNullException(nameof(deviceCodeResult));

        if (string.IsNullOrEmpty(deviceCodeResult.DeviceCode))
            throw new ArgumentException("Device code cannot be null or empty.", nameof(deviceCodeResult));

        return await LoginWithMicrosoftDeviceCodeAsync(
            deviceCodeResult.DeviceCode,
            deviceCodeResult.Interval,
            deviceCodeResult.ExpiresAt,
            cancellationToken);
    }
    /// <summary>
    /// 使用微软授权码登录
    /// </summary>
    /// <param name="code">微软授权码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    public async Task<MinecraftAccount> LoginWithMicrosoftCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        // 1. 使用授权码获取微软令牌
        var microsoftAuthInfo = await _microsoftAuthHandler.GetTokenByCodeAsync(code, cancellationToken);

        // 2. 使用微软令牌获取Xbox Live令牌
        var xboxLiveAuthInfo = await _xboxAuthHandler.GetXboxLiveTokenAsync(microsoftAuthInfo.AccessToken!, cancellationToken);

        // 3. 使用Xbox Live令牌获取Xbox服务令牌
        var xboxServiceAuthInfo = await _xboxAuthHandler.GetXboxServiceTokenAsync(
            xboxLiveAuthInfo.AccessToken!,
            xboxLiveAuthInfo.UserHash!,
            cancellationToken);

        // 4. 使用Xbox服务令牌登录Mojang
        var mojangAuthInfo = await _mojangAuthHandler.LoginWithXboxAsync(
            xboxServiceAuthInfo.AccessToken!,
            xboxServiceAuthInfo.UserHash!,
            cancellationToken);

        // 5. 创建Minecraft账号
        var account = new MinecraftAccount
        {
            Type = AccountType.Microsoft,
            PlayerName = xboxLiveAuthInfo.DisplayClaims?.UserInfo?[0]?.GamerTag,
            MicrosoftAuthInfo = microsoftAuthInfo,
            XboxAuthInfo = xboxServiceAuthInfo,
            MojangAuthInfo = mojangAuthInfo,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        // 6. 创建会话
        account.CurrentSession = await CreateSessionAsync(account, "OFFICIAL_MODE", cancellationToken);

        return account;
    }

    /// <summary>
    /// 使用微软刷新令牌登录
    /// </summary>
    /// <param name="refreshToken">微软刷新令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    public async Task<MinecraftAccount> LoginWithMicrosoftRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // 1. 使用刷新令牌获取微软令牌
        var microsoftAuthInfo = await _microsoftAuthHandler.GetTokenByRefreshTokenAsync(refreshToken, cancellationToken);

        // 2. 使用微软令牌获取Xbox Live令牌
        var xboxLiveAuthInfo = await _xboxAuthHandler.GetXboxLiveTokenAsync(microsoftAuthInfo.AccessToken!, cancellationToken);

        // 3. 使用Xbox Live令牌获取Xbox服务令牌
        var xboxServiceAuthInfo = await _xboxAuthHandler.GetXboxServiceTokenAsync(
            xboxLiveAuthInfo.AccessToken!,
            xboxLiveAuthInfo.UserHash!,
            cancellationToken);

        // 4. 使用Xbox服务令牌登录Mojang
        var mojangAuthInfo = await _mojangAuthHandler.LoginWithXboxAsync(
            xboxServiceAuthInfo.AccessToken!,
            xboxServiceAuthInfo.UserHash!,
            cancellationToken);

        // 5. 创建Minecraft账号
        var account = new MinecraftAccount
        {
            Type = AccountType.Microsoft,
            PlayerName = xboxLiveAuthInfo.DisplayClaims?.UserInfo?[0]?.GamerTag,
            MicrosoftAuthInfo = microsoftAuthInfo,
            XboxAuthInfo = xboxServiceAuthInfo,
            MojangAuthInfo = mojangAuthInfo,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        // 6. 创建会话
        account.CurrentSession = await CreateSessionAsync(account, "OFFICIAL_MODE", cancellationToken);

        return account;
    }

    /// <summary>
    /// 使用Xbox令牌登录
    /// </summary>
    /// <param name="xboxToken">Xbox令牌</param>
    /// <param name="userHash">Xbox用户哈希</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录后的Minecraft账号</returns>
    public async Task<MinecraftAccount> LoginWithXboxTokenAsync(string xboxToken, string userHash, CancellationToken cancellationToken = default)
    {
        // 1. 使用Xbox令牌登录Mojang
        var mojangAuthInfo = await _mojangAuthHandler.LoginWithXboxAsync(xboxToken, userHash, cancellationToken);

        // 2. 创建Minecraft账号
        var account = new MinecraftAccount
        {
            Type = AccountType.Xbox,
            XboxAuthInfo = new XboxAuthInfo
            {
                AccessToken = xboxToken,
                UserHash = userHash,
                TokenIssuedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24) // 假设Xbox令牌有效期为24小时
            },
            MojangAuthInfo = mojangAuthInfo,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        // 3. 创建会话
        account.CurrentSession = await CreateSessionAsync(account, "OFFICIAL_MODE", cancellationToken);

        return account;
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

        if (account.Type == AccountType.Microsoft && !string.IsNullOrEmpty(account.MicrosoftAuthInfo?.RefreshToken))
        {
            // 使用微软刷新令牌登录
            var refreshedAccount = await LoginWithMicrosoftRefreshTokenAsync(account.MicrosoftAuthInfo.RefreshToken, cancellationToken);
            account.MicrosoftAuthInfo = refreshedAccount.MicrosoftAuthInfo;
            account.XboxAuthInfo = refreshedAccount.XboxAuthInfo;
            account.MojangAuthInfo = refreshedAccount.MojangAuthInfo;
            account.CurrentSession = refreshedAccount.CurrentSession;
            account.LastUpdatedAt = DateTimeOffset.UtcNow;
            return account;
        }
        else if (account.Type == AccountType.Xbox && !string.IsNullOrEmpty(account.XboxAuthInfo?.AccessToken) && !string.IsNullOrEmpty(account.XboxAuthInfo.UserHash))
        {
            // 使用Xbox令牌登录
            var refreshedAccount = await LoginWithXboxTokenAsync(account.XboxAuthInfo.AccessToken, account.XboxAuthInfo.UserHash, cancellationToken);
            account.MojangAuthInfo = refreshedAccount.MojangAuthInfo;
            account.CurrentSession = refreshedAccount.CurrentSession;
            account.LastUpdatedAt = DateTimeOffset.UtcNow;
            return account;
        }
        else if (account.Type == AccountType.Mojang && !string.IsNullOrEmpty(account.MojangAuthInfo?.RefreshToken))
        {
            // 刷新Mojang令牌
            account.MojangAuthInfo = await _mojangAuthHandler.RefreshTokenAsync(account.MojangAuthInfo.RefreshToken, cancellationToken);
            account.CurrentSession = await CreateSessionAsync(account, "OFFICIAL_MODE", cancellationToken);
            account.LastUpdatedAt = DateTimeOffset.UtcNow;
            return account;
        }

        throw new InvalidOperationException("Unsupported account type or missing credentials.");
    }

    /// <summary>
    /// 刷新Minecraft账号令牌
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新后的Minecraft账号</returns>
    public async Task<MinecraftAccount> RefreshTokenAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (account.Type == AccountType.Microsoft && account.MicrosoftAuthInfo != null)
        {
            if (string.IsNullOrEmpty(account.MicrosoftAuthInfo.RefreshToken))
                throw new InvalidOperationException("Microsoft refresh token is missing.");

            // 刷新微软令牌
            account.MicrosoftAuthInfo = await _microsoftAuthHandler.GetTokenByRefreshTokenAsync(account.MicrosoftAuthInfo.RefreshToken, cancellationToken);

            // 使用刷新Xbox令牌
            account.XboxAuthInfo = await _xboxAuthHandler.GetXboxLiveTokenAsync(account.MicrosoftAuthInfo.AccessToken!, cancellationToken);

            // 刷新Mojang令牌
            account.MojangAuthInfo = await _mojangAuthHandler.RefreshTokenAsync(account.MojangAuthInfo!.RefreshToken!, cancellationToken);
        }
        else if (account.Type == AccountType.Xbox && account.XboxAuthInfo != null)
        {
            if (string.IsNullOrEmpty(account.XboxAuthInfo.AccessToken) || string.IsNullOrEmpty(account.XboxAuthInfo.UserHash))
                throw new InvalidOperationException("Xbox token or user hash is missing.");

            // 刷新Mojang令牌
            account.MojangAuthInfo = await _mojangAuthHandler.RefreshTokenAsync(account.MojangAuthInfo!.RefreshToken!, cancellationToken);
        }
        else if (account.Type == AccountType.Mojang && account.MojangAuthInfo != null)
        {
            if (string.IsNullOrEmpty(account.MojangAuthInfo.RefreshToken))
                throw new InvalidOperationException("Mojang refresh token is missing.");

            // 刷新Mojang令牌
            account.MojangAuthInfo = await _mojangAuthHandler.RefreshTokenAsync(account.MojangAuthInfo.RefreshToken, cancellationToken);
        }
        else
        {
            throw new InvalidOperationException("Unsupported account type or missing credentials.");
        }

        account.LastUpdatedAt = DateTimeOffset.UtcNow;
        return account;
    }

    /// <summary>
    /// 验证Minecraft账号令牌
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>令牌是否有效</returns>
    public async Task<bool> ValidateTokenAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            return false;

        return await _mojangAuthHandler.ValidateTokenAsync(account.MojangAuthInfo.AccessToken, cancellationToken);
    }

    /// <summary>
    /// 登出Minecraft账号
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否登出成功</returns>
    public async Task<bool> LogoutAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        var success = true;

        if (account.MojangAuthInfo != null && !string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
        {
            // 注销Mojang令牌
            success &= await _mojangAuthHandler.LogoutAsync(account.MojangAuthInfo.AccessToken, cancellationToken);
        }

        // 清除会话
        account.CurrentSession = null;
        account.LastUpdatedAt = DateTimeOffset.UtcNow;

        return success;
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

        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");

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
            ExpiresAt = account.MojangAuthInfo.ExpiresAt
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

        // 检查会话是否过期
        if (session.IsExpired)
            return Task.FromResult(false);

        // 对于官方认证，我们无法直接验证会话，只能检查是否过期
        return Task.FromResult(true);
    }
}
