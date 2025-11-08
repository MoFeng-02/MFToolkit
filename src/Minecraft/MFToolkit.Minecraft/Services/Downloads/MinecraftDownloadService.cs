using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Downloads.Interfaces;
using MFToolkit.Minecraft.Services.Downloads.Internal;
using MFToolkit.Minecraft.Services.Downloads.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MFToolkit.Minecraft.Services.Downloads;

/// <summary>
/// Minecraft下载服务实现
/// <para>
/// 实现方式，通过ModLoaderType来进行区分，ModLoaderType为空时，默认为Vanilla下载服务，再在启动下载中根据ModLoaderType的值进行区分，通过内部Internal 的 各个Handle来进行处理每个搭配的下载逻辑
/// </para>
/// <para>
/// 如<remarks>VanillaHandle</remarks>进行原版下载路径提供
/// </para>
/// </summary>
public class MinecraftDownloadService : IMinecraftDownloadService
{
    private SemaphoreSlim _downloadSemaphore;
    private readonly HttpClient _httpClient;
    private readonly IMinecraftVersionService _minecraftVersionService;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _versionCancellationTokens;
    private readonly ConcurrentDictionary<Guid, MinecraftDownloadProgress> _activeDownloads;
    private readonly ConcurrentDictionary<string, Task> _versionDownloadTasks;
    private readonly ILogger<MinecraftDownloadService> _logger;
    public ModLoaderType ModLoaderType { get; set; } = ModLoaderType.None;
    public event Action<MinecraftDownloadProgress>? ProgressChanged;
    public event Action<MinecraftDownloadCompletedResult>? DownloadCompleted;
    public event Action<MinecraftDownloadError>? DownloadError;

    public event Action<Action>? ExecuteOnUiThread;

    #region Handlers

    /// <summary>
    /// 原版处理器
    /// </summary>
    private readonly VanillaHandle _vanillaHandle;

    #endregion
    public ObservableCollection<MinecraftDownloadProgress> DownloadQueue { get; } = new();
    public IOptionsMonitor<StorageOptions> StorageOptions { get; set; }
    public IOptionsMonitor<DownloadOptions> DownloadOptions { get; set; }

    public MinecraftDownloadService(IOptionsMonitor<StorageOptions> storageOptions, IOptionsMonitor<DownloadOptions> downloadOptions, HttpClient httpClient, ILogger<MinecraftDownloadService> logger, IMinecraftVersionService minecraftVersionService)
    {
        StorageOptions = storageOptions;
        DownloadOptions = downloadOptions;
        _httpClient = httpClient;
        _logger = logger;

        _downloadSemaphore = new SemaphoreSlim(downloadOptions.CurrentValue.Settings.MaxDownloadThreads);
        _versionCancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
        _activeDownloads = new ConcurrentDictionary<Guid, MinecraftDownloadProgress>();
        _versionDownloadTasks = new ConcurrentDictionary<string, Task>();
        _vanillaHandle = new VanillaHandle(_httpClient, storageOptions.CurrentValue, downloadOptions.CurrentValue, logger);
        // 监听配置变化
        downloadOptions.OnChange(OnDownloadOptionsChanged);
        _minecraftVersionService = minecraftVersionService;
    }

    /// <summary>
    /// 处理下载配置变化
    /// </summary>
    private void OnDownloadOptionsChanged(DownloadOptions newOptions, string? name)
    {
        // 更新 HttpClient 超时
        _httpClient.Timeout = TimeSpan.FromSeconds(newOptions.Settings.DownloadTimeoutSeconds);

        // 更新信号量以匹配新的最大线程数
        var oldSemaphore = _downloadSemaphore;
        _downloadSemaphore = new SemaphoreSlim(newOptions.Settings.MaxDownloadThreads);
        oldSemaphore?.Dispose();
    }

    #region 下载控制方法

    public async Task<bool> StartDownloadAsync(string versionId, string? saveFileName = null, StorageOptions? storageOptions = null, DownloadPriority priority = DownloadPriority.Normal, CancellationToken cancellationToken = default)
    {
        try
        {
            // 检查是否已经在下载
            if (_versionDownloadTasks.ContainsKey(versionId))
            {
                _logger.LogWarning($"版本 {versionId} 已经在下载中");
                return false;
            }

            var actualStorageOptions = storageOptions ?? StorageOptions.CurrentValue;

            // 1. 获取版本信息
            var versionInfo = await GetVersionInfoAsync(versionId, cancellationToken);
            if (versionInfo == null)
            {
                _logger.LogError($"无法获取版本 {versionId} 的信息");
                return false;
            }

            // 2. 创建版本专用的取消令牌
            var versionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (!_versionCancellationTokens.TryAdd(versionId, versionCts))
            {
                versionCts.Dispose();
                return false;
            }

            // 3. 创建下载任务并启动
            var downloadTask = StartVersionDownloadAsync(versionId, versionInfo, actualStorageOptions, priority, saveFileName, versionCts.Token);

            // 4. 存储任务并设置续接处理
            _versionDownloadTasks[versionId] = downloadTask.ContinueWith(a =>
            {
                _versionDownloadTasks.TryRemove(versionId, out _);
                _versionCancellationTokens.TryRemove(versionId, out _);
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"启动版本 {versionId} 下载失败");
            OnDownloadError(new MinecraftDownloadError
            {
                VersionId = versionId,
                ErrorMessage = $"启动下载失败: {ex.Message}",
                Ex = ex
            });
            return false;
        }
    }

    /// <summary>
    /// 启动版本下载流程
    /// </summary>
    private async Task StartVersionDownloadAsync(string versionId, GameVersionInfo versionInfo, StorageOptions storageOptions, DownloadPriority priority, string? saveFileName = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"开始下载版本 {versionId}");
            #region 原版阶段
            // 阶段1: 下载基础文件（版本文件、库文件等）
            var baseTasks = await GetBaseDownloadTasksAsync(versionId, versionInfo, priority, saveFileName, cancellationToken);
            if (baseTasks.Count == 0)
            {
                _logger.LogWarning($"版本 {versionId} 没有需要下载的基础文件");
                return;
            }
            //var allTasks = baseTasks.Concat(assetTasks).ToList();
            await ExecuteParallelDownloadAsync(versionId, baseTasks, cancellationToken);

            // 阶段2 基础文件下载完成后，开始下载资源文件
            var assetIndexContent = await ProcessAssetIndexAsync(versionId, versionInfo, cancellationToken);
            if (assetIndexContent == null)
            {
                _logger.LogError($"版本 {versionId} 资源索引处理失败");
                return;
            }
            var assetTasks = await GetAssetDownloadTasksAsync(versionId, versionInfo, assetIndexContent, priority, cancellationToken);
            // 执行第二次下载，下载资源文件
            await ExecuteParallelDownloadAsync(versionId, assetTasks, cancellationToken);
            #endregion
            #region 根据 ModLoaderType 进行扩展下载阶段

            #endregion
            _logger.LogInformation($"版本 {versionId} 下载完成");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"版本 {versionId} 下载被取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"版本 {versionId} 下载过程中发生错误");
            OnDownloadError(new MinecraftDownloadError
            {
                VersionId = versionId,
                ErrorMessage = $"下载过程错误: {ex.Message}",
                Ex = ex
            });
        }
    }

    /// <summary>
    /// 获取基础下载任务（版本文件、库文件等）
    /// </summary>
    private async Task<List<MinecraftDownloadProgress>> GetBaseDownloadTasksAsync(string versionId, GameVersionInfo versionInfo, DownloadPriority priority, string? saveFileName = null, CancellationToken cancellationToken = default)
    {
        var tasks = await _vanillaHandle.GetDownloadTasksAsync(versionInfo, GetCurrentOs(), saveFileName, null);

        var progressList = tasks.Select(task => new MinecraftDownloadProgress
        {
            Id = task.Id,
            VersionId = versionId,
            Size = task.Size,
            Sha1 = task.Sha1,
            OriginUrl = task.OriginUrl,
            DownloadUrl = task.DownloadUrl,
            Name = task.Name,
            MinecraftFileType = task.MinecraftFileType,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = task.SavePath,
            Priority = priority,
            RetryCount = 0,
            MaxRetries = task.MaxRetries,
            CreatedTime = DateTime.Now,
            LocalFilePath = task.LocalFilePath,
            DownloadBytes = 0,
            Progress = 0,
            ProgressPercentage = "0%"
        }).ToList();

        // 添加到观察集合
        await ExecuteOnUiThreadAsync(() =>
        {
            foreach (var progress in progressList)
            {
                DownloadQueue.Add(progress);
            }
        });

        return progressList;
    }

    /// <summary>
    /// 处理资源索引文件
    /// </summary>
    private async Task<AssetIndexContent?> ProcessAssetIndexAsync(string versionId, GameVersionInfo versionInfo, CancellationToken cancellationToken)
    {
        if (versionInfo.AssetIndex == null)
        {
            _logger.LogInformation($"版本 {versionId} 没有资源索引");
            return null;
        }

        // 检查是否已存在有效的资源索引
        return await _vanillaHandle.DownloadAndParseAssetIndexAsync(versionInfo);

    }

    /// <summary>
    /// 获取资源文件下载任务
    /// </summary>
    private async Task<List<MinecraftDownloadProgress>> GetAssetDownloadTasksAsync(string versionId, GameVersionInfo versionInfo, AssetIndexContent assetIndexContent, DownloadPriority priority, CancellationToken cancellationToken)
    {
        if (assetIndexContent?.Objects == null || assetIndexContent.Objects.Count == 0)
        {
            _logger.LogInformation($"版本 {versionId} 没有资源文件需要下载");
            return new List<MinecraftDownloadProgress>();
        }

        _logger.LogInformation($"开始生成 {assetIndexContent.Objects.Count} 个资源文件下载任务");

        var assetTasks = await _vanillaHandle.GetAssetDownloadTasksAsync(versionInfo);

        var progressList = assetTasks.Select(task => new MinecraftDownloadProgress
        {
            Id = task.Id,
            VersionId = versionId,
            Size = task.Size,
            Sha1 = task.Sha1,
            OriginUrl = task.OriginUrl,
            DownloadUrl = task.DownloadUrl,
            Name = task.Name,
            MinecraftFileType = task.MinecraftFileType,
            DownloadStatus = DownloadStatus.Pending,
            SavePath = task.SavePath,
            Priority = priority,
            RetryCount = 0,
            MaxRetries = task.MaxRetries,
            CreatedTime = DateTime.Now,
            LocalFilePath = task.LocalFilePath,
            DownloadBytes = 0,
            Progress = 0,
            ProgressPercentage = "0%"
        }).ToList();

        // 添加到观察集合
        await ExecuteOnUiThreadAsync(() =>
        {
            foreach (var progress in progressList)
            {
                DownloadQueue.Add(progress);
            }
        });

        _logger.LogInformation($"已生成 {progressList.Count} 个资源文件下载任务");
        return progressList;
    }

    /// <summary>
    /// 执行并行下载
    /// </summary>
    private async Task ExecuteParallelDownloadAsync(string versionId, List<MinecraftDownloadProgress> tasks, CancellationToken cancellationToken)
    {
        var successTasks = new ConcurrentBag<MinecraftDownloadTask>();
        var errorTasks = new ConcurrentBag<MinecraftDownloadError>();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = DownloadOptions.CurrentValue.Settings.MaxDownloadThreads
        };

        try
        {
            await Parallel.ForEachAsync(tasks, parallelOptions, async (progress, ct) =>
            {
                try
                {
                    // ✅ 检查文件是否已经验证通过
                    if (progress.DownloadStatus == DownloadStatus.Completed || progress.IsVerified)
                    {
                        _logger.LogDebug($"文件 {progress.Name} 已经下载完成，跳过");
                        successTasks.Add(progress);
                        return;
                    }
                    await _downloadSemaphore.WaitAsync(ct);
                    _activeDownloads[progress.Id] = progress;

                    var result = await DownloadFileAsync(progress, ct);
                    if (result)
                    {
                        successTasks.Add(progress);
                        _logger.LogDebug($"文件 {progress.Name} 下载完成");
                    }
                    else
                    {
                        errorTasks.Add(new MinecraftDownloadError
                        {
                            Id = progress.Id,
                            VersionId = progress.VersionId,
                            Size = progress.Size,
                            Sha1 = progress.Sha1,
                            OriginUrl = progress.OriginUrl,
                            DownloadUrl = progress.DownloadUrl,
                            Name = progress.Name,
                            MinecraftFileType = progress.MinecraftFileType,
                            DownloadStatus = DownloadStatus.Failed,
                            SavePath = progress.SavePath,
                            ErrorMessage = "下载失败",
                            Ex = null
                        });
                        _logger.LogWarning($"文件 {progress.Name} 下载失败");
                    }
                }
                catch (OperationCanceledException)
                {
                    progress.DownloadStatus = DownloadStatus.Cancelled;
                    await UpdateProgressAsync(progress);
                    _logger.LogInformation($"文件 {progress.Name} 下载被取消");
                }
                catch (Exception ex)
                {
                    progress.DownloadStatus = DownloadStatus.Failed;
                    await UpdateProgressAsync(progress);

                    errorTasks.Add(new MinecraftDownloadError
                    {
                        Id = progress.Id,
                        VersionId = progress.VersionId,
                        Size = progress.Size,
                        Sha1 = progress.Sha1,
                        OriginUrl = progress.OriginUrl,
                        DownloadUrl = progress.DownloadUrl,
                        Name = progress.Name,
                        MinecraftFileType = progress.MinecraftFileType,
                        DownloadStatus = DownloadStatus.Failed,
                        SavePath = progress.SavePath,
                        ErrorMessage = $"下载异常: {ex.Message}",
                        Ex = ex
                    });
                    _logger.LogError(ex, $"文件 {progress.Name} 下载异常");
                }
                finally
                {
                    _activeDownloads.TryRemove(progress.Id, out _);
                    _downloadSemaphore.Release();
                }
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"版本 {versionId} 并行下载被取消");
        }

        // 触发完成事件
        var completedResult = new MinecraftDownloadCompletedResult
        {
            TotalBytes = tasks.Sum(t => t.Size),
            DownloadedBytes = successTasks.Sum(t => t.Size),
            SuccessTasks = successTasks.ToList(),
            ErrorTasks = errorTasks.ToList()
        };

        OnDownloadCompleted(completedResult);
    }
    /// <summary>
    /// 下载单个文件
    /// </summary>
    private async Task<bool> DownloadFileAsync(MinecraftDownloadProgress progress, CancellationToken cancellationToken)
    {
        for (int retry = 0; retry <= progress.MaxRetries; retry++)
        {
            try
            {
                progress.DownloadStatus = DownloadStatus.Downloading;
                progress.RetryCount = retry;
                await UpdateProgressAsync(progress);

                // 确保目录存在
                var directory = Path.GetDirectoryName(progress.SavePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 检查是否支持断点续传
                long existingBytes = 0;
                if (DownloadOptions.CurrentValue.Settings.EnableResume && File.Exists(progress.SavePath))
                {
                    var fileInfo = new FileInfo(progress.SavePath);
                    existingBytes = fileInfo.Length;

                    if (existingBytes == progress.Size)
                    {
                        // 文件已完整下载
                        progress.DownloadBytes = progress.Size;
                        progress.Progress = 100;
                        progress.ProgressPercentage = "100%";
                        progress.DownloadStatus = DownloadStatus.Completed;
                        await UpdateProgressAsync(progress);
                        return true;
                    }
                }

                // 创建 HTTP 请求
                using var request = new HttpRequestMessage(HttpMethod.Get, progress.DownloadUrl);
                if (existingBytes > 0)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingBytes, null);
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                // 处理下载流
                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(progress.SavePath, existingBytes > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write);

                var totalBytes = existingBytes + (response.Content.Headers.ContentLength ?? 0);
                var buffer = new byte[81920];
                int bytesRead;
                var totalRead = existingBytes;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalRead += bytesRead;

                    // 更新进度
                    progress.DownloadBytes = totalRead;
                    progress.Progress = (double)totalRead / progress.Size * 100;
                    progress.ProgressPercentage = $"{progress.Progress:F1}%";
                    await UpdateProgressAsync(progress);

                    cancellationToken.ThrowIfCancellationRequested();
                }

                // 下载完成，验证文件
                progress.DownloadStatus = DownloadStatus.Verifying;
                await UpdateProgressAsync(progress);

                var isValid = await _vanillaHandle.ValidateFileAsync(progress);

                if (isValid)
                {
                    progress.DownloadStatus = DownloadStatus.Completed;
                    progress.Progress = 100;
                    progress.ProgressPercentage = "100%";
                    await UpdateProgressAsync(progress);
                    return true;
                }
                else
                {
                    // 文件验证失败，删除损坏的文件
                    if (File.Exists(progress.SavePath))
                    {
                        File.Delete(progress.SavePath);
                    }
                    throw new Exception("文件验证失败");
                }
            }
            catch (Exception ex) when (retry < progress.MaxRetries)
            {
                // 重试前等待
                await Task.Delay(DownloadOptions.CurrentValue.Settings.RetryDelayMs, cancellationToken);
                _logger.LogWarning($"下载{progress.Name}失败，正在重试 {retry + 1}/{progress.MaxRetries}：{ex.Message}");
                continue;
            }
            catch
            {
                // 最后一次尝试也失败
                progress.DownloadStatus = DownloadStatus.Failed;
                await UpdateProgressAsync(progress);
                return false;
            }
        }

        return false;
    }

    public async Task<bool> PauseDownloadAsync()
    {
        // 暂停所有版本的下载
        foreach (var cts in _versionCancellationTokens.Values)
        {
            cts.Cancel();
        }

        // 更新所有活动下载的状态
        foreach (var progress in _activeDownloads.Values)
        {
            if (progress.DownloadStatus == DownloadStatus.Downloading)
            {
                progress.DownloadStatus = DownloadStatus.Pause;
                await UpdateProgressAsync(progress);
            }
        }

        return await Task.FromResult(true);
    }

    public async Task<bool> PauseDownloadAsync(string versionId)
    {
        if (_versionCancellationTokens.TryGetValue(versionId, out var cts))
        {
            await cts.CancelAsync();

            // 更新该版本的所有下载状态
            foreach (var progress in DownloadQueue.Where(p => p.VersionId == versionId && p.DownloadStatus == DownloadStatus.Downloading))
            {
                progress.DownloadStatus = DownloadStatus.Pause;
                await UpdateProgressAsync(progress);
            }

            return true;
        }
        return false;
    }

    public async Task<bool> CancelDownloadAsync()
    {
        // 取消所有下载
        await PauseDownloadAsync();

        // 删除已下载的文件
        foreach (var progress in DownloadQueue.Where(p => p.DownloadStatus == DownloadStatus.Completed || p.DownloadStatus == DownloadStatus.Pause))
        {
            try
            {
                if (File.Exists(progress.SavePath))
                {
                    File.Delete(progress.SavePath);
                }
            }
            catch
            {
                // 忽略删除失败
            }

            progress.DownloadStatus = DownloadStatus.Cancelled;
            await UpdateProgressAsync(progress);
        }

        return true;
    }

    public async Task<bool> CancelDownloadAsync(string versionId)
    {
        // 取消指定版本下载
        await PauseDownloadAsync(versionId);

        // 删除该版本已下载的文件
        foreach (var progress in DownloadQueue.Where(p => p.VersionId == versionId &&
            (p.DownloadStatus == DownloadStatus.Completed || p.DownloadStatus == DownloadStatus.Pause)))
        {
            try
            {
                if (File.Exists(progress.SavePath))
                {
                    File.Delete(progress.SavePath);
                }
            }
            catch
            {
                // 忽略删除失败
            }

            progress.DownloadStatus = DownloadStatus.Cancelled;
            await UpdateProgressAsync(progress);
        }

        return true;
    }

    public async Task<int> StartBatchDownloadAsync(IEnumerable<string> versionIds, IEnumerable<string>? saveFileNames = null, StorageOptions? storageOptions = null, DownloadPriority priority = DownloadPriority.Normal, CancellationToken cancellationToken = default)
    {
        var successCount = 0;
        var downloadTasks = new List<Task<bool>>();
        var versionList = versionIds.ToList();
        var saveNameList = saveFileNames?.ToList() ?? [];

        for (int i = 0; i < versionList.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var versionId = versionList[i];
            var saveFileName = i < saveNameList.Count ? saveNameList[i] : null;

            downloadTasks.Add(StartDownloadAsync(versionId, saveFileName, storageOptions, priority, cancellationToken));
        }

        var results = await Task.WhenAll(downloadTasks);
        successCount = results.Count(r => r);
        return successCount;
    }

    #endregion

    #region 验证和检查方法

    public async Task<MinecraftValidationFileResult> ValidateGameFilesAsync(string versionId, StorageOptions storageOptions, CancellationToken cancellationToken = default)
    {
        var result = new MinecraftValidationFileResult();

        // 获取版本信息
        var versionInfo = await GetVersionInfoAsync(versionId, cancellationToken);
        if (versionInfo == null)
            return result;

        // 获取所有应该存在的文件任务
        var expectedTasks = await _vanillaHandle.GetDownloadTasksAsync(versionInfo, GetCurrentOs(), null);

        foreach (var task in expectedTasks)
        {
            var isValid = await _vanillaHandle.ValidateFileAsync(task);
            if (isValid)
            {
                result.ValidBytes += task.Size;
                result.SuccessTasks.Add(task);
            }
            else
            {
                result.ErrorTasks.Add(new MinecraftDownloadError
                {
                    Id = task.Id,
                    VersionId = task.VersionId,
                    Size = task.Size,
                    Sha1 = task.Sha1,
                    OriginUrl = task.OriginUrl,
                    DownloadUrl = task.DownloadUrl,
                    Name = task.Name,
                    MinecraftFileType = task.MinecraftFileType,
                    DownloadStatus = DownloadStatus.Failed,
                    SavePath = task.SavePath,
                    ErrorMessage = "文件验证失败",
                    Ex = null
                });
            }
            result.TotalBytes += task.Size;
        }

        result.IsValid = result.ErrorTasks.Count == 0;
        return result;
    }

    public async Task<bool> IsVersionInstalledAsync(string versionId, StorageOptions storageOptions, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateGameFilesAsync(versionId, storageOptions, cancellationToken);
        return validationResult.IsValid;
    }

    public MinecraftDownloadProgress GetDownloadProgress()
    {
        var totalBytes = DownloadQueue.Sum(q => q.Size);
        var downloadedBytes = DownloadQueue.Sum(q => q.DownloadBytes);
        var completedCount = DownloadQueue.Count(q => q.DownloadStatus == DownloadStatus.Completed);
        var totalCount = DownloadQueue.Count;

        return new MinecraftDownloadProgress
        {
            Name = "总体进度",
            Size = totalBytes,
            DownloadBytes = downloadedBytes,
            Progress = totalBytes > 0 ? (double)downloadedBytes / totalBytes * 100 : 0,
            ProgressPercentage = totalCount > 0 ? $"{completedCount}/{totalCount}" : "0/0",
            DownloadStatus = GetOverallStatus()
        };
    }

    public MinecraftDownloadProgress GetDownloadProgress(string versionId)
    {
        var versionTasks = DownloadQueue.Where(q => q.VersionId == versionId).ToList();
        var totalBytes = versionTasks.Sum(q => q.Size);
        var downloadedBytes = versionTasks.Sum(q => q.DownloadBytes);
        var completedCount = versionTasks.Count(q => q.DownloadStatus == DownloadStatus.Completed);
        var totalCount = versionTasks.Count;

        return new MinecraftDownloadProgress
        {
            VersionId = versionId,
            Name = $"{versionId} 下载进度",
            Size = totalBytes,
            DownloadBytes = downloadedBytes,
            Progress = totalBytes > 0 ? (double)downloadedBytes / totalBytes * 100 : 0,
            ProgressPercentage = totalCount > 0 ? $"{completedCount}/{totalCount}" : "0/0",
            DownloadStatus = GetVersionStatus(versionId)
        };
    }

    public async Task<bool> CanResumeDownloadAsync(string versionId, StorageOptions storageOptions, CancellationToken cancellationToken = default)
    {
        // 检查是否有部分下载的文件可以续传
        var versionInfo = await GetVersionInfoAsync(versionId, cancellationToken);
        if (versionInfo == null) return false;

        var tasks = await _vanillaHandle.GetDownloadTasksAsync(versionInfo, GetCurrentOs(), null);
        return tasks.Any(task =>
            File.Exists(task.SavePath) &&
            new FileInfo(task.SavePath).Length < task.Size &&
            new FileInfo(task.SavePath).Length > 0);
    }

    public async Task<bool> SetDownloadPriorityAsync(string versionId, DownloadPriority priority)
    {
        var versionTasks = DownloadQueue.Where(q => q.VersionId == versionId &&
            (q.DownloadStatus == DownloadStatus.Pending || q.DownloadStatus == DownloadStatus.Pause)).ToList();

        if (versionTasks.Count == 0)
            return false;

        foreach (var task in versionTasks)
        {
            task.Priority = priority;
            await UpdateProgressAsync(task);
        }

        return true;
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取版本信息（需要实现具体的版本信息获取逻辑）
    /// </summary>
    private async Task<GameVersionInfo?> GetVersionInfoAsync(string versionId, CancellationToken cancellationToken)
    {
        return await _minecraftVersionService.GetGameVersionInfoAsync(versionId);
    }

    /// <summary>
    /// 获取当前操作系统标识
    /// </summary>
    private static string GetCurrentOs()
    {
        if (OperatingSystem.IsWindows()) return "windows";
        if (OperatingSystem.IsLinux()) return "linux";
        if (OperatingSystem.IsMacOS()) return "osx";
        return "windows"; // 默认
    }

    /// <summary>
    /// 获取总体下载状态
    /// </summary>
    private DownloadStatus GetOverallStatus()
    {
        if (DownloadQueue.All(q => q.DownloadStatus == DownloadStatus.Completed))
            return DownloadStatus.Completed;
        if (DownloadQueue.Any(q => q.DownloadStatus == DownloadStatus.Downloading))
            return DownloadStatus.Downloading;
        if (DownloadQueue.Any(q => q.DownloadStatus == DownloadStatus.Pause))
            return DownloadStatus.Pause;
        if (DownloadQueue.Any(q => q.DownloadStatus == DownloadStatus.Failed))
            return DownloadStatus.Failed;
        return DownloadStatus.Pending;
    }

    /// <summary>
    /// 获取版本下载状态
    /// </summary>
    private DownloadStatus GetVersionStatus(string versionId)
    {
        var versionTasks = DownloadQueue.Where(q => q.VersionId == versionId).ToList();
        if (versionTasks.All(q => q.DownloadStatus == DownloadStatus.Completed))
            return DownloadStatus.Completed;
        if (versionTasks.Any(q => q.DownloadStatus == DownloadStatus.Downloading))
            return DownloadStatus.Downloading;
        if (versionTasks.Any(q => q.DownloadStatus == DownloadStatus.Pause))
            return DownloadStatus.Pause;
        if (versionTasks.Any(q => q.DownloadStatus == DownloadStatus.Failed))
            return DownloadStatus.Failed;
        return DownloadStatus.Pending;
    }

    /// <summary>
    /// 更新进度并触发事件
    /// </summary>
    private async Task UpdateProgressAsync(MinecraftDownloadProgress progress)
    {
        ProgressChanged?.Invoke(progress);

        // 更新观察集合中的项目
        await ExecuteOnUiThreadAsync(() =>
          {
              var existingItem = DownloadQueue.FirstOrDefault(q => q.Id == progress.Id);
              if (existingItem != null)
              {
                  var index = DownloadQueue.IndexOf(existingItem);
                  if (index >= 0)
                  {
                      DownloadQueue[index] = progress;
                  }
              }
          });
    }

    /// <summary>
    /// 在 UI 线程执行操作，这里可能需要接口额外定义一个Action里面放值来实现UI线程相关调用
    /// </summary>
    private async Task ExecuteOnUiThreadAsync(Action action)
    {
        // 这里需要根据具体的 UI 框架来实现
        // 通过事件传递给订阅者，由订阅者在 UI 线程上执行
        ExecuteOnUiThread?.Invoke(action);
        await Task.CompletedTask; // 确保一致性
    }

    /// <summary>
    /// 触发下载完成事件
    /// </summary>
    private void OnDownloadCompleted(MinecraftDownloadCompletedResult result)
    {
        DownloadCompleted?.Invoke(result);
    }

    /// <summary>
    /// 触发下载错误事件
    /// </summary>
    private void OnDownloadError(MinecraftDownloadError error)
    {
        DownloadError?.Invoke(error);
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _httpClient?.Dispose();
        _downloadSemaphore?.Dispose();

        foreach (var cts in _versionCancellationTokens.Values)
        {
            cts?.Dispose();
        }
        _versionCancellationTokens.Clear();

        _activeDownloads.Clear();
        DownloadQueue.Clear();
    }
    #endregion
}