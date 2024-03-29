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

    public DownloadService(HttpClientService _httpClient)
    {
        httpClient = _httpClient;
        CancellationToken = CancellationTokenSource.Token;
    }

    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        httpClient.Dispose();
    }

    public async Task<DownloadResult?> DownloadAsync(DownloadModel downloadModel)
    {
        if (string.IsNullOrWhiteSpace(downloadModel.FileSavePath) || string.IsNullOrWhiteSpace(downloadModel.DownloadUrl)) throw new Exception("关键内容为空");
        DownloadResult? result;
        if (AutoRedownload)
        {
            while (true)
            {
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
                        continue;
                    }
                    else if (isStop)
                    {
                        DownloadStateAction?.Invoke(false, DownloadState.Stop, ex);
                        break;
                    }
                    else
                        DownloadStateAction?.Invoke(false, DownloadState.DownloadingError, ex);
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
        using FileStream fileStream = new(downloadModel.FileSavePath, FileMode.Create, FileAccess.Write, FileShare.None);
        // 每次写入最大缓存
        byte[] buffer = new byte[downloadModel.WirteSize];
        long sumCount = response.Content.Headers.GetHeaderValuesFirst<long>("Content-Length");
        int bytesRead;
        // 处理出现网络错误或者其他问题的时候如何可以继续下载，明天完成
        downloadModel.SumDownloadSize = sumCount;
        downloadResult.Size = sumCount;
        DownloadStateAction?.Invoke(false, DownloadState.Ready, null);
        DownloadStateAction?.Invoke(true, DownloadState.Start, null);
        DownloadStateAction?.Invoke(true, DownloadState.BeDownloading, null);
        while ((bytesRead = await contentStream.ReadAsync(buffer, CancellationToken)) > 0)
        {
            CancellationToken.ThrowIfCancellationRequested();
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), CancellationToken);
            downloadModel.YetDownloadSize += bytesRead;
            downloadModel.SaveHandle?.Invoke(downloadModel.YetDownloadSize, sumCount);
            // 检查取消标记是否被设置，如果被设置则抛出异常以中止下载
        }
        downloadResult.Success = true;
        downloadResult.Message = "完成";
        return downloadResult;
    }
    public void PauseDownload()
    {
        CancellationTokenSource.Cancel();
        isPause = true;
        DownloadStateAction?.Invoke(true, DownloadState.PauseDownloading, null);
    }

    public void ResumeDownload()
    {
        CancellationTokenSource = new();
        CancellationToken cancellationToken = CancellationTokenSource.Token;
        isPause = false;
        DownloadStateAction?.Invoke(true, DownloadState.ResumeDownloading, null);
    }

    //public void StartDownload()
    //{
    //    CancellationTokenSource = new CancellationTokenSource();
    //}

    public void StopDownload()
    {
        isStop = true;
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();

        DownloadStateAction?.Invoke(false, DownloadState.Stop, null);
    }
}
