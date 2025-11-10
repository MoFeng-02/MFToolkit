using System.Text.Json;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Helpers;
using MFToolkit.Minecraft.JsonExtensions;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Downloads.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MFToolkit.Minecraft.Services.Downloads;

public class MinecraftVersionService : IMinecraftVersionService
{
    private readonly DownloadOptions _downloadOptions;
    private readonly StorageOptions _storageOptions;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MinecraftVersionService> _logger;

    public MinecraftVersionService(IOptionsMonitor<DownloadOptions> downloadOptions, HttpClient httpClient,
        IOptionsMonitor<StorageOptions> storageOptions, ILogger<MinecraftVersionService> logger)
    {
        _httpClient = httpClient;
        _downloadOptions = downloadOptions.CurrentValue;
        _storageOptions = storageOptions.CurrentValue;
        _logger = logger;
    }

    public async Task<VersionManifest> GetVersionManifestAsync(
        string version = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json",
        bool isRefresh = false,
        CancellationToken cancellationToken = default)
    {
        if (!isRefresh && File.Exists(_storageOptions.GetVersionManifestPath()))
        {
            var t = await File.ReadAllTextAsync(_storageOptions.GetVersionManifestPath(), cancellationToken);
            try
            {
                var r = JsonSerializer.Deserialize(t, MinecraftJsonSerializerContext.Default.VersionManifest);
                if (r != null)
                {
                    return r;
                }
            }
            catch (Exception ex)
            {
                _logger.BeginScope("{ServiceName}::GetVersionManifestAsync 解析本地版本清单失败，准备重新下载。异常信息：{ExceptionMessage}",
                    nameof(MinecraftVersionService), ex.Message);
            }
            // 无法解析则继续下载
        }

        var mirrorUrl = _downloadOptions.ConvertToMirrorUrl(version);

        var response = await _httpClient.GetAsync(mirrorUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        // 确保目录存在
        Directory.CreateDirectory(_storageOptions.GetVersionManifestDirectory());
        await File.WriteAllTextAsync(_storageOptions.GetVersionManifestPath(), json, cancellationToken);
        return JsonSerializer.Deserialize(json, MinecraftJsonSerializerContext.Default.VersionManifest) ??
               throw new InvalidOperationException("无法解析版本清单");
    }

    public async Task<GameVersionDetail> GetGameVersionInfoAsync(string versionId, string? localPath = null,
        CancellationToken cancellationToken = default)
    {
        // 1. 首先尝试从本地文件读取
        if (!string.IsNullOrWhiteSpace(localPath) && File.Exists(localPath))
        {
            try
            {
                var localVersionInfo = await TryGetFromLocalFileAsync(versionId, localPath, cancellationToken);
                if (localVersionInfo != null)
                {
                    _logger.LogInformation($"从本地文件加载版本 {versionId} 信息成功");
                    return localVersionInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"从本地文件加载版本 {versionId} 信息失败，将尝试远程获取");
            }
        }

        // 2. 从远程获取版本信息
        return await GetFromRemoteAsync(versionId, localPath, cancellationToken);
    }

    /// <summary>
    /// 尝试从本地文件获取版本信息
    /// </summary>
    private async Task<GameVersionDetail?> TryGetFromLocalFileAsync(string versionId, string localPath,
        CancellationToken cancellationToken)
    {
        // 获取版本清单以验证本地文件
        var manifest = await GetVersionManifestAsync(cancellationToken: cancellationToken);
        var versionInfoInManifest = manifest.Versions.FirstOrDefault(v => v.Id == versionId);

        if (versionInfoInManifest == null)
        {
            _logger.LogWarning($"版本 {versionId} 不在版本清单中，无法验证本地文件");
            return null;
        }

        // 检查文件完整性（对于v2清单）
        if (versionInfoInManifest.Sha1 != null)
        {
            var isValid =
                await FileHashChecker.CheckFileSha1Async(localPath, versionInfoInManifest.Sha1, _logger,
                    cancellationToken);
            if (!isValid)
            {
                _logger.LogWarning($"版本 {versionId} 的本地文件SHA1校验失败，文件可能已损坏");
                return null;
            }

            _logger.LogDebug($"版本 {versionId} 的本地文件SHA1校验通过");
        }

        // 读取并解析本地文件
        try
        {
            var jsonContent = await File.ReadAllTextAsync(localPath, cancellationToken);
            var gameVersionInfo =
                JsonSerializer.Deserialize(jsonContent, MinecraftJsonSerializerContext.Default.GameVersionDetail);

            if (gameVersionInfo == null)
            {
                _logger.LogWarning($"版本 {versionId} 的本地文件内容为空或格式错误");
                return null;
            }

            // 验证必要字段
            if (string.IsNullOrEmpty(gameVersionInfo.Id))
            {
                _logger.LogWarning($"版本 {versionId} 的本地文件缺少必要字段 Id");
                return null;
            }

            // 确保版本ID匹配
            if (!string.Equals(gameVersionInfo.Id, versionId, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"本地文件版本ID不匹配: 期望 {versionId}, 实际 {gameVersionInfo.Id}");
                return null;
            }

            return gameVersionInfo;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, $"版本 {versionId} 的本地文件JSON解析失败");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"读取版本 {versionId} 的本地文件时发生错误");
            return null;
        }
    }

    /// <summary>
    /// 从远程获取版本信息
    /// </summary>
    private async Task<GameVersionDetail> GetFromRemoteAsync(string versionId, string? localPath,
        CancellationToken cancellationToken)
    {
        // 获取版本清单
        var manifest = await GetVersionManifestAsync(cancellationToken: cancellationToken);
        var versionInfo = manifest.Versions.FirstOrDefault(v => v.Id == versionId);

        if (versionInfo == null)
        {
            throw new KeyNotFoundException($"未找到版本 {versionId} 的信息");
        }

        _logger.LogInformation($"开始下载版本 {versionId} 的详细信息");

        try
        {
            // 下载版本JSON文件
            using var response = await _httpClient.GetAsync(versionInfo.Url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // 解析版本信息
            var gameVersionInfo =
                JsonSerializer.Deserialize(jsonContent, MinecraftJsonSerializerContext.Default.GameVersionDetail);
            if (gameVersionInfo == null)
            {
                throw new InvalidOperationException($"无法解析版本 {versionId} 的JSON数据");
            }

            // 验证必要字段
            if (string.IsNullOrEmpty(gameVersionInfo.Id))
            {
                throw new InvalidOperationException($"版本 {versionId} 的JSON数据缺少Id字段");
            }

            // 保存到本地文件（如果提供了路径）
            if (!string.IsNullOrWhiteSpace(localPath))
            {
                await SaveVersionInfoToLocalAsync(gameVersionInfo, localPath, cancellationToken);
            }

            _logger.LogInformation($"成功下载并解析版本 {versionId} 的信息");
            return gameVersionInfo;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"下载版本 {versionId} 信息失败: {ex.Message}");
            throw new InvalidOperationException($"下载版本 {versionId} 信息失败", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, $"解析版本 {versionId} 的JSON数据失败");
            throw new InvalidOperationException($"解析版本 {versionId} 的JSON数据失败", ex);
        }
    }

    /// <summary>
    /// 保存版本信息到本地文件
    /// </summary>
    private async Task SaveVersionInfoToLocalAsync(GameVersionDetail versionDetail, string localPath,
        CancellationToken cancellationToken)
    {
        try
        {
            // 确保目录存在
            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 序列化并保存
            var jsonContent =
                JsonSerializer.Serialize(versionDetail, MinecraftJsonSerializerContext.Default.GameVersionDetail);
            await File.WriteAllTextAsync(localPath, jsonContent, cancellationToken);

            _logger.LogDebug($"版本 {versionDetail.Id} 的信息已保存到本地: {localPath}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"保存版本 {versionDetail.Id} 信息到本地文件失败: {localPath}");
            // 不抛出异常，因为主要功能已经完成
        }
    }
}