using System.Diagnostics.CodeAnalysis;

namespace MFToolkit.Minecraft.Services.Downloads.Models;

/// <summary>
/// 下载进度
/// </summary>
[method: SetsRequiredMembers]
public class MinecraftDownloadProgress() : MinecraftDownloadTask
{
    /// <summary>
    /// 当前下载大小
    /// </summary>
    public long DownloadBytes { get; set; }

    /// <summary>
    /// 下载进度
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// 下载百分比文本
    /// </summary>
    public string? ProgressPercentage { get; set; } = "0%";

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
}
