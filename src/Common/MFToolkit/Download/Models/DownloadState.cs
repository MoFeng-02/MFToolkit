namespace MFToolkit.Download.Models;
/// <summary>
/// 下载状态
/// </summary>
public enum DownloadState
{
    /// <summary>
    /// 初始化
    /// </summary>
    Init,
    /// <summary>
    /// 准备就绪
    /// </summary>
    Ready,
    /// <summary>
    /// 启动下载
    /// </summary>
    Start,
    /// <summary>
    /// 取消下载（结束）
    /// </summary>
    Stop,
    /// <summary>
    /// 重试中
    /// </summary>
    RetryDownloading,
    /// <summary>
    /// 下载中
    /// </summary>
    BeDownloading,
    /// <summary>
    /// 暂停下载
    /// </summary>
    PauseDownloading,
    /// <summary>
    /// 继续下载
    /// </summary>
    ResumeDownloading,
    /// <summary>
    /// 完成下载
    /// </summary>
    CompleteDownloading,
    /// <summary>
    /// 下载错误
    /// </summary>
    DownloadingError,
    /// <summary>
    /// 网络错误
    /// </summary>
    NetworkError,
    /// <summary>
    /// 文件被锁定
    /// </summary>
    FileLocked,
    /// <summary>
    /// 访问被拒绝
    /// </summary>
    AccessDenied,
    /// <summary>
    /// 其他错误
    /// </summary>
    OtherError
}
