namespace MFToolkit.Minecraft.Services.Downloads.Models;

/// <summary>
/// 下载完成返回
/// </summary>
public class MinecraftDownloadCompletedResult
{
    /// <summary>
    /// 总大小
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// 实际下载大小
    /// </summary>
    public long DownloadedBytes { get; set; }

    /// <summary>
    /// 成功任务列表
    /// </summary>
    public List<MinecraftDownloadTask> SuccessTasks { get; set; } = [];

    /// <summary>
    /// 失败任务列表
    /// </summary>
    public List<MinecraftDownloadError> ErrorTasks { get; set; } = [];
}
