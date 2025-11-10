using System.Diagnostics.CodeAnalysis;
using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Entities.Downloads;

/// <summary>
/// 下载进度
/// </summary>
[method: SetsRequiredMembers]
public class MinecraftDownloadProgress() : MinecraftDownloadTask
{
    private long _downloadBytes;
    private double _progress;
    private string? _progressPercentage = "0%";
    private string? _error;

    /// <summary>
    /// 当前下载大小
    /// </summary>
    public long DownloadBytes
    {
        get => _downloadBytes;
        set => SetField(ref _downloadBytes, value);
    }

    /// <summary>
    /// 下载进度
    /// </summary>
    public double Progress
    {
        get => _progress;
        set => SetField(ref _progress, value);
    }

    /// <summary>
    /// 下载百分比文本
    /// </summary>
    public string? ProgressPercentage
    {
        get => _progressPercentage;
        set => SetField(ref _progressPercentage, value);
    }

    /// <summary>
    /// 下载错误后的提示信息（当状态为Failed时有效）
    /// </summary>
    public string? Error
    {
        get => _error;
        set => SetField(ref _error, value);
    }

    /// <summary>
    /// 计算下载时间
    /// </summary>
    public TimeSpan DownloadTime => Progress > 0
        ? TimeSpan.FromSeconds(DownloadBytes / (Progress / 100.0 * Size))
        : TimeSpan.Zero;

    /// <summary>
    /// 计算剩余时间
    /// </summary>
    public TimeSpan RemainingTime => Progress > 0
        ? TimeSpan.FromSeconds((Size - DownloadBytes) / (Progress / 100.0 * Size))
        : TimeSpan.Zero;
    
    
    /// <summary>
    /// 线程安全的状态更新
    /// <param name="status">新状态</param>
    /// <param name="error">可选错误信息</param>
    /// </summary>
    public void UpdateStatus(DownloadStatus status, string? error = null)
    {
        lock (this)
        {
            DownloadStatus = status;
            Error = error;
        }
    }
    
    /// <summary>
    /// 线程安全的进度更新
    /// <param name="progress">新进度值（0-100）</param>
    /// </summary>
    public void UpdateProgress(double progress)
    {
        lock (this)
        {
            Progress = progress;
        }
    }
}