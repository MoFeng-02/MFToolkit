using MFToolkit.App;
using MFToolkit.Download.DownloadHandlers;
using MFToolkit.Download.Models;
using MFToolkit.Extensions;
using MFToolkit.Http;

namespace MFToolkit.Download.DownloadServices;
public class DownloadService : IDownloadService, IDisposable
{
    /// <summary>
    /// 下载Token
    /// </summary>
    protected virtual CancellationTokenSource CancellationTokenSource { get; set; } = new();
    protected virtual CancellationToken CancellationToken { get; set; }
    protected virtual DownloadModel currentDownloadModel { get; set; } = null!;
    /// <summary>
    /// 关于本字段，如遇到未注册，请注册，参考：
    /// <para>
    /// MFToolkit/Injects/GlobalInjects.cs下的AddInjectServices方法
    /// </para>
    /// </summary>
    protected readonly HttpClientService httpClient;
    //private readonly DownloadHandler? downloadHandler;
    protected virtual DownloadPauseInfoHandler? downloadPauseInfoHandler { get; set; }
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

    public DownloadService(HttpClientService httpClientService)
    {
        httpClient = httpClientService;
        CancellationToken = CancellationTokenSource.Token;
        //downloadHandler = AppUtil.GetService<DownloadHandler>();
        downloadPauseInfoHandler = AppUtil.GetService<DownloadPauseInfoHandler>();
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
        currentDownloadModel = downloadModel;
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
                    result = await DownloadActionAsync();
                }
                catch (Exception ex)
                {
                    result = null;
                    if (isPause)
                    {
                        DownloadStateAction?.Invoke(false, DownloadState.PauseDownloading, ex);

                        if (downloadPauseInfoHandler == null)
                        {
                            DownloadStateAction?.Invoke(false, DownloadState.OtherError, new("请注册：AddDownloadPauseInfoHandler，服务以确保正常使用暂停信息处理"));
                            break;
                        }
                        await downloadPauseInfoHandler.SavePauseInfoAsync(downloadModel);
                        break;
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
                result = await DownloadActionAsync();
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
    private async Task<DownloadResult> DownloadActionAsync()
    {
        DownloadStateAction?.Invoke(false, DownloadState.Init, null);
        // 更新偏移量
        httpClient.DefaultRequestHeaders.Range = new System.Net.Http.Headers.RangeHeaderValue(currentDownloadModel.YetDownloadSize, null);
        // 读取请求报头
        using var response = await httpClient.GetAsync(currentDownloadModel.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, CancellationToken);
        DownloadResult downloadResult = new();
        if (!response.IsSuccessStatusCode)
        {
            downloadResult.Message = await response.Content.ReadAsStringAsync();
            return downloadResult;
        }
        using Stream contentStream = await response.Content.ReadAsStreamAsync();
        using FileStream fileStream = currentDownloadModel.YetDownloadSize == 0 ? new(currentDownloadModel.FileSavePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None) : new(currentDownloadModel.FileSavePath, FileMode.Append, FileAccess.Write, FileShare.None);
        // 每次写入最大缓存
        byte[] buffer = new byte[currentDownloadModel.WirteSize];
        long sumCount = response.Content.Headers.GetHeaderValuesFirst<long>("Content-Length");
        int bytesRead;
        currentDownloadModel.SumDownloadSize ??= sumCount;
        downloadResult.Size = sumCount;
        DownloadStateAction?.Invoke(false, DownloadState.Ready, null);
        DownloadStateAction?.Invoke(true, DownloadState.Start, null);
        DownloadStateAction?.Invoke(true, DownloadState.BeDownloading, null);
        while ((bytesRead = await contentStream.ReadAsync(buffer, CancellationToken)) > 0)
        {
            CancellationToken.ThrowIfCancellationRequested();
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), CancellationToken);
            currentDownloadModel.YetDownloadSize += bytesRead;
            DownloadProgress?.Invoke(currentDownloadModel.YetDownloadSize, currentDownloadModel.SumDownloadSize ?? sumCount);
            // 检查取消标记是否被设置，如果被设置则抛出异常以中止下载
        }
        downloadResult.Success = true;
        downloadResult.Message = "完成";
        DownloadStateAction?.Invoke(false, DownloadState.CompleteDownloading, null);
        return downloadResult;
    }
    public async Task PauseDownloadAsync()
    {
        CancellationTokenSource.Cancel();
        isPause = true;
        DownloadStateAction?.Invoke(true, DownloadState.PauseDownloading, null);
        if (downloadPauseInfoHandler == null)
        {
            DownloadStateAction?.Invoke(false, DownloadState.OtherError, new("请注册：AddDownloadPauseInfoHandler，服务以确保正常使用暂停信息处理"));
            return;
        }
        await Task.CompletedTask;
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
        // 考虑取消后是否要删除已经下载的部分内容
        // ...
        await Task.CompletedTask;
    }
}
