using MFToolkit.Download.Handler;
using MFToolkit.Download.Models;
using MFToolkit.Extensions;
using MFToolkit.Http;
using Microsoft.Extensions.Logging;

namespace MFToolkit.Download.Services;
/// <summary>
/// 下载服务
/// </summary>
public class DownloadService : IDownloadService, IDisposable
{
    private readonly ILogger<DownloadService>? _logger;
    /// <summary>
    /// 下载Token
    /// </summary>
    protected virtual CancellationTokenSource CancellationTokenSource { get; set; } = new();
    /// <summary>
    /// 下载Token
    /// </summary>
    protected virtual CancellationToken CancellationToken { get; set; }
    /// <summary>
    /// 当前下载模型
    /// </summary>
    protected virtual DownloadModel currentDownloadModel { get; set; } = null!;
    /// <summary>
    /// 关于本字段，如遇到未注册，请注册，参考：
    /// <para>
    /// MFToolkit/Injects/GlobalInjects.cs下的AddInjectServices方法
    /// </para>
    /// </summary>
    protected readonly HttpClientService httpClient;
    //private readonly DownloadHandler? downloadHandler;
    /// <summary>
    /// 暂停信息处理
    /// </summary>
    protected virtual DownloadPauseInfoHandler? DownloadPauseInfoHandler { get; set; }
    /// <summary>
    /// 是否暂停
    /// </summary>
    private bool isPause;
    /// <summary>
    /// 是否是手动停止
    /// </summary>
    private bool isStop;

    /// <summary>
    /// 自动重新下载，默认自动重新下载，但是最大只能尝试五次，每次等待5秒，目前是如此限制，暂时不开放手动重试逻辑，因为有一些处理还未完善，代码有待优化
    /// </summary>
    public bool AutoRedownload { get; set; } = true;
    /// <summary>
    /// 下载状态
    /// </summary>
    public Action<bool, DownloadState, Exception?>? DownloadStateAction { get; set; }
    /// <summary>
    /// 下载进度
    /// <para>0: 当前下载大小，1: 总大小</para>
    /// </summary>
    public Action<long, long>? DownloadProgress { get; set; }

    /// <summary>
    /// 下载服务
    /// </summary>
    /// <param name="httpClientService"></param>
    public DownloadService(HttpClientService httpClientService)
    {
        httpClient = httpClientService;
        CancellationToken = CancellationTokenSource.Token;
        //downloadHandler = AppUtil.GetService<DownloadHandler>();
    }

    /// <summary>
    /// 下载服务
    /// </summary>
    /// <param name="httpClientService"></param>
    /// <param name="logger"></param>
    public DownloadService(HttpClientService httpClientService, ILogger<DownloadService> logger)
    {
        httpClient = httpClientService;
        CancellationToken = CancellationTokenSource.Token;
        _logger = logger;
        //downloadHandler = AppUtil.GetService<DownloadHandler>();
    }

    /// <summary>
    /// 下载服务
    /// </summary>
    /// <param name="httpClientService"></param>
    /// <param name="downloadPauseInfoHandler"></param>
    public DownloadService(HttpClientService httpClientService, DownloadPauseInfoHandler downloadPauseInfoHandler)
    {
        httpClient = httpClientService;
        CancellationToken = CancellationTokenSource.Token;
        //downloadHandler = AppUtil.GetService<DownloadHandler>();
        DownloadPauseInfoHandler = downloadPauseInfoHandler;
    }

    /// <summary>
    /// 下载服务
    /// </summary>
    /// <param name="httpClientService"></param>
    /// <param name="downloadPauseInfoHandler"></param>
    /// <param name="logger"></param>
    public DownloadService(HttpClientService httpClientService, DownloadPauseInfoHandler downloadPauseInfoHandler, ILogger<DownloadService> logger)
    {
        httpClient = httpClientService;
        CancellationToken = CancellationTokenSource.Token;
        DownloadPauseInfoHandler = downloadPauseInfoHandler;
        _logger = logger;
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        httpClient.Dispose();
    }
    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="downloadModel"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<DownloadResult?> DownloadAsync(DownloadModel downloadModel)
    {
        if (string.IsNullOrWhiteSpace(downloadModel.FileSavePath) || string.IsNullOrWhiteSpace(downloadModel.DownloadUrl))
        {
            _logger?.LogError("下载失败：关键内容为空（文件保存路径或下载URL）");
            throw new Exception("关键内容为空");
        }

        _logger?.LogInformation("开始下载：{DownloadUrl}，保存路径：{FileSavePath}", downloadModel.DownloadUrl, downloadModel.FileSavePath);
        DownloadResult? result = null;
        currentDownloadModel = downloadModel;
        if (AutoRedownload)
        {
            int autoCount = 0;
            while (true)
            {
                if (autoCount > 5)
                {
                    _logger?.LogWarning("重试次数超过5次，取消下载");
                    break;
                }
                try
                {
                    result = await DownloadActionAsync();
                    _logger?.LogInformation("下载成功：{DownloadUrl}", downloadModel.DownloadUrl);
                    break;
                }
                catch (Exception ex)
                {
                    result = null;
                    if (isPause)
                    {
                        _logger?.LogWarning(ex, "下载暂停");
                        DownloadStateAction?.Invoke(false, DownloadState.PauseDownloading, ex);
                        break;
                    }
                    else if (isStop)
                    {
                        _logger?.LogWarning(ex, "下载取消");
                        DownloadStateAction?.Invoke(false, DownloadState.Stop, ex);
                        break;
                    }
                    else
                        HandleDownloadException(ex);
                    _logger?.LogError(ex, "下载过程中发生异常：{Message}", ex.Message);
                    autoCount = await RetryAsync(autoCount, ex);
                }
            }
        }
        else
        {
            try
            {
                result = await DownloadActionAsync();
                _logger?.LogInformation("下载成功：{DownloadUrl}", downloadModel.DownloadUrl);
            }
            catch (Exception ex)
            {
                if (isPause)
                {
                    _logger?.LogWarning(ex, "下载暂停");
                    DownloadStateAction?.Invoke(false, DownloadState.PauseDownloading, ex);
                }
                else if (isStop)
                {
                    _logger?.LogWarning(ex, "下载取消");
                    DownloadStateAction?.Invoke(false, DownloadState.Stop, ex);
                }
                else
                {
                    _logger?.LogError(ex, "下载过程中发生异常：{Message}", ex.Message);
                    HandleDownloadException(ex);
                }
                result = null;
            }
        }
        return result;
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="savePath">保存位置</param>
    /// <returns></returns>
    public async Task<DownloadResult?> DownloadAsync(string url, string savePath)
    {
        return await DownloadAsync(new DownloadModel
        {
            DownloadUrl = url,
            FileSavePath = savePath
        });
    }

    /// <summary>
    /// 重试下载
    /// </summary>
    /// <param name="autoCount"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    private async Task<int> RetryAsync(int autoCount, Exception ex)
    {
        _logger?.LogWarning("下载失败，等待5秒后重试。重试次数：{AutoCount}", autoCount + 1);
        Console.WriteLine("等待5秒重试");
        // 等待进行重试
        await Task.Delay(1000 * 5);
        autoCount++;
        await Console.Out.WriteLineAsync($"尝试重新下载：{autoCount}次");
        DownloadStateAction?.Invoke(false, DownloadState.RetryDownloading, ex);
        return autoCount;
    }
    /// <summary>
    /// 下载操作
    /// </summary>
    /// <returns></returns>
    private async Task<DownloadResult> DownloadActionAsync()
    {
        _logger?.LogInformation("初始化下载：{DownloadUrl}", currentDownloadModel.DownloadUrl);
        DownloadStateAction?.Invoke(false, DownloadState.Init, null);
        // 更新偏移量
        httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(currentDownloadModel.YetSize, null);
        // 读取请求报头
        using var response = await httpClient.GetAsync(currentDownloadModel.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, CancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger?.LogError("下载失败，HTTP状态码：{StatusCode}，错误信息：{ErrorMessage}", response.StatusCode, errorMessage);
            return new DownloadResult { Message = errorMessage };
        }
        // 修改文件流使用方式（确保异步释放）
        await using var contentStream = await response.Content.ReadAsStreamAsync();
        // 创建文件流
        await using var fileStream = new FileStream(
            currentDownloadModel.FileSavePath,
            currentDownloadModel.YetSize == 0 ? FileMode.Create : FileMode.Append,
            FileAccess.Write,
            FileShare.None,
            bufferSize: currentDownloadModel.WriteSize, // 使用缓冲区大小
            useAsync: true);
        // 每次写入最大缓存
        byte[] buffer = new byte[currentDownloadModel.WriteSize];
        long sumCount = response.Content.Headers.GetHeaderValuesFirst<long>("Content-Length");
        int bytesRead;
        currentDownloadModel.SumSize ??= sumCount;
        //DownloadStateAction?.Invoke(false, DownloadState.Ready, null);
        _logger?.LogInformation("开始下载文件，总大小：{SumCount} 字节", sumCount);
        DownloadStateAction?.Invoke(true, DownloadState.Start, null);
        //DownloadStateAction?.Invoke(true, DownloadState.BeDownloading, null);
        while ((bytesRead = await contentStream.ReadAsync(buffer, CancellationToken)) > 0)
        {
            CancellationToken.ThrowIfCancellationRequested();
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), CancellationToken);
            currentDownloadModel.YetSize += bytesRead;
            DownloadProgress?.Invoke(currentDownloadModel.YetSize, currentDownloadModel.SumSize ?? sumCount);
            // 检查取消标记是否被设置，如果被设置则抛出异常以中止下载
        }
        _logger?.LogInformation("下载完成：{DownloadUrl}", currentDownloadModel.DownloadUrl);
        DownloadStateAction?.Invoke(false, DownloadState.CompleteDownloading, null);
        return new DownloadResult { Success = true, Message = "完成", Size = sumCount };
    }
    /// <summary>
    /// 暂停下载
    /// </summary>
    /// <returns></returns>
    public async Task PauseDownloadAsync()
    {
        _logger?.LogInformation("暂停下载：{DownloadUrl}", currentDownloadModel?.DownloadUrl);
        CancellationTokenSource.Cancel();
        isPause = true;
        DownloadStateAction?.Invoke(true, DownloadState.PauseDownloading, null);

        if (DownloadPauseInfoHandler == null)
        {
            _logger?.LogWarning("未注册 DownloadPauseInfoHandler，无法保存暂停信息");
            DownloadStateAction?.Invoke(false, DownloadState.OtherError, new("请注册：AddDownloadPauseInfoHandler，服务以确保正常使用暂停信息处理"));
            return;
        }
        _logger?.LogInformation("保存暂停信息：{DownloadUrl}", currentDownloadModel?.DownloadUrl);
        // 保存暂停信息
        await DownloadPauseInfoHandler.SavePauseInfoAsync(currentDownloadModel);
    }
    /// <summary>
    /// 继续下载
    /// </summary>
    /// <returns></returns>
    public async Task ResumeDownloadAsync()
    {
        _logger?.LogInformation("继续下载：{DownloadUrl}", currentDownloadModel?.DownloadUrl);
        CancellationTokenSource = new();
        CancellationToken = CancellationTokenSource.Token;
        isPause = false;
        DownloadStateAction?.Invoke(true, DownloadState.ResumeDownloading, null);
        await StartDownloadAsync();
        await Task.CompletedTask;
    }
    /// <summary>
    /// 停止下载
    /// </summary>
    /// <returns></returns>
    public async Task StopDownloadAsync()
    {
        _logger?.LogInformation("停止下载：{DownloadUrl}", currentDownloadModel?.DownloadUrl);
        isStop = true;
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();

        DownloadStateAction?.Invoke(false, DownloadState.Stop, null);
        // 删除已下载的数据
        if (File.Exists(currentDownloadModel?.FileSavePath))
        {
            File.Delete(currentDownloadModel.FileSavePath);
        }
        // 删除暂停信息
        DownloadPauseInfoHandler?.DeletePauseInfo(currentDownloadModel?.Key);
        await Task.CompletedTask;
    }
    /// <summary>
    /// 开始下在
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task StartDownloadAsync(DownloadModel? downloadModel = null)
    {
        //DownloadStateAction?.Invoke(false, DownloadState.Start, null);
        await DownloadAsync(currentDownloadModel);
    }

    /// <summary>
    /// 处理下载异常
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    private void HandleDownloadException(Exception ex)
    {
        if (ex is HttpRequestException httpEx)
        {
            Console.WriteLine($"网络错误: {httpEx.StatusCode}");
            DownloadStateAction?.Invoke(false, DownloadState.NetworkError, httpEx);
        }
        else if (ex is IOException ioEx && ioEx.Message.Contains("being used by another process"))
        {
            Console.WriteLine($"文件被占用: {ioEx.Message}");
            DownloadStateAction?.Invoke(false, DownloadState.FileLocked, ioEx);
        }
        else if (ex is UnauthorizedAccessException accessEx)
        {
            Console.WriteLine($"权限不足: {accessEx.Message}");
            DownloadStateAction?.Invoke(false, DownloadState.AccessDenied, accessEx);
        }
        else
        {
            DownloadStateAction?.Invoke(false, DownloadState.DownloadingError, ex);
        }
    }
}
