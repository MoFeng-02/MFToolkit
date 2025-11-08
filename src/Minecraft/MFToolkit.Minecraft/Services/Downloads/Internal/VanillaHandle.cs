using System.Text.Json;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Helpers;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Downloads.Models;
using Microsoft.Extensions.Logging;
using MFToolkit.Minecraft.JsonExtensions;

namespace MFToolkit.Minecraft.Services.Downloads.Internal;
/// <summary>
/// 原版 Minecraft 文件处理类
/// </summary>
internal class VanillaHandle
{
    private readonly HttpClient _httpClient;
    private readonly StorageOptions _storageOptions;
    private readonly DownloadOptions _downloadOptions;
    private readonly ILogger _logger;

    public VanillaHandle(HttpClient httpClient, StorageOptions storageOptions, DownloadOptions downloadOptions, ILogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
        _downloadOptions = downloadOptions ?? throw new ArgumentNullException(nameof(downloadOptions));
        _logger = logger;
    }

    /// <summary>
    /// 获取原版版本的所有下载任务
    /// </summary>
    /// <param name="versionInfo">版本详情Json序列化表</param>
    /// <param name="os">系统</param>
    /// <param name="saveFileName">保存的文件名称（保存文件夹，版本文件，版本文件json）</param>
    /// <param name="features"></param>
    /// <returns></returns>
    public async Task<List<MinecraftDownloadTask>> GetDownloadTasksAsync(GameVersionInfo versionInfo, string os, string? saveFileName = null, Dictionary<string, bool>? features = null)
    {
        var tasks = new List<MinecraftDownloadTask>();

        // 1. 添加版本客户端文件
        if (versionInfo.Downloads?.Client != null)
        {
            _logger.LogInformation($"添加版本客户端文件下载任务: {versionInfo.Id}");
            tasks.Add(CreateVersionClientTask(versionInfo, saveFileName));
        }

        // 2. 添加资源索引文件
        if (versionInfo.AssetIndex != null)
        {
            _logger.LogInformation($"添加资源索引文件下载任务: {versionInfo.AssetIndex.Id}");
            tasks.Add(CreateAssetIndexTask(versionInfo));
        }

        // 3. 添加库文件（包括原生库）
        _logger.LogInformation($"添加库文件下载任务: {versionInfo.Libraries.Count} 个库");
        tasks.AddRange(await CreateLibraryTasksAsync(versionInfo, os, features));

        // 4. 添加日志配置文件
        if (versionInfo.Logging?.Client?.File != null)
        {
            _logger.LogInformation($"添加日志配置文件下载任务");
            tasks.Add(CreateLogConfigTask(versionInfo));
        }

        return tasks;
    }

    /// <summary>
    /// 验证文件完整性
    /// </summary>
    public async Task<bool> ValidateFileAsync(MinecraftDownloadTask task, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(task.SavePath))
            return false;

        try
        {
            // 检查文件大小
            var fileInfo = new FileInfo(task.SavePath);
            if (fileInfo.Length != task.Size)
                return false;

            // 使用 FileHashChecker 检查 SHA1
            if (!string.IsNullOrEmpty(task.Sha1))
            {
                return await FileHashChecker.CheckFileSha1Async(task.SavePath, task.Sha1, _logger, cancellationToken);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 创建版本客户端文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateVersionClientTask(GameVersionInfo versionInfo, string? saveFileName)
    {
        var clientDownload = versionInfo.Downloads!.Client!;
        var savePath = _storageOptions.GetVersionJarPath(saveFileName ?? versionInfo.Id);

        return new MinecraftDownloadTask
        {
            VersionId = versionInfo.Id,
            Size = clientDownload.Size,
            Sha1 = clientDownload.Sha1 ?? string.Empty,
            OriginUrl = clientDownload.Url,
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(clientDownload.Url),
            Name = $"{saveFileName ?? versionInfo.Id}.jar",
            MinecraftFileType = MinecraftFileType.Version,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = savePath,
            Priority = DownloadPriority.High, // 版本文件优先级高
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now
        };
    }

    /// <summary>
    /// 创建资源索引文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateAssetIndexTask(GameVersionInfo versionInfo)
    {
        var assetIndex = versionInfo.AssetIndex!;
        var savePath = _storageOptions.GetAssetIndexPath(assetIndex.Id);

        return new MinecraftDownloadTask
        {
            VersionId = versionInfo.Id,
            Size = assetIndex.Size,
            Sha1 = assetIndex.Sha1,
            OriginUrl = assetIndex.Url,
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(assetIndex.Url),
            Name = $"{assetIndex.Id}.json",
            MinecraftFileType = MinecraftFileType.AssetIndex,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = savePath,
            Priority = DownloadPriority.Normal,
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now
        };
    }

    /// <summary>
    /// 创建库文件下载任务（包括普通库和原生库）
    /// </summary>
    private async Task<List<MinecraftDownloadTask>> CreateLibraryTasksAsync(GameVersionInfo versionInfo, string os, Dictionary<string, bool>? features)
    {
        var tasks = new List<MinecraftDownloadTask>();

        foreach (var library in versionInfo.Libraries)
        {
            if (!library.IsApplicable(os, features))
                continue;

            // 添加主要工件
            if (library.Downloads?.Artifact != null)
            {
                tasks.Add(CreateLibraryTask(versionInfo, library, library.Downloads.Artifact, isNative: false));
            }

            // 添加原生库
            string? nativeClassifier = library.GetNativeClassifier(os);
            if (!string.IsNullOrEmpty(nativeClassifier) &&
                library.Downloads?.Classifiers != null &&
                library.Downloads.Classifiers.TryGetValue(nativeClassifier, out var nativeDownload))
            {
                tasks.Add(CreateLibraryTask(versionInfo, library, nativeDownload, isNative: true));
            }
        }

        return await Task.FromResult(tasks);
    }

    /// <summary>
    /// 创建单个库文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateLibraryTask(GameVersionInfo versionInfo, Library library, Download download, bool isNative)
    {
        var libraryPath = library.GetPath();
        var savePath = _storageOptions.GetLibraryPath(libraryPath);

        return new MinecraftDownloadTask
        {
            VersionId = versionInfo.Id,
            Size = download.Size,
            Sha1 = download.Sha1 ?? string.Empty,
            OriginUrl = download.Url,
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(download.Url),
            Name = Path.GetFileName(libraryPath),
            MinecraftFileType = isNative ? MinecraftFileType.Natives : MinecraftFileType.Libraries,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = savePath,
            Priority = DownloadPriority.Normal,
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now,
            LocalFilePath = savePath // 保存实际文件路径
        };
    }

    /// <summary>
    /// 创建日志配置文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateLogConfigTask(GameVersionInfo versionInfo)
    {
        var logFile = versionInfo.Logging!.Client!.File;
        var savePath = Path.Combine(_storageOptions.GetLogsPath(versionInfo.Id), "configs", $"{logFile.Id}.xml");

        return new MinecraftDownloadTask
        {
            VersionId = versionInfo.Id,
            Size = logFile.Size,
            Sha1 = logFile.Sha1,
            OriginUrl = logFile.Url,
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(logFile.Url),
            Name = $"{logFile.Id}.xml",
            MinecraftFileType = MinecraftFileType.Config,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = savePath,
            Priority = DownloadPriority.Low, // 日志配置优先级低
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now
        };
    }

    /// <summary>
    /// 获取资源文件下载任务（需要先下载并解析资源索引）
    /// </summary>
    public async Task<List<MinecraftDownloadTask>> GetAssetDownloadTasksAsync(GameVersionInfo versionInfo)
    {
        var tasks = new List<MinecraftDownloadTask>();

        // 1. 读取资源索引文件
        var assetIndexPath = _storageOptions.GetAssetIndexPath(versionInfo.AssetIndex!.Id);
        if (!File.Exists(assetIndexPath))
        {
            throw new FileNotFoundException($"资源索引文件不存在: {assetIndexPath}");
        }

        var assetIndexContent = await File.ReadAllTextAsync(assetIndexPath);
        var assetIndex = JsonSerializer.Deserialize(assetIndexContent, MinecraftJsonSerializerContext.Default.AssetIndexContent);

        if (assetIndex?.Objects == null)
        {
            return tasks;
        }

        // 2. 为每个资源对象创建下载任务
        foreach (var (assetName, assetObject) in assetIndex.Objects)
        {
            if (string.IsNullOrEmpty(assetObject.Hash))
                continue;

            tasks.Add(CreateAssetTask(versionInfo, assetName, assetObject));
        }

        return tasks;
    }

    /// <summary>
    /// 创建单个资源文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateAssetTask(GameVersionInfo versionInfo, string assetName, AssetObject assetObject)
    {
        // 构建资源下载URL（官方格式）
        var hash = assetObject.Hash;
        var url = $"https://resources.download.minecraft.net/{hash[..2]}/{hash}";
        var savePath = _storageOptions.GetAssetObjectPath(hash);

        return new MinecraftDownloadTask
        {
            VersionId = versionInfo.Id,
            Size = assetObject.Size,
            Sha1 = hash,
            OriginUrl = url,
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(url),
            Name = assetName,
            MinecraftFileType = MinecraftFileType.Assets,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = savePath,
            Priority = DownloadPriority.Normal,
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now
        };
    }

    /// <summary>
    /// 验证文件完整性
    /// </summary>
    public async Task<bool> ValidateFileAsync(MinecraftDownloadTask task)
    {
        return await FileHashChecker.CheckFileSha1Async(task.SavePath, task.Sha1, _logger);
    }

    /// <summary>
    /// 下载并解析资源索引文件，需要调用方特殊处理的方法
    /// </summary>
    public async Task<AssetIndexContent?> DownloadAndParseAssetIndexAsync(GameVersionInfo versionInfo)
    {
        if (versionInfo.AssetIndex == null)
            return null;
        var assetIndexTask = CreateAssetIndexTask(versionInfo);
        var assetIndexPath = assetIndexTask.SavePath;
        // 先判断是否存在并且有效
        var fileValid = await ValidateFileAsync(assetIndexTask);
        if (fileValid)
        {
            try
            {
                // 有效的情况下直接尝试读取并解析
                var existingContent = await File.ReadAllTextAsync(assetIndexPath);
                var assetContent = JsonSerializer.Deserialize(existingContent, MinecraftJsonSerializerContext.Default.AssetIndexContent);

                return assetContent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"解析已有资源索引文件失败，准备重新下载: {ex.Message}");
            }
        }
        // 确保目录存在
        Directory.CreateDirectory(Path.GetDirectoryName(assetIndexPath)!);

        // 下载资源索引文件
        using var response = await _httpClient.GetAsync(assetIndexTask.DownloadUrl);
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();

        // 保存文件
        await File.WriteAllTextAsync(assetIndexPath, content);

        // 解析并返回
        return JsonSerializer.Deserialize(content, MinecraftJsonSerializerContext.Default.AssetIndexContent);
    }
}