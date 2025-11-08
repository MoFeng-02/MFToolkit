using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Services.Downloads.Events;
/// <summary>
/// 下载进度事件参数
/// </summary>
public class DownloadProgressEventArgs : EventArgs
{
    /// <summary>
    /// 已下载字节数
    /// </summary>
    public long DownloadedBytes { get; set; }

    /// <summary>
    /// 总字节数
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// 下载进度（0-100）
    /// </summary>
    public double Progress => TotalBytes > 0 ? (DownloadedBytes * 100.0 / TotalBytes) : 0;

    /// <summary>
    /// 下载速度（字节/秒）
    /// </summary>
    public double DownloadSpeed { get; set; }

    public DownloadProgressEventArgs(long downloadedBytes, long totalBytes, double downloadSpeed)
    {
        DownloadedBytes = downloadedBytes;
        TotalBytes = totalBytes;
        DownloadSpeed = downloadSpeed;
    }
}

/// <summary>
/// 下载状态事件参数
/// </summary>
public class DownloadStatusEventArgs : EventArgs
{
    /// <summary>
    /// 新的下载状态
    /// </summary>
    public DownloadStatus NewStatus { get; set; }

    /// <summary>
    /// 之前的下载状态
    /// </summary>
    public DownloadStatus PreviousStatus { get; set; }

    /// <summary>
    /// 错误信息（如果状态变为Failed）
    /// </summary>
    public string? ErrorMessage { get; set; }

    public DownloadStatusEventArgs(DownloadStatus newStatus, DownloadStatus previousStatus, string? errorMessage = null)
    {
        NewStatus = newStatus;
        PreviousStatus = previousStatus;
        ErrorMessage = errorMessage;
    }
}