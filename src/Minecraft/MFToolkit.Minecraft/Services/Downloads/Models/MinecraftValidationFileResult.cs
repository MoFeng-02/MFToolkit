namespace MFToolkit.Minecraft.Services.Downloads.Models;

/// <summary>
/// 表示验证文件的结果
/// </summary>
public class MinecraftValidationFileResult
{
    /// <summary>
    /// 验证是否通过
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 总大小
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// 实际验证大小
    /// </summary>
    public long ValidBytes { get; set; }

    /// <summary>
    /// 成功任务列表
    /// </summary>
    public List<MinecraftDownloadTask> SuccessTasks { get; set; } = [];

    /// <summary>
    /// 失败任务列表
    /// </summary>
    public List<MinecraftDownloadError> ErrorTasks { get; set; } = [];
}
