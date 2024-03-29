using MFToolkit.Download.DownloadHandlers;
using MFToolkit.Download.Models;
using MFToolkit.Extensions;
using MFToolkit.Utils.HttpExtensions;

namespace MFToolkit.Download.DownloadServices;
public class DownloadService : IDownloadService, IDisposable
{
    /// <summary>
    /// 下载Token
    /// </summary>
    private CancellationTokenSource CancellationTokenSource = new();
    private CancellationToken CancellationToken;
    private DownloadModel? currentDownloadModel;
    private readonly HttpClientService httpClient;
    private readonly DownloadHandler downloadHandler;
    /// <summary>
    /// 是否下载中
    /// </summary>
    private bool Downloading = false;
    /// <summary>
    /// 是否暂停
    /// </summary>
    private bool isPause;
    /// <summary>
    /// 是否是手动停止
    /// </summary>
    private bool isStop;

    public bool AutoRedownload { get; set; } = true;
    public Action<bool, DownloadState, Exception?>? DownloadStateAction { get; set; }
    public Action<long, long>? DownloadProgress { get; set; }

    public DownloadService(HttpClientService _httpClient, DownloadHandler downloadHandler)
    {
        httpClient = _httpClient;
        CancellationToken = CancellationTokenSource.Token;
        this.downloadHandler = downloadHandler;
    }

    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        httpClient.Dispose();
    }

    public async Task<DownloadResult?> DownloadAsync(DownloadModel downloadModel)
    {
        if (string.IsNullOrWhiteSpace(downloadModel.FileSavePath) || string.IsNullOrWhiteSpace(downloadModel.DownloadUrl)) throw new Exception("关键内容为空");
        DownloadResult? result = null;
        if (AutoRedownload)
        {
            int autoCount = 0;
            while (true)
            {
                if (autoCount > 5)
                {
                    await Console.Out.WriteLineAsync("重试大于5次，已取消");
                    break;
                }
                try
                {
                    result = await DownloadActionAsync(downloadModel);
                }
                catch (Exception ex)
                {
                    result = null;
                    if (isPause)
                    {
                        DownloadStateAction?.Invoke(false, DownloadState.PauseDownloading, ex);
                        await downloadHandler.SetPauseInfoAsync(downloadModel);
                        continue;
                    }
                    else if (isStop)
                    {
                        DownloadStateAction?.Invoke(false, DownloadState.Stop, ex);
                        break;
                    }
                    else
                        DownloadStateAction?.Invoke(false, DownloadState.DownloadingError, ex);
                    // 等待进行重试
                    await Task.Delay(1000 * 5);
                    autoCount++;
                    await Console.Out.WriteLineAsync($"尝试重新下载：{autoCount}次");
                    DownloadStateAction?.Invoke(false, DownloadState.RetryDownloading, ex);
                    continue;
                }
                break;
            }
        }
        else
        {
            try
            {
                result = await DownloadActionAsync(downloadModel);
            }
            catch (Exception ex)
            {
                if (isPause)
                {
                    DownloadStateAction?.Invoke(false, DownloadState.PauseDownloading, ex);
                }
                else if (isStop)
                {
                    DownloadStateAction?.Invoke(false, DownloadState.Stop, ex);
                }
                else
                    DownloadStateAction?.Invoke(false, DownloadState.DownloadingError, ex);
                result = null;
            }
        }
        return result;
    }
    /// <summary>
    /// 下载操作
    /// </summary>
    /// <param name="downloadModel"></param>
    /// <returns></returns>
    private async Task<DownloadResult> DownloadActionAsync(DownloadModel downloadModel)
    {
        DownloadStateAction?.Invoke(false, DownloadState.Init, null);
        currentDownloadModel = downloadModel;
        // 更新偏移量
        httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(downloadModel.YetDownloadSize, null);
        // 读取请求报头
        using var response = await httpClient.GetAsync(downloadModel.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, CancellationToken);
        DownloadResult downloadResult = new();
        if (!response.IsSuccessStatusCode)
        {
            downloadResult.Message = await response.Content.ReadAsStringAsync();
            return downloadResult;
        }
        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        using FileStream fileStream = downloadModel.YetDownloadSize == 0 ? new(downloadModel.FileSavePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None) : new(downloadModel.FileSavePath, FileMode.Append, FileAccess.Write, FileShare.None);
        // 每次写入最大缓存
        byte[] buffer = new byte[downloadModel.WirteSize];
        long sumCount = response.Content.Headers.GetHeaderValuesFirst<long>("Content-Length");
        int bytesRead;
        downloadModel.SumDownloadSize ??= sumCount;
        downloadResult.Size = sumCount;
        DownloadStateAction?.Invoke(false, DownloadState.Ready, null);
        DownloadStateAction?.Invoke(true, DownloadState.Start, null);
        DownloadStateAction?.Invoke(true, DownloadState.BeDownloading, null);
        while ((bytesRead = await contentStream.ReadAsync(buffer, CancellationToken)) > 0)
        {
            CancellationToken.ThrowIfCancellationRequested();
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), CancellationToken);
            downloadModel.YetDownloadSize += bytesRead;
            DownloadProgress?.Invoke(downloadModel.YetDownloadSize, downloadModel.SumDownloadSize ?? sumCount);
            // 检查取消标记是否被设置，如果被设置则抛出异常以中止下载
        }
        downloadResult.Success = true;
        downloadResult.Message = "完成";
        await fileStream.DisposeAsync();
        return downloadResult;
    }
    public async Task PauseDownloadAsync()
    {
        CancellationTokenSource.Cancel();
        isPause = true;
        await downloadHandler.SetPauseInfoAsync(currentDownloadModel);
        DownloadStateAction?.Invoke(true, DownloadState.PauseDownloading, null);
    }

    public async Task ResumeDownloadAsync()
    {
        CancellationTokenSource = new();
        CancellationToken = CancellationTokenSource.Token;
        isPause = false;
        DownloadStateAction?.Invoke(true, DownloadState.ResumeDownloading, null);
        await Task.CompletedTask;
    }

    public async Task StopDownloadAsync()
    {
        isStop = true;
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();

        DownloadStateAction?.Invoke(false, DownloadState.Stop, null);
        await Task.CompletedTask;
    }
}
