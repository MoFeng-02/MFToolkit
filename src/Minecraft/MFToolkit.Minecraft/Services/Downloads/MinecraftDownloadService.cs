using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using MFToolkit.Minecraft.Entities.Downloads;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Helpers;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Downloads.Interfaces;
using MFToolkit.Minecraft.Services.Downloads.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MFToolkit.Minecraft.Services.Downloads;

/// <summary>
/// Minecraft下载服务实现类，负责管理游戏版本、核心文件和资源的下载流程
/// </summary>
public class MinecraftDownloadService : IMinecraftDownloadService
{
    // /// <summary>
    // /// 下载并发控制信号量，限制同时进行的下载任务数量
    // /// 【已弃用】由 EnhancedDownloadConcurrencyController 替代
    // /// </summary>
    // private SemaphoreSlim _downloadSemaphore; // 可以移除

    /// <summary>
    /// 当前使用的模组加载器类型
    /// </summary>
    public ModLoaderType ModLoaderType { get; set; }

    /// <summary>
    /// 下载进度变更事件（参数：进度信息，异常信息）
    /// </summary>
    public event Action<MinecraftDownloadProgress, Exception?>? ProgressChanged;

    /// <summary>
    /// 下载完成事件（触发于整个版本下载流程结束）
    /// </summary>
    public event Action<MinecraftDownloadCompletedResult>? DownloadCompleted;

    public Action<VersionInfoDetail>? CompletedInfoAction { get; set; }

    // /// <summary>
    // /// UI线程执行委托事件，用于跨线程操作UI元素
    // /// </summary>
    // public event Action<Action>? ExecuteOnUiThread;

    /// <summary>
    /// 下载队列，用于展示当前所有下载任务（非线程安全，需通过UI线程操作）
    /// </summary>
    public ObservableCollection<MinecraftDownloadProgress> DownloadQueue { get; } = [];

    /// <summary>
    /// 下载队列锁，保护非线程安全集合的操作
    /// </summary>
    private readonly Lock _downloadQueueLock = new();

    /// <summary>
    /// 存储配置选项
    /// </summary>
    public IOptionsMonitor<StorageOptions> StorageOptions { get; set; }

    /// <summary>
    /// 下载配置选项
    /// </summary>
    public IOptionsMonitor<DownloadOptions> DownloadOptions { get; set; }

    /// <summary>
    /// 版本文件路径字典，记录每个版本需要下载的文件路径及状态
    /// </summary>
    public Dictionary<string, List<MinecraftVersionAllFilePath>> DictionaryVersionPaths { get; set; } = [];

    /// <summary>
    /// 当前下载版本的取消令牌字典（线程安全）
    /// Key: 版本ID
    /// Value: 对应的取消令牌源（用于单独取消某个版本的下载）
    /// </summary>
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _versionCancellationTokens;

    /// <summary>
    /// 活动下载任务的进度字典（线程安全）
    /// Key: 下载任务唯一标识（Guid）
    /// Value: 进度信息对象（用于跟踪所有活跃任务）
    /// </summary>
    private readonly ConcurrentDictionary<Guid, MinecraftDownloadProgress> _activeDownloads;

    /// <summary>
    /// 活动下载任务字典（线程安全）
    /// Key: 下载URL（用于去重）
    /// Value: 对应的下载任务（用于判断URL是否正在下载）
    /// </summary>
    private readonly ConcurrentDictionary<string, Task> _activeTasks;

    /// <summary>
    /// 全局链接的取消令牌源，用于关联外部取消请求和内部取消操作
    /// </summary>
    private CancellationTokenSource _linkedCts = new();

    /// <summary>
    /// 当前活跃的下载任务数量
    /// </summary>
    public int ActiveDownloads => _activeTasks.Count;

    /// <summary>
    /// 原版Minecraft下载处理器
    /// </summary>
    private VanillaHandle _vanillaHandle;

    /// <summary>
    /// HTTP客户端，用于发送下载请求
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger<MinecraftDownloadService> _logger;

    /// <summary>
    /// 完成的任务
    /// </summary>
    private readonly ConcurrentDictionary<string, MinecraftDownloadCompletedResult> _completedDownloads = [];

    #region 并发配置

    /// <summary>
    /// 下载并发控制器
    /// </summary>
    private readonly EnhancedDownloadConcurrencyController _concurrencyController = new();

    #endregion

    /// <summary>
    /// 构造函数，初始化服务依赖
    /// </summary>
    public MinecraftDownloadService(
        HttpClient httpClient,
        ILogger<MinecraftDownloadService> logger,
        IOptionsMonitor<StorageOptions> storageOptions,
        IOptionsMonitor<DownloadOptions> downloadOptions)
    {
        _httpClient = httpClient;
        _logger = logger;
        StorageOptions = storageOptions;
        DownloadOptions = downloadOptions;

        _versionCancellationTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
        _activeDownloads = new ConcurrentDictionary<Guid, MinecraftDownloadProgress>();
        _activeTasks = new ConcurrentDictionary<string, Task>();
        // 初始化时同步一次当前配置（确保启动时使用正确的并发数）
        var initialMax = downloadOptions.CurrentValue.Settings.MaxDownloadThreads;
        _concurrencyController.OnDownloadOptionsChanged(initialMax, null);
        DownloadOptions.OnChange(OnDownloadOptionsChanged);
    }

    /// <summary>
    /// 处理下载配置变化
    /// </summary>
    private void OnDownloadOptionsChanged(DownloadOptions newOptions, string? name)
    {
        try
        {
            // 添加配置验证
            if (newOptions.Settings.MaxDownloadThreads <= 0)
            {
                _logger.LogWarning("无效的并发数配置: {MaxThreads}", newOptions.Settings.MaxDownloadThreads);
                return;
            }

            if (newOptions.Settings.DownloadTimeoutSeconds <= 0)
            {
                _logger.LogWarning("无效的超时配置: {Timeout}", newOptions.Settings.DownloadTimeoutSeconds);
                return;
            }

            // 更新HTTP超时
            _httpClient.Timeout = TimeSpan.FromSeconds(newOptions.Settings.DownloadTimeoutSeconds);

            // 从配置中获取新的最大并发数，传递给控制器
            _concurrencyController.OnDownloadOptionsChanged(
                newOptions.Settings.MaxDownloadThreads,
                name
            );

            _logger.LogInformation("下载配置已更新: 超时={Timeout}s, 并发数={Concurrency}",
                newOptions.Settings.DownloadTimeoutSeconds,
                newOptions.Settings.MaxDownloadThreads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新下载配置时发生异常");
        }
    }

    /// <summary>
    /// 获取指定版本的文件路径列表
    /// </summary>
    public List<MinecraftVersionAllFilePath> GetVersionPaths(string versionId)
    {
        return DictionaryVersionPaths.TryGetValue(versionId, out var paths) ? paths : [];
    }

    /// <summary>
    /// 开始下载指定的Minecraft版本（完整流程：清单->核心文件->资源文件）
    /// </summary>
    public async Task<bool> StartDownloadAsync(
        VersionInfo versionInfo,
        string? customName = null,
        StorageOptions? storageOptions = null,
        CancellationToken cancellationToken = default)
    {
        // 验证输入参数
        ArgumentNullException.ThrowIfNull(versionInfo);

        // 为当前版本创建专属取消令牌（可单独取消该版本下载）
        using var versionCts = CreateVersionCancellationTokenSource(versionInfo.Id, cancellationToken);

        // 创建版本总表
        var completedDownload = new MinecraftDownloadCompletedResult()
        {
            VersionId = versionInfo.Id,
        };
        try
        {
            storageOptions ??= StorageOptions.CurrentValue;
            _vanillaHandle = new VanillaHandle(
                storageOptions,
                DownloadOptions.CurrentValue,
                _logger,
                customName);
            ClearDownloadQueue();
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(versionCts.Token);

            _completedDownloads.TryAdd(versionInfo.Id, completedDownload);
            // 1. 下载版本详情清单
            var version = _vanillaHandle.GetGameVersionInfoJson(versionInfo);
            var versionProgress = version.SetDownloadProgress();
            DictionaryVersionPaths.TryAdd(versionInfo.Id, [version.SetDownloadVersionAllFilePath()]);

            // 批量添加：版本清单

            await StartDownloadAsync([versionProgress], versionCts.Token);

            // 2. 下载核心文件
            var gameVersion = await _vanillaHandle.GetGameVersionInfoAsync(version.SavePath);
            var coreDownloadTasks =
                await _vanillaHandle.GetDownloadTasksAsync(gameVersion, GetCurrentOs());
            var coreProgressList = coreDownloadTasks.Select(q => q.SetDownloadProgress()).ToList();

            // 批量添加：核心文件
            completedDownload.TotalTasks.AddRange(coreProgressList);
            // 批量更新文件路径
            UpdateVersionPathsBatch(versionInfo.Id, coreDownloadTasks.Select(q => q.SetDownloadVersionAllFilePath()));
            await StartDownloadAsync(coreProgressList, versionCts.Token);


            // 3. 下载资源文件
            var assetDownloadTasks = await _vanillaHandle.GetAssetDownloadTasksAsync(gameVersion);
            var assetProgressList = assetDownloadTasks.Select(q => q.SetDownloadProgress()).ToList();

            // 批量添加：资源文件（分块处理，避免一次性添加过多）
            completedDownload.TotalTasks.AddRange(assetProgressList);
            // 批量更新文件路径
            UpdateVersionPathsBatch(versionInfo.Id, assetDownloadTasks.Select(q => q.SetDownloadVersionAllFilePath()));
            await StartDownloadAsync(assetProgressList, versionCts.Token);


            // 全部下载完成后，执行一次获取版本信息的操作
            CompletedInfoAction?.Invoke(
                VersionInfoDetail.GetVersionInfoDetail(gameVersion, storageOptions, ModLoaderType, customName));
            return true;
        }
        // 改进：区分处理不同类型的异常
        catch (OperationCanceledException)
        {
            _logger.LogWarning("版本 {VersionId} 下载被取消", versionInfo.Id);
            return false;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "网络错误导致版本 {VersionId} 下载失败", versionInfo.Id);
            return false;
        }
        catch (IOException ioEx)
        {
            _logger.LogError(ioEx, "文件IO错误导致版本 {VersionId} 下载失败", versionInfo.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "未知错误导致版本 {VersionId} 下载失败", versionInfo.Id);
            return false;
        }
        finally
        {
            // 触发完成事件
            TriggerDownloadCompleted(versionInfo.Id);
            // 清理当前版本的取消令牌
            _versionCancellationTokens.TryRemove(versionInfo.Id, out _);
        }
    }

    /// <summary>
    /// 批量下载多个版本（未实现）
    /// </summary>
    public Task<int> StartBatchDownloadAsync(
        IEnumerable<VersionInfo> versionInfos,
        IEnumerable<string>? customNames = null,
        StorageOptions? storageOptions = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 暂停所有下载（未实现）
    /// </summary>
    public Task<bool> PauseDownloadAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 暂停指定版本的下载（未实现）
    /// </summary>
    public Task<bool> PauseDownloadAsync(string versionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 取消所有下载
    /// </summary>
    public Task<bool> CancelDownloadAsync()
    {
        try
        {
            _linkedCts.Cancel();
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取消所有下载失败");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 取消指定版本的下载
    /// </summary>
    public Task<bool> CancelDownloadAsync(string versionId)
    {
        if (_versionCancellationTokens.TryGetValue(versionId, out var cts))
        {
            try
            {
                cts.Cancel();
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消版本 {VersionId} 下载失败", versionId);
            }
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// 验证游戏文件完整性（未实现）
    /// </summary>
    public Task<MinecraftValidationFileResult> ValidateGameFilesAsync(
        string versionId,
        StorageOptions storageOptions,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 检查版本是否已安装（未实现）
    /// </summary>
    public Task<bool> IsVersionInstalledAsync(
        string versionId,
        StorageOptions storageOptions,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取整体下载进度（未实现）
    /// </summary>
    public MinecraftDownloadProgress GetDownloadProgress()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取指定版本的下载进度
    /// </summary>
    public MinecraftDownloadProgress GetDownloadProgress(string versionId)
    {
        // 从活跃下载中筛选指定版本的任务
        var versionProgresses = _activeDownloads.Values
            .Where(p => p.VersionId == versionId)
            .ToList();

        if (versionProgresses.Count == 0)
        {
            return new MinecraftDownloadProgress { VersionId = versionId, DownloadStatus = DownloadStatus.None };
        }

        // 计算版本整体进度（平均进度）
        var totalProgress = versionProgresses.Average(p => p.Progress);
        return new MinecraftDownloadProgress
        {
            VersionId = versionId,
            Progress = Math.Round(totalProgress, 2),
            DownloadStatus = GetAggregateStatus(versionProgresses)
        };
    }

    /// <summary>
    /// 检查是否可以续传下载（未实现）
    /// </summary>
    public Task<bool> CanResumeDownloadAsync(
        string versionId,
        StorageOptions storageOptions,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 设置下载优先级（未实现）
    /// </summary>
    public Task<bool> SetDownloadPriorityAsync(string versionId, DownloadPriority priority)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        // 释放并发控制器
        _concurrencyController.Dispose();

        _httpClient.Dispose();
        // 按正确顺序释放资源
        _linkedCts.Cancel();
        _linkedCts.Dispose();
        // 取消所有版本下载
        foreach (var cts in _versionCancellationTokens.Values)
        {
            try
            {
                cts.Cancel();
                cts.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放取消令牌源时发生异常");
            }
        }

        _versionCancellationTokens.Clear();
        _activeDownloads.Clear();
        _activeTasks.Clear();

        // 修复：添加控制器释放
        _concurrencyController.Dispose();
        // ExecuteOnUiThread?.Invoke(DownloadQueue.Clear);
    }

    #region 私有方法组

    /// <summary>
    /// 启动多个下载任务
    /// </summary>
    /// <param name="items">下载任务列表</param>
    /// <param name="cancellationToken">取消令牌（关联到具体版本）</param>
    private async Task StartDownloadAsync(List<MinecraftDownloadProgress> items, CancellationToken cancellationToken)
    {
        var parallelOptions = new ParallelOptions
        {
            // 使用并发控制器提供的最大并发数
            MaxDegreeOfParallelism = _concurrencyController.GetCurrentMaxConcurrency(),
            CancellationToken = cancellationToken
        };

        try
        {
            // 使用线程安全的集合来跟踪任务
            await Parallel.ForEachAsync(items, parallelOptions, async (item, ct) =>
            {
                // 将任务添加到线程安全集合
                lock (_downloadQueueLock)
                {
                    DownloadQueue.Add(item);
                }

                var downloadTask = ProcessDownloadAsync(item, ct);
                _activeTasks[item.DownloadUrl] = downloadTask;

                try
                {
                    await downloadTask;
                }
                finally
                {
                    _activeTasks.TryRemove(item.DownloadUrl, out _);
                }
            });

            // 等待所有任务完成
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"版本 {items.FirstOrDefault()?.VersionId} 并行下载被取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "并行下载执行失败");
            throw;
        }
        // var tasks = items.Select(item =>
        // {
        //     var downloadTask = ProcessDownloadAsync(item, cancellationToken);
        //     _activeTasks[item.DownloadUrl] = downloadTask;
        //     return downloadTask;
        // });

        // await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 处理单个下载项的完整生命周期
    /// </summary>
    private async Task ProcessDownloadAsync(
        MinecraftDownloadProgress item,
        CancellationToken token)
    {
        var stopwatch = Stopwatch.StartNew();
        // 添加到活跃下载字典
        _activeDownloads.TryAdd(item.Id, item);

        try
        {
            _logger.LogDebug("任务 {TaskName} 进入并发控制器队列", item.Name);
            // // 关键：用并发控制器包裹下载逻辑，自动控制并发数
            // await _concurrencyController.ExecuteWithConcurrencyControl(async (_) =>
            // {
            //     // 在信号量中添加避免多个添加处理
            //     DownloadQueue.Add(item);
            //     _logger.LogDebug("任务 {TaskName} 开始执行下载", item.Name);
            //     // 原有的下载前状态更新
            //     item.UpdateStatus(DownloadStatus.Downloading);
            //     _logger.LogInformation("开始下载: {Name}（URL: {Url}）", item.Name, item.DownloadUrl);
            //
            //     // 下载逻辑包括重试逻辑
            //     await DownloadWithRetryAsync(item, token);
            //
            //     // 下载后处理（如校验、解压等）
            //     await HandlePostDownload(item, token);
            //
            //     // 下载完成状态更新
            //     item.UpdateStatus(DownloadStatus.Completed);
            //     _logger.LogInformation("下载完成: {Name}（耗时: {Elapsed}ms）", item.Name, stopwatch.ElapsedMilliseconds);
            //
            //     return true; // 占位返回值（控制器需要泛型返回，无实际意义）
            // }, token); // 传入取消令牌，支持任务取消

            // ExecuteOnUiThread?.Invoke(() => ProgressChanged?.Invoke(item, null));
            // 直接执行下载，不再使用并发控制器包装
            item.UpdateStatus(DownloadStatus.Downloading);
            _logger.LogInformation("开始下载: {Name}（URL: {Url}）", item.Name, item.DownloadUrl);

            // 下载逻辑包括重试逻辑
            await DownloadWithRetryAsync(item, token);

            // 下载后处理（如校验、解压等）
            await HandlePostDownload(item, token);

            // 下载完成状态更新
            item.UpdateStatus(DownloadStatus.Completed);
            _logger.LogInformation("下载完成: {Name}（耗时: {Elapsed}ms）", item.Name, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            item.UpdateStatus(DownloadStatus.Cancelled);
            _logger.LogWarning("下载被取消: {Name}", item.Name);
            // ExecuteOnUiThread?.Invoke(() => ProgressChanged?.Invoke(item, null));
        }
        catch (Exception ex)
        {
            item.UpdateStatus(DownloadStatus.Failed, ex.Message);
            _logger.LogError(ex, "下载失败: {Name} (URL: {Url})", item.Name, item.DownloadUrl);
            // ExecuteOnUiThread?.Invoke(() => ProgressChanged?.Invoke(item, ex));
        }
        finally
        {
            stopwatch.Stop();
            _activeTasks.TryRemove(item.DownloadUrl, out _);
            _activeDownloads.TryRemove(item.Id, out _); // 从活跃下载中移除
            // 使用锁保护集合操作
            lock (_downloadQueueLock)
            {
                DownloadQueue.Remove(item);
            }
        }
    }

    /// <summary>
    /// 处理下载完成后的后续操作（更新文件状态）
    /// </summary>
    private async Task HandlePostDownload(MinecraftDownloadProgress item, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            var versionFile = GetVersionPaths(item.VersionId)
                .FirstOrDefault(q => q.SavePath == item.SavePath);

            if (versionFile != null)
            {
                versionFile.IsDownloadSuccess = true;
                versionFile.LastUpdatedTime = DateTime.Now; // 记录下载完成时间
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载后处理失败: {File}", item.SavePath);
            throw;
        }
    }

    /// <summary>
    /// 带重试机制的下载方法
    /// </summary>
    private async Task DownloadWithRetryAsync(
        MinecraftDownloadProgress item,
        CancellationToken token,
        int maxRetries = 3)
    {
        for (var retry = 0; retry < maxRetries; retry++)
        {
            // 是否是最后一次尝试
            var isLastRetry = retry == maxRetries - 1;
            try
            {
                if (retry == 0 && await FileHashChecker.CheckFileSha1Async(item.SavePath, item.Sha1, _logger, token))
                {
                    item.UpdateStatus(DownloadStatus.Completed);
                    item.Progress = 100;
                    _logger.LogInformation("文件已存在且完整: {Path}", item.SavePath);
                    return;
                }

                var directory = Path.GetDirectoryName(item.SavePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await DownloadFileAsync(item, token, isLastRetry);
                return;
            }
            catch (Exception ex) when (retry < maxRetries - 1 && !token.IsCancellationRequested)
            {
                // 404等不可恢复的错误尝试以源url执行一次
                if (ex is HttpRequestException { StatusCode: HttpStatusCode.NotFound or HttpStatusCode.Forbidden })
                {
                    _logger.LogError(ex, "文件不可访问，重试一次: {Url}，以源url执行{OriginUrl}", item.DownloadUrl, item.OriginUrl);
                    // 判断是否是最后一回，或者源url和实际url本就相同就弹出错误
                    if (isLastRetry || item.OriginUrl == item.DownloadUrl)
                        throw;

                    retry = maxRetries - 2;
                    // 进入下一回
                    continue;
                }

                item.UpdateStatus(DownloadStatus.Retrying);
                item.Error = $"下载失败，正在重试（{retry + 1}/{maxRetries}）：{ex.Message}";
                _logger.LogWarning(ex, "下载重试 {Retry}/{MaxRetries}: {Name}",
                    retry + 1, maxRetries, item.Name);

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)), token);
            }
        }

        throw new InvalidOperationException($"超过最大重试次数（{maxRetries}次）: {item.DownloadUrl}");
    }

    /// <summary>
    /// 执行文件下载的核心逻辑
    /// </summary>
    private async Task DownloadFileAsync(MinecraftDownloadProgress item, CancellationToken token,
        bool isStartOrigin = false)
    {
        using var response = await _httpClient.GetAsync(
            isStartOrigin ? item.OriginUrl : item.DownloadUrl,
            HttpCompletionOption.ResponseHeadersRead,
            token);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(token);
        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var totalBytesRead = 0L;
        var buffer = new byte[8192];

        // 直接在方法内创建 SHA1 实例
        using var sha1 = SHA1.Create();


        await using var fileStream = new FileStream(
            item.SavePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            buffer.Length,
            useAsync: true);

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer, token);
            if (bytesRead == 0) break;

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);

            sha1.TransformBlock(buffer, 0, bytesRead, null, 0);

            totalBytesRead += bytesRead;

            // 更新进度
            if (totalBytes > 0)
            {
                item.UpdateProgress(Math.Round((double)totalBytesRead / totalBytes * 100, 2));
            }
            else
            {
                item.UpdateProgress(-1); // 未知进度
            }

            // ExecuteOnUiThread?.Invoke(() => ProgressChanged?.Invoke(item, null));
        }

        // 完成哈希计算
        sha1.TransformFinalBlock([], 0, 0);
        var hashBytes = sha1.Hash;
        var actualHash = hashBytes != null ? Convert.ToHexStringLower(hashBytes) : null;
        await fileStream.FlushAsync(token);
        // 校验哈希
        if (actualHash != null && !string.IsNullOrEmpty(item.Sha1) && actualHash != item.Sha1)
        {
            File.Delete(item.SavePath);
            throw new InvalidDataException($"文件SHA1校验失败: {item.SavePath} (期望: {item.Sha1}, 实际: {actualHash})");
        }
    }

    /// <summary>
    /// 清空下载队列
    /// </summary>
    private void ClearDownloadQueue()
    {
        // foreach (var item in DownloadQueue.Where(i =>
        //              i.DownloadStatus == DownloadStatus.Downloading ||
        //              i.DownloadStatus == DownloadStatus.Retrying))
        // {
        //     item.UpdateStatus(DownloadStatus.Cancelled);
        // }
        lock (_downloadQueueLock)
        {
            DownloadQueue.Clear();
        }

        _logger.LogInformation("下载队列已清空");
        // ExecuteOnUiThread?.Invoke(DownloadQueue.Clear);
    }

    /// <summary>
    /// 获取当前操作系统标识
    /// </summary>
    private static string GetCurrentOs()
    {
        if (OperatingSystem.IsWindows()) return "windows";
        if (OperatingSystem.IsLinux()) return "linux";
        if (OperatingSystem.IsMacOS()) return "osx";
        return "windows";
    }

    /// <summary>
    /// 计算多个下载任务的聚合状态
    /// </summary>
    /// <param name="progresses">进度列表</param>
    /// <returns>聚合状态（失败>取消>下载中>完成>未开始）</returns>
    private static DownloadStatus GetAggregateStatus(IEnumerable<MinecraftDownloadProgress> progresses)
    {
        var statuses = progresses.Select(p => p.DownloadStatus).ToList();

        if (statuses.Any(s => s == DownloadStatus.Failed)) return DownloadStatus.Failed;
        if (statuses.Any(s => s == DownloadStatus.Cancelled)) return DownloadStatus.Cancelled;
        if (statuses.Any(s => s is DownloadStatus.Downloading or DownloadStatus.Retrying))
            return DownloadStatus.Downloading;
        return statuses.All(s => s == DownloadStatus.Completed) ? DownloadStatus.Completed : DownloadStatus.None;
    }

    /// <summary>
    /// 为版本创建取消令牌源
    /// </summary>
    private CancellationTokenSource CreateVersionCancellationTokenSource(string versionId,
        CancellationToken externalToken)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

        // 添加超时保护
        cts.CancelAfter(TimeSpan.FromHours(2)); // 2小时超时

        if (!_versionCancellationTokens.TryAdd(versionId, cts))
        {
            cts.Dispose();
            throw new InvalidOperationException($"版本 {versionId} 的下载任务已存在");
        }

        return cts;
    }

    /// <summary>
    /// 批量更新版本文件路径
    /// </summary>
    private void UpdateVersionPathsBatch(string versionId, IEnumerable<MinecraftVersionAllFilePath> filePaths)
    {
        var paths = filePaths.ToList();
        if (paths.Count == 0) return;

        if (DictionaryVersionPaths.TryGetValue(versionId, out var existingPaths))
        {
            existingPaths.AddRange(paths);
        }
        else
        {
            DictionaryVersionPaths[versionId] = paths;
        }
    }

    private void TriggerDownloadCompleted(string versionId)
    {
        // // 筛选成功的任务（仅状态为 Completed）
        // var successTasks = DownloadQueue
        //     .Where(q => q.DownloadStatus == DownloadStatus.Completed);
        //
        // // 筛选失败的任务（仅状态为 Failed，排除 Cancelled）
        // var failedTasks = DownloadQueue
        //     .Where(q => q.DownloadStatus == DownloadStatus.Failed);
        //
        // // 可选：单独记录取消的任务（如果需要）
        // var cancelledTasks = DownloadQueue
        //     .Where(q => q.DownloadStatus == DownloadStatus.Cancelled);
        // 触发整体下载完成事件
        DownloadCompleted?.Invoke(_completedDownloads[versionId]);
    }

    #endregion
}