using MFToolkit.Download.Models;

namespace MFToolkit.Download.DownloadServices;
public interface IDownloadService
{
    /// <summary>
    /// 是否启动自动重新下载，默认 自动重新下载
    /// <para>在例如非用户操作的情况下自动断开的下载继续尝试下载</para>
    /// </summary>
    bool AutoRedownload { get; set; }
    /// <summary>
    /// 下载进度
    /// </summary>
    Action<long, long>? DownloadProgress { get; set; }
    /// <summary>
    /// 启动下载
    /// </summary>
    /// <returns></returns>
    //void StartDownload();
    /// <summary>
    /// 暂停下载
    /// </summary>
    /// <returns></returns>
    Task PauseDownloadAsync();
    /// <summary>
    /// 继续下载
    /// </summary>
    /// <returns></returns>
    Task ResumeDownloadAsync();
    /// <summary>
    /// 停止下载（删除已下载的数据）
    /// </summary>
    /// <returns></returns>
    Task StopDownloadAsync();
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="downloadModel">下载模型</param>
    /// <param name="token">用于继续下载还是暂停的操作</param>
    /// <returns></returns>
    //Task DownloadAsync(DownloadModel downloadModel, CancellationToken token);
    Task<DownloadResult?> DownloadAsync(DownloadModel downloadModel);
    /// <summary>
    /// 下载状态
    /// <para>bool: 是否处于下载中</para>
    /// <para>DownloadState: 当前下载状态</para>
    /// <para>Exception: 当前下载是否出现了问题</para>
    /// </summary>
    Action<bool, DownloadState, Exception?>? DownloadStateAction { get; set; }
}
