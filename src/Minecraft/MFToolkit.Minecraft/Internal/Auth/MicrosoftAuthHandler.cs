using System.Net.Http.Json;
using MFToolkit.Minecraft.Constants;
using MFToolkit.Minecraft.Core;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Account.Http;
using MFToolkit.Minecraft.Exceptions.Auths;
using MFToolkit.Minecraft.Options;
using Microsoft.Extensions.Options;

namespace MFToolkit.Minecraft.Internal.Auth;

/// <summary>
/// 微软认证处理器
/// </summary>
internal class MicrosoftAuthHandler
{
    private readonly HttpClient _httpClient;
    private readonly OfficialAuthOptions _options;

    /// <summary>
    /// 初始化微软认证处理器
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="options">官方认证配置</param>
    public MicrosoftAuthHandler(HttpClient httpClient, IOptions<OfficialAuthOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrEmpty(_options.ClientId))
            throw new InvalidOperationException("ClientId is not configured for Microsoft authentication.");

        //if (string.IsNullOrEmpty(_options.RedirectUri))
        //    throw new InvalidOperationException("RedirectUri is not configured for Microsoft authentication.");
    }

    /// <summary>
    /// 获取微软授权URL
    /// </summary>
    /// <param name="state">状态参数</param>
    /// <param name="scope">权限范围</param>
    /// <returns>微软授权URL</returns>
    public string GetAuthorizationUrl(string state, string[]? scope = null)
    {
        if (string.IsNullOrEmpty(state))
            throw new ArgumentException("State cannot be null or empty.", nameof(state));

        var scopes = scope ?? OAuthScopes.MicrosoftScopes;
        var scopeString = string.Join(" ", scopes);

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId! },
            { "response_type", "code" },
            //{ "redirect_uri", _options.RedirectUri },
            { "scope", scopeString },
            { "state", state },
            { "prompt", "select_account" }
        };
        if(!string.IsNullOrEmpty(_options.RedirectUri))
        {
            parameters.Add("redirect_uri", _options.RedirectUri!);
        }

        var queryString = string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{AuthEndpoints.MicrosoftAuthorization}?{queryString}";
    }

    /// <summary>
    /// 使用授权码获取微软令牌
    /// </summary>
    /// <param name="code">授权码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>微软认证信息</returns>
    public async Task<MicrosoftAuthInfo> GetTokenByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code))
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId! },
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", _options.RedirectUri! }
        };

        if (!string.IsNullOrEmpty(_options.ClientSecret))
        {
            parameters.Add("client_secret", _options.ClientSecret);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MicrosoftToken)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync(MinecraftJsonSerializerContext.Default.MicrosoftTokenResponse, cancellationToken);
        if (tokenResponse == null)
            throw new InvalidOperationException("Failed to parse Microsoft token response.");

        return new MicrosoftAuthInfo
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = tokenResponse.TokenType,
            ExpiresIn = tokenResponse.ExpiresIn,
            IssuedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// 使用刷新令牌获取微软令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>微软认证信息</returns>
    public async Task<MicrosoftAuthInfo> GetTokenByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
            throw new ArgumentException("Refresh token cannot be null or empty.", nameof(refreshToken));

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId! },
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" },
            { "redirect_uri", _options.RedirectUri! }
        };

        if (!string.IsNullOrEmpty(_options.ClientSecret))
        {
            parameters.Add("client_secret", _options.ClientSecret);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MicrosoftToken)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync(MinecraftJsonSerializerContext.Default.MicrosoftTokenResponse, cancellationToken);
        if (tokenResponse == null)
            throw new InvalidOperationException("Failed to parse Microsoft token response.");

        return new MicrosoftAuthInfo
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = tokenResponse.TokenType,
            ExpiresIn = tokenResponse.ExpiresIn,
            IssuedAt = DateTimeOffset.UtcNow
        };
    }
    /// <summary>
    /// 获取微软设备代码
    /// </summary>
    /// <param name="scope">权限范围</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>设备代码结果</returns>
    public async Task<MicrosoftDeviceCodeResult> GetDeviceCodeAsync(string[]? scope = null, CancellationToken cancellationToken = default)
    {
        var scopes = scope ?? OAuthScopes.MicrosoftScopes;
        var scopeString = string.Join(" ", scopes);

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId! },
            { "scope", scopeString }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MicrosoftDeviceCode)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var deviceCodeResponse = await response.Content.ReadFromJsonAsync(MinecraftJsonSerializerContext.Default.MicrosoftDeviceCodeResponse,cancellationToken);
        if (deviceCodeResponse == null)
            throw new InvalidOperationException("Failed to parse Microsoft device code response.");

        return new MicrosoftDeviceCodeResult
        {
            DeviceCode = deviceCodeResponse.DeviceCode,
            UserCode = deviceCodeResponse.UserCode,
            VerificationUri = deviceCodeResponse.VerificationUri,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(deviceCodeResponse.ExpiresIn),
            Interval = deviceCodeResponse.Interval,
            Message = deviceCodeResponse.Message
        };
    }

    /// <summary>
    /// 使用设备代码获取微软令牌
    /// </summary>
    /// <param name="deviceCode">设备代码</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>微软认证信息</returns>
    public async Task<MicrosoftAuthInfo> GetTokenByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(deviceCode))
            throw new ArgumentException("Device code cannot be null or empty.", nameof(deviceCode));

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId! },
            { "device_code", deviceCode },
            { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MicrosoftToken)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        // 如果响应不是400 Bad请求，尝试解析错误信息
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync(MinecraftJsonSerializerContext.Default.MicrosoftTokenErrorResponse, cancellationToken);
            if (errorResponse != null)
            {
                // 根据错误类型抛出相应的异常
                switch (errorResponse.Error)
                {
                    case "authorization_pending":
                        throw new AuthorizationPendingException("Authorization is pending. Please complete the authentication on the device.");
                    case "authorization_declined":
                        throw new AuthorizationDeclinedException("Authorization has been declined by the user.");
                    case "bad_verification_code":
                        throw new BadVerificationCodeException("The verification code is invalid.");
                    case "expired_token":
                        throw new ExpiredTokenException("The device code has expired. Please request a new one.");
                    default:
                        throw new HttpRequestException($"Microsoft token request failed with error: {errorResponse.Error}. Description: {errorResponse.ErrorDescription}");
                }
            }

            // 如果无法解析错误信息，直接抛出异常
            response.EnsureSuccessStatusCode();
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>(cancellationToken);
        if (tokenResponse == null)
            throw new InvalidOperationException("Failed to parse Microsoft token response.");

        return new MicrosoftAuthInfo
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = tokenResponse.TokenType,
            ExpiresIn = tokenResponse.ExpiresIn,
            IssuedAt = DateTimeOffset.UtcNow
        };
    }

}
