using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Entities.Downloads;

/// <summary>
/// 下载完成返回
/// </summary>
public class MinecraftDownloadCompletedResult
{
    /// <summary>
    /// 版本ID
    /// </summary>
    public required string VersionId { get; set; }
    // /// <summary>
    // /// 总大小
    // /// </summary>
    // public long TotalBytes { get; set; }
    //
    // /// <summary>
    // /// 实际下载大小
    // /// </summary>
    // public long DownloadedBytes { get; set; }

    public List<MinecraftDownloadProgress> TotalTasks { get; } = [];

    /// <summary>
    /// 成功任务列表
    /// </summary>
    public IReadOnlyList<MinecraftDownloadProgress> SuccessTasks =>
        TotalTasks.Where(q => q.DownloadStatus == DownloadStatus.Completed).ToList();

    /// <summary>
    /// 失败任务列表
    /// </summary>
    public IReadOnlyList<MinecraftDownloadProgress> FailedTasks =>
        TotalTasks.Where(q => q.DownloadStatus == DownloadStatus.Failed).ToList();

    /// <summary>
    /// 取消的任务列表
    /// </summary>
    public IReadOnlyList<MinecraftDownloadProgress> CancelledTasks =>
        TotalTasks.Where(q => q.DownloadStatus == DownloadStatus.Cancelled).ToList();
}