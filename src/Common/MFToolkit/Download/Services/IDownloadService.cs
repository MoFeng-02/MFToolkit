using MFToolkit.Download.Models;

namespace MFToolkit.Download.Services;

/// <summary>
/// 下载服务接口
/// </summary>
public interface IDownloadService
{
    /// <summary>
    /// 是否启动自动重新下载，默认 自动重新下载，但是最大只能尝试五次，每次等待5秒，目前是如此限制，暂时不开放手动重试逻辑，因为有一些处理还未完善，代码有待优化
    /// <para>在例如非用户操作的情况下自动断开的下载继续尝试下载</para>
    /// </summary>
    bool AutoRedownload { get; set; }
    /// <summary>
    /// 下载进度
    /// <para>0: 当前下载大小，1: 总大小</para>
    /// </summary>
    Action<long, long>? DownloadProgress { get; set; }
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
    /// 开始下载
    /// </summary>
    /// <param name="downloadModel">下载模型</param>
    /// <returns></returns>
    Task StartDownloadAsync(DownloadModel? downloadModel = null);
    /// <summary>
    /// 停止下载（删除已下载的数据）
    /// </summary>
    /// <returns></returns>
    Task StopDownloadAsync();
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="downloadModel">下载模型</param>
    /// <returns></returns>
    Task<DownloadResult?> DownloadAsync(DownloadModel downloadModel);

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="savePath">保存位置</param>
    /// <returns></returns>
    Task<DownloadResult?> DownloadAsync(string url, string savePath);
    /// <summary>
    /// 下载状态
    /// <para>bool: 是否处于下载中</para>
    /// <para>DownloadState: 当前下载状态</para>
    /// <para>Exception: 当前下载是否出现了问题</para>
    /// </summary>
    Action<bool, DownloadState, Exception?>? DownloadStateAction { get; set; }
}
