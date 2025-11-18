using System.Runtime.InteropServices;

namespace MFToolkit.Minecraft.Services.Java.Internals;

/// <summary>
/// 平台检测辅助类
/// AOT兼容，使用RuntimeInformation而非条件属性
/// </summary>
internal static class PlatformHelper
{
    /// <summary>
    /// 当前操作系统平台
    /// </summary>
    public static OSPlatform CurrentPlatform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return OSPlatform.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return OSPlatform.OSX;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return OSPlatform.Linux;
                
            // 默认为未知平台，尝试根据RuntimeIdentifier判断
            var runtimeId = RuntimeInformation.RuntimeIdentifier.ToLowerInvariant();
            if (runtimeId.Contains("win"))
                return OSPlatform.Windows;
            if (runtimeId.Contains("osx") || runtimeId.Contains("macos"))
                return OSPlatform.OSX;
            if (runtimeId.Contains("linux"))
                return OSPlatform.Linux;
                
            return OSPlatform.FreeBSD; // 作为未知平台的替代
        }
    }
        
    /// <summary>
    /// 是否为Windows系统
    /// </summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
    /// <summary>
    /// 是否为macOS系统
    /// </summary>
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        
    /// <summary>
    /// 是否为Linux系统
    /// </summary>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        
    /// <summary>
    /// 当前进程架构
    /// </summary>
    public static Architecture ProcessArchitecture => RuntimeInformation.ProcessArchitecture;
}