using System.Diagnostics.CodeAnalysis;

namespace MFToolkit.Minecraft.Entities.Downloads;
/// <summary>
/// 表示Minecraft相关文件的下载错误信息
/// </summary>
[method:SetsRequiredMembers]
public class MinecraftDownloadError() : MinecraftDownloadTask
{
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 错误异常
    /// </summary>
    public Exception? Ex { get; set; }
}
