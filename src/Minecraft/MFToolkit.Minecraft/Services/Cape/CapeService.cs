using MFToolkit.Minecraft.Constants;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Cape;
using SkiaSharp;

namespace MFToolkit.Minecraft.Services.Cape;

/// <summary>
/// Minecraft披风服务实现
/// </summary>
public class CapeService : ICapeService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 初始化披风服务
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    public CapeService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// 获取玩家披风
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>披风信息</returns>
    public async Task<CapeInfo> GetCapeAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");

        var profileService = new Profile.ProfileService(_httpClient);
        var profile = await profileService.GetProfileAsync(account, cancellationToken);

        return profile.CurrentCape ?? new CapeInfo();
    }

    /// <summary>
    /// 通过UUID获取玩家披风
    /// </summary>
    /// <param name="uuid">玩家UUID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>披风风信息</returns>
    public async Task<CapeInfo> GetCapeByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        var profileService = new Profile.ProfileService(_httpClient);
        var profile = await profileService.GetProfileByUuidAsync(uuid, cancellationToken);

        return profile.CurrentCape ?? new CapeInfo();
    }

    /// <summary>
    /// 上传披风
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="capeStream">披风图片流</param>
    /// <param name="contentType">图片内容类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传后的披风信息</returns>
    public async Task<CapeInfo> UploadCapeAsync(MinecraftAccount account, Stream capeStream, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        ArgumentNullException.ThrowIfNull(capeStream);

        if (string.IsNullOrEmpty(contentType))
            throw new ArgumentException("Content type cannot be null or empty.", nameof(contentType));

        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");

        // 验证披风文件
        if (!await ValidateCapeFileAsync(capeStream, contentType, cancellationToken))
            throw new InvalidOperationException("Invalid cape file.");

        // 重置流位置
        capeStream.Position = 0;

        // 创建请求内容
        using var content = new MultipartFormDataContent
        {
            { new StreamContent(capeStream), "file", "cape.png" }
        };

        // 创建请求
        using var request = new HttpRequestMessage(HttpMethod.Post, AuthEndpoints.MinecraftCapeUpload)
        {
            Content = content
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", account.MojangAuthInfo.AccessToken);
        request.Headers.Add("Content-Type", contentType);

        // 发送请求
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        // 获取更新后的披风信息
        return await GetCapeAsync(account, cancellationToken);
    }

    /// <summary>
    /// 通过URL上传披风
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="capeUrl">披风图片URL</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上传后的披风信息</returns>
    public async Task<CapeInfo> UploadCapeFromUrlAsync(MinecraftAccount account, string capeUrl, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (string.IsNullOrEmpty(capeUrl))
            throw new ArgumentException("Cape URL cannot be null or empty.", nameof(capeUrl));

        if (!Uri.TryCreate(capeUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid cape URL.", nameof(capeUrl));

        // 下载披风图片
        using var response = await _httpClient.GetAsync(capeUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (string.IsNullOrEmpty(contentType))
            throw new InvalidOperationException("Could not determine content type of the cape image.");

        using var capeStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // 上传披风
        return await UploadCapeAsync(account, capeStream, contentType, cancellationToken);
    }

    /// <summary>
    /// 重置披为默认值
    /// </summary>
    /// <param name="account">Minecraft账号</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否重置成功</returns>
    public Task<bool> ResetCapeAsync(MinecraftAccount account, CancellationToken cancellationToken = default)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (account.MojangAuthInfo == null || string.IsNullOrEmpty(account.MojangAuthInfo.AccessToken))
            throw new InvalidOperationException("Mojang access token is missing.");

        // 目前Mojang API不支持直接重置披风为默认值
        // 这里只是一个占位实现，实际需要根据官方API更新
        throw new NotImplementedException("Resetting cape to default is not yet implemented.");
    }

    /// <summary>
    /// 验证披风文件
    /// </summary>
    /// <param name="capeStream">披风图片流</param>
    /// <param name="contentType">图片内容类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证结果</returns>
    public async Task<bool> ValidateCapeFileAsync(Stream capeStream, string contentType, CancellationToken cancellationToken = default)
    {
        if (capeStream == null)
            throw new ArgumentNullException(nameof(capeStream));

        if (string.IsNullOrEmpty(contentType))
            throw new ArgumentException("Content type cannot be null or empty.", nameof(contentType));

        // 检查内容类型
        if (!contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) &&
            !contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 检查文件大小（最大2MB）
        if (capeStream.Length > 2 * 1024 * 1024)
            return false;

        // 重置流位置
        var originalPosition = capeStream.Position;

        try
        {
            // 检查图片尺寸（披风通常为64x32或128x64）
            var image = SKBitmap.DecodeBounds(capeStream);

            if ((image.Width != 64 || image.Height != 32) &&
                (image.Width != 128 || image.Height != 64))
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
            capeStream.Position = originalPosition;
        }
    }
}
