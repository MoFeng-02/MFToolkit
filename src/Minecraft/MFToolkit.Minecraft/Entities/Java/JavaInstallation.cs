using MFToolkit.Minecraft.Enums.Java;

namespace MFToolkit.Minecraft.Entities.Java;

/// <summary>
/// Java安装信息
/// </summary>
public class JavaInstallation
{
    /// <summary>
    /// Java可执行文件完整路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Java版本信息
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 是否为64位版本
    /// </summary>
    public bool Is64Bit { get; set; }

    /// <summary>
    /// Java供应商
    /// </summary>
    public JavaVendor Vendor { get; set; }

    /// <summary>
    /// 架构信息 (x86, x64, arm64, etc.)
    /// </summary>
    public string Architecture { get; set; } = "Unknown";

    /// <summary>
    /// 安装来源
    /// </summary>
    public InstallationSource Source { get; set; }

    /// <summary>
    /// Java主版本号 (8, 11, 17, 21, etc.)
    /// </summary>
    public int MajorVersion { get; set; }
}