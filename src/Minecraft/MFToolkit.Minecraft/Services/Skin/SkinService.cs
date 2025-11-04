using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using MFToolkit.Minecraft.Constants;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Skin;
using MFToolkit.Minecraft.Options;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace MFToolkit.Minecraft.Services.Skin;

/// <summary>
/// Minecraft皮肤服务实现
/// </summary>
public class SkinService : ISkinService
{
    private readonly HttpClient _httpClient;
    private readonly SkinOptions _options;

    /// <summary>
    /// 初始化皮肤服务
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="options">皮肤配置</param>
    public SkinService(HttpClient httpClient, IOptions<SkinOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 获取玩家玩家皮肤
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>皮肤信息</returns>
    public async Task<SkinInfo> GetSkinAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");

        var profileService = new Profile.ProfileService(_httpClient);
        var profile = await profileService.GetProfileAsync(account, cancellationToken);

        return profile.CurrentSkin ?? new SkinInfo();
    }

    /// <summary>
    /// 通过UUID获取玩家皮肤
    /// </summary>
    /// <param name="uuid">玩家UUID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>皮肤信息</returns>
    public async Task<SkinInfo> GetSkinByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        var profileService = new Profile.ProfileService(_httpClient);
        var profile = await profileService.GetProfileByUuidAsync(uuid, cancellationToken);

        return profile.CurrentSkin ?? new SkinInfo();
    }

    /// <summary>
    /// 上传皮肤
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="skinStream">皮肤图片流</param>
    /// <param name="contentType">图片内容类型</param>
    /// <param name="isSlim">是否为纤细模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传后的皮肤信息</returns>
    public async Task<SkinInfo> UploadSkinAsync(MinecraftAccount account, Stream skinStream, string contentType, bool isSlim = false, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (skinStream == null)
            throw new ArgumentNullException(nameof(skinStream));

        if (string.IsNullOrEmpty(contentType))
            throw new ArgumentException("Content type cannot be null or empty.", nameof(contentType));

        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");

        // 验证皮肤文件
        if (!await ValidateSkinFileAsync(skinStream, contentType, cancellationToken))
            throw new InvalidOperationException("Invalid skin file.");

        // 重置流位置
        skinStream.Position = 0;

        // 创建请求内容
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(skinStream), "file", "skin.png");

        // 创建请求
        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MinecraftSkinUpload)
        {
            Content = content
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.MojangAuthInfo.AccessToken);
        request.Headers.Add("Content-Type", contentType);

        // 添加皮肤模型参数
        var model = isSlim ? "slim" : "classic";
        request.Headers.Add("Model", model);

        // 发送请求
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        // 获取更新后的皮肤信息
        return await GetSkinAsync(account, cancellationToken);
    }

    /// <summary>
    /// 通过URL上传皮肤
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="skinUrl">皮肤图片URL</param>
    /// <param name="isSlim">是否为纤细模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传后的后的皮肤信息</returns>
    public async Task<SkinInfo> UploadSkinFromUrlAsync(MinecraftAccount account, string skinUrl, bool isSlim = false, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (string.IsNullOrEmpty(skinUrl))
            throw new ArgumentException("Skin URL cannot be null or empty.", nameof(skinUrl));

        if (!Uri.TryCreate(skinUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid skin URL.", nameof(skinUrl));

        // 下载皮肤图片
        using var response = await _httpClient.GetAsync(skinUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrEmpty(contentType))
            throw new InvalidOperationException("Could not determine content type of the skin image.");

        using var skinStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // 上传皮肤
        return await UploadSkinAsync(account, skinStream, contentType, isSlim, cancellationToken);
    }

    /// <summary>
    /// 重置皮肤为默认值
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否重置成功</returns>
    public Task<bool> ResetSkinAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");

        // 目前Mojang API不支持直接重置皮肤为默认值
        // 这里只是一个占位实现，实际需要根据官方API更新
        throw new NotImplementedException("Resetting skin to default is not yet implemented.");
    }

    /// <summary>
    /// 验证皮肤文件
    /// </summary>
    /// <param name="skinStream">皮肤图片流</param>
    /// <param name="contentType">图片内容类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证结果</returns>
    public async Task<bool> ValidateSkinFileAsync(Stream skinStream, string contentType, CancellationToken cancellationToken = default)
    {
        if (skinStream == null)
            throw new ArgumentNullException(nameof(skinStream));

        if (string.IsNullOrEmpty(contentType))
            throw new ArgumentException("Content type cannot be null or empty.", nameof(contentType));

        // 检查内容类型
        if (!Array.Exists(_options.AllowedContentTypes, t => t.Equals(contentType, StringComparison.OrdinalIgnoreCase)))
            return false;

        // 检查文件大小
        if (skinStream.Length > _options.MaxSizeBytes)
            return false;

        // 重置流位置
        var originalPosition = skinStream.Position;

        try
        {
            // 检查图片尺寸
            var image = SKBitmap.DecodeBounds(skinStream);

            if (!Array.Exists(_options.AllowedWidths, w => w == image.Width) ||
                !Array.Exists(_options.AllowedHeights, h => h == image.Height))
            {
                return false;
            }

            return await Task.FromResult(true);
        }
        catch
        {
            return false;
        }
        finally
        {
            // 恢复流位置
            skinStream.Position = originalPosition;
        }
    }
}
