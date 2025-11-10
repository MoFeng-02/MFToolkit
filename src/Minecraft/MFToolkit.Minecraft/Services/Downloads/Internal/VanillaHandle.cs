// VanillaHandle.cs (修复版本)
using System.Text.Json;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Helpers;
using MFToolkit.Minecraft.Options;
using Microsoft.Extensions.Logging;
using MFToolkit.Minecraft.JsonExtensions;
using MFToolkit.Minecraft.Entities.Downloads;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Extensions.VersionExtensions;

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

    public VanillaHandle(HttpClient httpClient, StorageOptions storageOptions, DownloadOptions downloadOptions,
        ILogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
        _downloadOptions = downloadOptions ?? throw new ArgumentNullException(nameof(downloadOptions));
        _logger = logger;
    }

    /// <summary>
    /// 下载版本Json文件
    /// </summary>
    public MinecraftDownloadTask GetGameVersionInfoJson(VersionInfo versionInfo, string? versionName = null)
    {
        var versionPath = _storageOptions.GetVersionJsonPath(versionName ?? versionInfo.Id);
        return new MinecraftDownloadTask()
        {
            VersionId = versionInfo.Id,
            Size = -1,
            Sha1 = versionInfo.Sha1,
            OriginUrl = versionInfo.Url.ToString(),
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(versionInfo.Url.ToString()),
            Name = $"{versionName ?? versionInfo.Id}.json",
            MinecraftFileType = MinecraftFileType.Version,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = versionPath,
            Priority = DownloadPriority.Urgent,
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now
        };
    }

    /// <summary>
    /// 从文件读取版本详情
    /// </summary>
    public async Task<GameVersionDetail> GetGameVersionInfoAsync(string savePath)
    {
        if (!File.Exists(savePath)) 
            throw new FileNotFoundException("Game version file not found.", savePath);
        
        var json = await File.ReadAllTextAsync(savePath);
        
        // 使用新的序列化上下文
        return JsonSerializer.Deserialize(json, MinecraftJsonSerializerContext.Default.GameVersionDetail) 
            ?? throw new JsonException("Failed to parse version JSON");
    }

    /// <summary>
    /// 获取原版版本的所有下载任务
    /// </summary>
    public async Task<List<MinecraftDownloadTask>> GetDownloadTasksAsync(GameVersionDetail versionDetail, string os,
        string? saveFileName = null, Dictionary<string, bool>? features = null)
    {
        var tasks = new List<MinecraftDownloadTask>();

        // 1. 添加版本客户端文件
        if (versionDetail.Downloads?.Client != null)
        {
            _logger.LogInformation("添加版本客户端文件下载任务: {VersionId}", versionDetail.Id);
            tasks.Add(CreateVersionClientTask(versionDetail, saveFileName));
        }

        // 2. 添加资源索引文件
        if (versionDetail.AssetIndex != null)
        {
            _logger.LogInformation("添加资源索引文件下载任务: {AssetIndexId}", versionDetail.AssetIndex.Id);
            tasks.Add(CreateAssetIndexTask(versionDetail));
        }

        // 3. 添加库文件（包括原生库）
        _logger.LogInformation("添加库文件下载任务: {LibraryCount} 个库", versionDetail.Libraries.Count);
        tasks.AddRange(await CreateLibraryTasksAsync(versionDetail, os, features));

        // 4. 添加日志配置文件
        if (versionDetail.Logging?.Client?.File != null)
        {
            _logger.LogInformation("添加日志配置文件下载任务");
            tasks.Add(CreateLogConfigTask(versionDetail));
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
            // 检查文件大小（如果已知）
            if (task.Size > 0)
            {
                var fileInfo = new FileInfo(task.SavePath);
                if (fileInfo.Length != task.Size)
                    return false;
            }

            // 检查 SHA1 哈希
            if (!string.IsNullOrEmpty(task.Sha1))
            {
                return await FileHashChecker.CheckFileSha1Async(task.SavePath, task.Sha1, _logger, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "验证文件完整性时发生错误: {FilePath}", task.SavePath);
            return false;
        }
    }

    /// <summary>
    /// 创建版本客户端文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateVersionClientTask(GameVersionDetail versionDetail, string? saveFileName)
    {
        var clientDownload = versionDetail.Downloads!.Client!;
        var savePath = _storageOptions.GetVersionJarPath(saveFileName ?? versionDetail.Id);

        return new MinecraftDownloadTask
        {
            VersionId = versionDetail.Id,
            Size = clientDownload.Size,
            Sha1 = clientDownload.Sha1,
            OriginUrl = clientDownload.Url,
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(clientDownload.Url),
            Name = $"{saveFileName ?? versionDetail.Id}.jar",
            MinecraftFileType = MinecraftFileType.Version,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = savePath,
            Priority = DownloadPriority.High,
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now
        };
    }

    /// <summary>
    /// 创建资源索引文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateAssetIndexTask(GameVersionDetail versionDetail)
    {
        var assetIndex = versionDetail.AssetIndex!;
        var savePath = _storageOptions.GetAssetIndexPath(assetIndex.Id);

        return new MinecraftDownloadTask
        {
            VersionId = versionDetail.Id,
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
    private async Task<List<MinecraftDownloadTask>> CreateLibraryTasksAsync(GameVersionDetail versionDetail, string os,
        Dictionary<string, bool>? features)
    {
        var tasks = new List<MinecraftDownloadTask>();

        foreach (var library in versionDetail.Libraries)
        {
            if (!library.IsApplicable(os, features))
                continue;

            // 添加主要工件
            if (library.Downloads?.Artifact != null)
            {
                tasks.Add(CreateLibraryTask(versionDetail, library, library.Downloads.Artifact, isNative: false));
            }

            // 添加原生库
            var nativeClassifier = library.GetNativeClassifier(os);
            if (!string.IsNullOrEmpty(nativeClassifier) &&
                library.Downloads?.Classifiers != null &&
                library.Downloads.Classifiers.TryGetValue(nativeClassifier, out var nativeDownload))
            {
                tasks.Add(CreateLibraryTask(versionDetail, library, nativeDownload, isNative: true));
            }
        }

        return await Task.FromResult(tasks);
    }

    /// <summary>
    /// 创建单个库文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateLibraryTask(GameVersionDetail versionDetail, Library library, DownloadItem download,
        bool isNative)
    {
        // 使用库名称构建路径（兼容旧版本格式）
        var libraryPath = GetLibraryPath(library);
        var savePath = _storageOptions.GetLibraryPath(libraryPath);

        return new MinecraftDownloadTask
        {
            VersionId = versionDetail.Id,
            Size = download.Size,
            Sha1 = download.Sha1,
            OriginUrl = download.Url,
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(download.Url),
            Name = Path.GetFileName(libraryPath),
            MinecraftFileType = isNative ? MinecraftFileType.Natives : MinecraftFileType.Libraries,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = savePath,
            Priority = DownloadPriority.Normal,
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now
        };
    }

    /// <summary>
    /// 从库名称构建文件路径
    /// </summary>
    private string GetLibraryPath(Library library)
    {
        // 如果有下载信息中的路径，优先使用
        if (!string.IsNullOrEmpty(library.Downloads?.Artifact?.Path))
            return library.Downloads.Artifact.Path;

        // 否则从库名称解析
        var parts = library.Name.Split(':');
        if (parts.Length < 3)
            return $"{library.Name}.jar";

        var groupId = parts[0].Replace('.', '/');
        var artifactId = parts[1];
        var version = parts[2];

        // 处理分类器（第四部分）
        string? classifier = null;
        string extension = "jar";

        if (parts.Length >= 4 && !string.IsNullOrEmpty(parts[3]))
        {
            // 检查是否包含扩展名
            if (parts[3].Contains('@'))
            {
                var classifierParts = parts[3].Split('@');
                classifier = classifierParts[0];
                extension = classifierParts.Length > 1 ? classifierParts[1] : extension;
            }
            else
            {
                classifier = parts[3];
            }
        }

        // 处理显式扩展名（第五部分）
        if (parts.Length >= 5 && !string.IsNullOrEmpty(parts[4]))
        {
            extension = parts[4];
        }

        // 构建标准 Maven 路径
        var fileName = $"{artifactId}-{version}";
        if (!string.IsNullOrEmpty(classifier))
        {
            fileName += $"-{classifier}";
        }
        fileName += $".{extension}";

        return $"{groupId}/{artifactId}/{version}/{fileName}";
    }

    /// <summary>
    /// 创建日志配置文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateLogConfigTask(GameVersionDetail versionDetail)
    {
        var logFile = versionDetail.Logging!.Client!.File;
        var savePath = Path.Combine(_storageOptions.GetLogsPath(versionDetail.Id), "configs", $"{logFile.Id}");

        return new MinecraftDownloadTask
        {
            VersionId = versionDetail.Id,
            Size = logFile.Size,
            Sha1 = logFile.Sha1,
            OriginUrl = logFile.Url,
            DownloadUrl = _downloadOptions.ConvertToMirrorUrl(logFile.Url),
            Name = $"{logFile.Id}",
            MinecraftFileType = MinecraftFileType.Config,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = savePath,
            Priority = DownloadPriority.Low,
            RetryCount = 0,
            MaxRetries = _downloadOptions.Settings.MaxRetryCount,
            CreatedTime = DateTime.Now
        };
    }

    /// <summary>
    /// 获取资源文件下载任务（需要先下载并解析资源索引）
    /// </summary>
    public async Task<List<MinecraftDownloadTask>> GetAssetDownloadTasksAsync(GameVersionDetail versionDetail)
    {
        var tasks = new List<MinecraftDownloadTask>();

        if (versionDetail.AssetIndex == null)
            return tasks;

        // 1. 读取资源索引文件
        var assetIndexPath = _storageOptions.GetAssetIndexPath(versionDetail.AssetIndex.Id);
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

            tasks.Add(CreateAssetTask(versionDetail, assetName, assetObject));
        }

        return tasks;
    }

    /// <summary>
    /// 创建单个资源文件下载任务
    /// </summary>
    private MinecraftDownloadTask CreateAssetTask(GameVersionDetail versionDetail, string assetName, AssetObject assetObject)
    {
        var hash = assetObject.Hash;
        var url = $"https://resources.download.minecraft.net/{hash[..2]}/{hash}";
        var savePath = _storageOptions.GetAssetObjectPath(hash);

        return new MinecraftDownloadTask
        {
            VersionId = versionDetail.Id,
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
    /// 下载并解析资源索引文件
    /// </summary>
    public async Task<AssetIndexContent?> DownloadAndParseAssetIndexAsync(GameVersionDetail versionDetail)
    {
        if (versionDetail.AssetIndex == null)
            return null;

        var assetIndexTask = CreateAssetIndexTask(versionDetail);
        var assetIndexPath = assetIndexTask.SavePath;

        // 检查现有文件是否有效
        var fileValid = await ValidateFileAsync(assetIndexTask);
        if (fileValid)
        {
            try
            {
                var existingContent = await File.ReadAllTextAsync(assetIndexPath);
                var assetContent = JsonSerializer.Deserialize(existingContent,
                    MinecraftJsonSerializerContext.Default.AssetIndexContent);

                if (assetContent != null)
                    return assetContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析已有资源索引文件失败，准备重新下载");
            }
        }

        // 下载新的资源索引文件
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(assetIndexPath)!);

            using var response = await _httpClient.GetAsync(assetIndexTask.DownloadUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("下载资源索引文件失败: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(assetIndexPath, content);

            return JsonSerializer.Deserialize(content, MinecraftJsonSerializerContext.Default.AssetIndexContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载资源索引文件时发生错误");
            return null;
        }
    }

    /// <summary>
    /// 获取启动参数（兼容新旧版本格式）
    /// </summary>
    public IEnumerable<string> GetLaunchArguments(GameVersionDetail versionDetail, string os, 
        Dictionary<string, bool>? features = null, Dictionary<string, string>? replacements = null)
    {
        // 使用扩展方法
        return versionDetail.GetGameArguments(os, features, replacements);
    }

    /// <summary>
    /// 获取JVM参数
    /// </summary>
    public IEnumerable<string> GetJvmArguments(GameVersionDetail versionDetail, string os,
        Dictionary<string, bool>? features = null, Dictionary<string, string>? replacements = null)
    {
        // 使用扩展方法
        return versionDetail.GetJvmArguments(os, features, replacements);
    }
}