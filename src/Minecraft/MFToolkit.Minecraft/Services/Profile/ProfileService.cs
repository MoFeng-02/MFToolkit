using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Constants;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Profile;

namespace MFToolkit.Minecraft.Services.Profile;

/// <summary>
/// Minecraft档案服务实现
/// </summary>
public class ProfileService : IProfileService
{
    private readonly HttpClient _httpClient;
    
    /// <summary>
    /// 初始化档案服务
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    public ProfileService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }
    
    /// <summary>
    /// 获取玩家档案
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>玩家档案</returns>
    public async Task<PlayerProfile> GetProfileAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
        
        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");
        
        using var request = new HttpRequestMessage(HttpMethod.Get, AuthEndpoints.MinecraftProfile);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.MojangAuthInfo.AccessToken);
        
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var profile = await response.Content.ReadFromJsonAsync<PlayerProfile>(cancellationToken);
        if (profile == null)
            throw new InvalidOperationException("Failed to parse player profile response.");
        
        // 更新账号信息
        if (!string.IsNullOrEmpty(profile.Name))
            account.PlayerName = profile.Name;
        
        if (!string.IsNullOrEmpty(profile.Id) && Guid.TryParse(profile.Id, out var uuid))
            account.PlayerUuid = uuid;
        
        account.LastUpdatedAt = DateTimeOffset.UtcNow;
        
        return profile;
    }
    
    /// <summary>
    /// 通过UUID获取玩家档案
    /// </summary>
    /// <param name="uuid">玩家UUID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>玩家档案</returns>
    public async Task<PlayerProfile> GetProfileByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        var url = $"https://sessionserver.mojang.com/session/minecraft/profile/{uuid.ToString("N")}";
        
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var profile = await response.Content.ReadFromJsonAsync<PlayerProfile>(cancellationToken);
        if (profile == null)
            throw new InvalidOperationException("Failed to parse player profile response.");
        
        return profile;
    }
    
    /// <summary>
    /// 通过名称获取玩家档案
    /// </summary>
    /// <param name="name">玩家名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>玩家档案</returns>
    public async Task<PlayerProfile> GetProfileByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Player name cannot be null or empty.", nameof(name));
        
        var url = $"https://api.mojang.com/users/profiles/minecraft/{name}";
        
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var profile = await response.Content.ReadFromJsonAsync<PlayerProfile>(cancellationToken);
        if (profile == null)
            throw new InvalidOperationException("Failed to parse player profile response.");
        
        return profile;
    }
    
    /// <summary>
    /// 更改玩家名称
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="newName">新玩家名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新后的玩家档案</returns>
    public async Task<PlayerProfile> ChangePlayerNameAsync(MinecraftAccount account, string newName, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
        
        if (string.IsNullOrEmpty(newName))
            throw new ArgumentException("New player name cannot be null or empty.", nameof(newName));
        
        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");
        
        // 检查名称是否可用
        var isAvailable = await CheckNameAvailabilityAsync(newName, cancellationToken);
        if (!isAvailable)
            throw new InvalidOperationException($"Player name '{newName}' is not available.");
        
        var url = string.Format(AuthEndpoints.MinecraftNameChange, newName);
        
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.MojangAuthInfo.AccessToken);
        
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        // 获取更新后的档案
        return await GetProfileAsync(account, cancellationToken);
    }
    
    /// <summary>
    /// 检查名称是否可用
    /// </summary>
    /// <param name="name">玩家名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>名称是否可用</returns>
    public async Task<bool> CheckNameAvailabilityAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Player name cannot be null or empty.", nameof(name));
        
        var url = string.Format(AuthEndpoints.MinecraftNameAvailability, name);
        
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        
        // 如果名称可用，返回204 No Content
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return true;
        
        // 如果名称不可用，返回404 Not Found
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;
        
        // 其他状态码视为错误
        response.EnsureSuccessStatusCode();
        return false;
    }
}
