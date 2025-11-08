namespace MFToolkit.Minecraft.Enums;
/// <summary>
/// 下载状态枚举
/// </summary>
public enum DownloadStatus
{
    /// <summary>
    /// 准备
    /// </summary>
    Pending,

    /// <summary>
    /// 下载中
    /// </summary>
    Downloading,

    /// <summary>
    /// 完成
    /// </summary>
    Completed,

    /// <summary>
    /// 暂停
    /// </summary>
    Pause,

    /// <summary>
    /// 失败
    /// </summary>
    Failed,

    /// <summary>
    /// 正在验证文件
    /// </summary>
    Verifying,

    /// <summary>
    /// 等待依赖项
    /// </summary>
    WaitingForDependencies,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled
}