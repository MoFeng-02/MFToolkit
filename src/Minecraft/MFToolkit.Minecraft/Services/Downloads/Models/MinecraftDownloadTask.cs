using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Services.Downloads.Models;

/// <summary>
/// Minecraft下载任务
/// </summary>
public class MinecraftDownloadTask
{
    /// <summary>
    /// 下载任务ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// 版本所属ID
    /// </summary>
    public required string VersionId { get; set; }

    /// <summary>
    /// 当前文件大小
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 当前文件sha1值
    /// </summary>
    public string Sha1 { get; set; } = string.Empty;

    /// <summary>
    /// 源下载Url
    /// </summary>
    public required string OriginUrl { get; set; } = string.Empty;

    /// <summary>
    /// 实际下载Url（即可能是镜像源下载所以可能替换）
    /// </summary>
    public required string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// 文件名称
    /// </summary>
    public required string Name { get; set; } = string.Empty;

    /// <summary>
    /// 本地保存路径
    /// </summary>
    public required string SavePath { get; set; } = string.Empty;
    /// <summary>
    /// 所属文件类型
    /// </summary>
    public MinecraftFileType MinecraftFileType { get; set; }

    /// <summary>
    /// 下载状态
    /// </summary>
    public DownloadStatus DownloadStatus { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public DownloadPriority Priority { get; set; } = DownloadPriority.Normal;
    /// <summary>
    /// 已重试次数
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// 最大重试次数（实际按DownloadOptions里面的来提供）
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.Now;
    /// <summary>
    /// 最新更新时间
    /// </summary>
    public DateTime? LastUpdatedTime { get; set; }
    /// <summary>
    /// 实际保存地址
    /// </summary>
    public string? LocalFilePath { get; set; } // 实际保存的本地路径
    /// <summary>
    /// 验证是否通过
    /// </summary>
    public bool IsVerified { get; set; } = false; // 文件校验状态
}
