using System.Runtime.InteropServices;
using MFToolkit.Minecraft.Entities.GameVersion;

namespace MFToolkit.Minecraft.Helpers;

/// <summary>
/// Minecraft 原生库检测工具类，用于判断 natives JAR 包是否符合当前系统
/// </summary>
public static class MinecraftNativeCheckerExtensions
{
    private static readonly string CurrentOS;
    private static readonly Architecture CurrentArch;

    static MinecraftNativeCheckerExtensions()
    {
        // 检测当前操作系统（Minecraft 常用标识）
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            CurrentOS = "windows";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            CurrentOS = "linux";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            CurrentOS = "osx"; // Minecraft 对 macOS 几乎只用 "osx" 标识
        else
            CurrentOS = "unknown";

        // 检测当前架构
        CurrentArch = RuntimeInformation.OSArchitecture;
    }

    /// <summary>
    /// 判断是否为当前系统可用的 Minecraft 原生库（natives JAR）
    /// </summary>
    public static bool IsCompatibleNative(this LibrarySimple? library)
    {
        if (library == null)
            return false;

        string name = library.Name?.ToLowerInvariant() ?? "";
        string path = library.Path?.ToLowerInvariant() ?? "";

        // 必须是 JAR 文件
        if (!path.EndsWith(".jar"))
            return false;

        // 必须包含 Minecraft 原生库特征（natives 标识）
        if (!IsMinecraftNative(name, path))
            return false;

        // 系统和架构必须完全匹配
        var (targetOs, targetArch) = ExtractTargetPlatform(name, path);
        return IsOsMatch(targetOs) && IsArchMatch(targetArch);
    }

    /// <summary>
    /// 批量过滤适用于当前系统的 native 库
    /// </summary>
    /// <param name="libraries">库文件集合</param>
    /// <returns>适用于当前系统的库集合</returns>
    public static List<LibrarySimple> FilterCompatibleNative(this IEnumerable<LibrarySimple>? libraries)
    {
        return libraries == null ? [] : libraries.Where(IsCompatibleNative).ToList();
    }
    
    /// <summary>
    /// 判断是否为 Minecraft 原生库（基于命名特征）
    /// </summary>
    private static bool IsMinecraftNative(string name, string path)
    {
        // Minecraft 原生库的核心标识：natives-<系统>（在名称或路径中）
        // 支持 Maven 坐标格式（如 group:artifact:version:natives-windows）
        string combined = $"{name} {path}";
        return combined.Contains("natives-") || 
               combined.Contains("-natives-") || 
               combined.Contains(":natives-");
    }

    /// <summary>
    /// 提取原生库的目标系统和架构（贴合 Minecraft 命名规范）
    /// </summary>
    private static (string Os, string Arch) ExtractTargetPlatform(string name, string path)
    {
        string combined = $"{name} {path}";
        return (ExtractTargetOs(combined), ExtractTargetArch(combined));
    }

    /// <summary>
    /// 提取目标系统（优先匹配 Minecraft 常用标识）
    /// </summary>
    private static string ExtractTargetOs(string combined)
    {
        // Windows 标识（Minecraft 格式：natives-windows 或 -windows-）
        if (combined.Contains("natives-windows") || combined.Contains("-windows-") || 
            combined.Contains(":natives-windows"))
            return "windows";

        // Linux 标识（Minecraft 格式：natives-linux 或 -linux-）
        if (combined.Contains("natives-linux") || combined.Contains("-linux-") || 
            combined.Contains(":natives-linux"))
            return "linux";

        // macOS 标识（Minecraft 几乎只用 osx，不用 macos）
        if (combined.Contains("natives-osx") || combined.Contains("-osx-") || 
            combined.Contains(":natives-osx"))
            return "osx";

        return "unknown";
    }

    /// <summary>
    /// 提取目标架构（优先匹配 Minecraft 常用格式）
    /// </summary>
    private static string ExtractTargetArch(string combined)
    {
        // Minecraft 常用 64位 x86 标识：x86_64（而非 x64）
        if (combined.Contains("x86_64") || combined.Contains("-x86_64-"))
            return "x86_64";

        // 32位 x86 标识：x86 或 i386/i686
        if (combined.Contains("x86") && !combined.Contains("x86_64") || 
            combined.Contains("i386") || combined.Contains("i686"))
            return "x86";

        // 64位 ARM 标识：arm64 或 aarch64（Minecraft 常见）
        if (combined.Contains("arm64") || combined.Contains("aarch64"))
            return "arm64";

        // 32位 ARM 标识：arm（较少见，主要用于旧设备）
        if (combined.Contains("arm") && !combined.Contains("arm64"))
            return "arm";

        // 无明确架构标识（部分旧库可能不带架构）
        return "unknown";
    }

    /// <summary>
    /// 系统是否匹配（严格匹配，Minecraft 不跨系统兼容）
    /// </summary>
    private static bool IsOsMatch(string targetOs)
    {
        return targetOs == CurrentOS;
    }

    /// <summary>
    /// 架构是否匹配（严格匹配，Minecraft 不跨架构兼容）
    /// </summary>
    private static bool IsArchMatch(string targetArch)
    {
        // 无明确架构标识的库（如旧版本通用库）默认视为兼容
        if (targetArch == "unknown")
            return true;

        // 映射当前架构到 Minecraft 常用标识
        string currentArchStr = CurrentArch switch
        {
            Architecture.X64 => "x86_64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown"
        };

        return targetArch == currentArchStr;
    }

    /// <summary>
    /// 获取当前系统信息（用于日志输出）
    /// </summary>
    public static string GetCurrentSystemInfo()
    {
        string archStr = CurrentArch switch
        {
            Architecture.X64 => "x86_64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => "unknown"
        };
        return $"Minecraft 环境检测：系统={CurrentOS}，架构={archStr}";
    }

    /// <summary>
    /// 提取原生库的目标平台信息（用于显示）
    /// </summary>
    public static string GetTargetPlatform(LibrarySimple library)
    {
        if (library == null)
            return "unknown";

        string name = library.Name?.ToLowerInvariant() ?? "";
        string path = library.Path?.ToLowerInvariant() ?? "";
        var (os, arch) = ExtractTargetPlatform(name, path);

        return arch == "unknown" ? os : $"{os}-{arch}";
    }
}
