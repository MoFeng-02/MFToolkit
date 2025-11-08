using System.Net.Http.Json;
using MFToolkit.Minecraft.Constants;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Account.Http;
using MFToolkit.Minecraft.JsonExtensions;

namespace MFToolkit.Minecraft.Services.Auth.Internal;

/// <summary>
/// Xbox认证处理器
/// </summary>
internal class XboxAuthHandler
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 初始化Xbox认证处理器
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    public XboxAuthHandler(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// 使用微软访问令牌获取Xbox Live令牌
    /// </summary>
    /// <param name="microsoftAccessToken">微软访问令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Xbox认证信息</returns>
    public async Task<XboxAuthInfo> GetXboxLiveTokenAsync(string microsoftAccessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(microsoftAccessToken))
            throw new ArgumentException("Microsoft access token cannot be null or empty.", nameof(microsoftAccessToken));

        var requestBody = new XboxAuthRequest
        {
            Properties = new()
            {
                AuthMethod = "RPS",
                SiteName = "user.auth.xboxlive.com",
                RpsTicket = $"d={microsoftAccessToken}"
            },
            RelyingParty = "http://auth.xboxlive.com",
            TokenType = "JWT"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.XboxLiveAuthorize)
        {
            Content = JsonContent.Create(requestBody, MinecraftJsonSerializerContext.Default.XboxAuthRequest)
        };

        request.Headers.Add("x-xbl-contract-version", "1");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync(MinecraftJsonSerializerContext.Default.XboxLiveTokenResponse, cancellationToken);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            throw new InvalidOperationException("Failed to get Xbox Live token.");

        // 解析令牌过期时间

        return new XboxAuthInfo
        {
            UserHash = tokenResponse.DisplayClaims?.Xui?[0]?.Uhs,
            AccessToken = tokenResponse.Token,
            TokenType = tokenResponse.TokenType,
            DisplayClaims = new XboxDisplayClaims
            {
                UserInfo = tokenResponse.DisplayClaims?.Xui?.Select(x => new XboxUserInfo
                {
                    XboxId = x.Xid,
                    GamerTag = x.Gtg,
                    GamerPictureUrl = x.Pgt
                }).ToArray()
            },
            TokenIssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = tokenResponse.NotAfter
        };
    }

    /// <summary>
    /// 使用Xbox Live令牌获取Xbox服务令牌
    /// </summary>
    /// <param name="xboxLiveToken">Xbox Live令牌</param>
    /// <param name="userHash">Xbox用户哈希</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Xbox服务认证信息</returns>
    public async Task<XboxAuthInfo> GetXboxServiceTokenAsync(string xboxLiveToken, string userHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(xboxLiveToken))
            throw new ArgumentException("Xbox Live token cannot be null or empty.", nameof(xboxLiveToken));

        if (string.IsNullOrEmpty(userHash))
            throw new ArgumentException("User hash cannot be null or empty.", nameof(userHash));

        var requestBody = new XboxXstsAuthRequest
        {
            Properties = new()
            {
                SandboxId = "RETAIL",
                UserTokens = [xboxLiveToken]
            },
            RelyingParty = "rp://api.minecraftservices.com/",
            TokenType = "JWT"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.XboxAuthorize)
        {
            Content = JsonContent.Create(requestBody, MinecraftJsonSerializerContext.Default.XboxXstsAuthRequest)
        };

        request.Headers.Add("x-xbl-contract-version", "1");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync(MinecraftJsonSerializerContext.Default.XboxServiceTokenResponse, cancellationToken);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            throw new InvalidOperationException("Failed to get Xbox service token.");


        return new XboxAuthInfo
        {
            UserHash = userHash,
            AccessToken = tokenResponse.Token,
            TokenType = tokenResponse.TokenType,
            DisplayClaims = new XboxDisplayClaims
            {
                UserInfo = tokenResponse.DisplayClaims?.Xui?.Select(x => new XboxUserInfo
                {
                    XboxId = x.Xid,
                    GamerTag = x.Gtg,
                    GamerPictureUrl = x.Pgt
                }).ToArray()
            },
            TokenIssuedAt = DateTimeOffset.UtcNow,
            ExpiresAt = tokenResponse.NotAfter
        };
    }

}
