using System.Net.Http.Json;
using MFToolkit.Minecraft.Constants;
using MFToolkit.Minecraft.Core;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Account.Http;

namespace MFToolkit.Minecraft.Internal.Auth;

/// <summary>
/// Mojang认证处理器
/// </summary>
internal class MojangAuthHandler
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 初始化Mojang认证处理器
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    public MojangAuthHandler(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// 使用Xbox令牌登录Minecraft
    /// </summary>
    /// <param name="xboxToken">Xbox令牌</param>
    /// <param name="userHash">Xbox用户哈希</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Mojang认证信息</returns>
    public async Task<MojangAuthInfo> LoginWithXboxAsync(string xboxToken, string userHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(xboxToken))
            throw new ArgumentException("Xbox token cannot be null or empty.", nameof(xboxToken));

        if (string.IsNullOrEmpty(userHash))
            throw new ArgumentException("User hash cannot be null or empty.", nameof(userHash));

        var requestBody = new MojangAuthRequest
        {
            IdentityToken = $"XBL3.0 x={userHash};{xboxToken}"
        };

        //using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MinecraftLogin)
        //{
        //    Content = JsonContent.Create(requestBody, MinecraftJsonSerializerContext.Default.MojangAuthRequest)
        //};

        //using var response = await _httpClient.SendAsync(request, cancellationToken);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MinecraftLauncher/2.2.1468"); // 使用官方 User-Agent
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        using var response = await _httpClient.PostAsJsonAsync(AuthEndpoints.MinecraftLogin, requestBody, MinecraftJsonSerializerContext.Default.MojangAuthRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync(MinecraftJsonSerializerContext.Default.MojangTokenResponse, cancellationToken);
        if (tokenResponse == null)
            throw new InvalidOperationException("Failed to parse Mojang token response.");

        var result = new MojangAuthInfo
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = tokenResponse.TokenType,
            ExpiresIn = tokenResponse.ExpiresIn,
            TokenIssuedAt = DateTimeOffset.UtcNow
        };
        return result;
    }

    /// <summary>
    /// 刷新Mojang令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新后的Mojang认证信息</returns>
    public async Task<MojangAuthInfo> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("Refresh token cannot be null or empty.", nameof(refreshToken));

        var requestBody = new MojangAuthRefreshRequest
        {
            RefreshToken = refreshToken
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MinecraftRefresh)
        {
            Content = JsonContent.Create(requestBody, MinecraftJsonSerializerContext.Default.MojangAuthRefreshRequest)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync(MinecraftJsonSerializerContext.Default.MojangTokenResponse, cancellationToken);
        if (tokenResponse == null)
            throw new InvalidOperationException("Failed to parse Mojang token response.");

        return new MojangAuthInfo
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = tokenResponse.TokenType,
            ExpiresIn = tokenResponse.ExpiresIn,
            TokenIssuedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// 验证Mojang令牌
    /// </summary>
    /// <param name="accessToken">访问令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>令牌是否有效</returns>
    public async Task<bool> ValidateTokenAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

        using var request = new HttpRequestMessage(HttpMethod.Get, AuthEndpoints.MinecraftValidate);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 注销Mojang令牌
    /// </summary>
    /// <param name="accessToken">访问令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否注销成功</returns>
    public async Task<bool> LogoutAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

        var requestBody = new MojangLogingOut
        {
            AccessToken = accessToken
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MinecraftLogout)
        {
            Content = JsonContent.Create(requestBody, MinecraftJsonSerializerContext.Default.MojangLogingOut)
        };

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证是否拥有游戏
    /// </summary>
    /// <param name="accessToken">Minecraft Token <see cref="MinecraftAuthToken.AccessToken"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> IsHaveMinecraftAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentNullException(nameof(accessToken));
        //_httpClient.DefaultRequestHeaders.Authorization = ("Bearer", accessToken);
        _httpClient.DefaultRequestHeaders.Authorization = new ("Bearer", accessToken);
        using var response = await _httpClient.GetAsync(AuthEndpoints.MinecraftHaveGame, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return !string.IsNullOrWhiteSpace(content) && content != "{}";
    }
}
