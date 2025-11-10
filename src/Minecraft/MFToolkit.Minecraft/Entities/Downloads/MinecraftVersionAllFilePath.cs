using System.Diagnostics.CodeAnalysis;

namespace MFToolkit.Minecraft.Entities.Downloads;

[method: SetsRequiredMembers]
public class MinecraftVersionAllFilePath() : MinecraftDownloadTask
{

    /// <summary>
    /// 是否已经下载完成
    /// </summary>
    public bool IsDownloadSuccess { get; set; }
    
    /// <summary>
    /// 本机文件名称
    /// <remarks>
    /// windows、osx（MacOs）、Linux等等
    /// </remarks>
    /// </summary>
    public string? NativesName { get; set; }

    /// <summary>
    /// 几位系统/Arm系统
    /// <remarks>
    /// x64,x86,x86_64,arm,arm64
    /// </remarks>
    /// </summary>
    public string? BitType { get; set; }

    /// <summary>
    /// 是否需要解压 natives 文件
    /// </summary>
    public bool RequiresExtraction { get; set; }

    /// <summary>
    /// 需要排除的解压路径
    /// </summary>
    public List<string>? ExcludePatterns { get; set; }
}