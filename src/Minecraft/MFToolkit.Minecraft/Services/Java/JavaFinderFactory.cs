using MFToolkit.Minecraft.Services.Java.Interfaces;

namespace MFToolkit.Minecraft.Services.Java;

/// <summary>
/// Java查找器工厂
/// 使用SupportedOSPlatform属性进行平台兼容性检查
/// </summary>
public static class JavaFinderFactory
{
    /// <summary>
    /// 创建适合当前平台的Java查找器
    /// </summary>
    /// <returns>平台特定的Java查找器实例</returns>
    /// <exception cref="PlatformNotSupportedException">当前平台不支持时抛出</exception>
    public static IJavaFinder CreateJavaFinder()
    {
        if (Internals.PlatformHelper.IsWindows)
        {
            return new Internals.WindowsJavaFinder();
        }

        if (Internals.PlatformHelper.IsMacOS)
        {
            return new Internals.MacOSJavaFinder();
        }

        if (Internals.PlatformHelper.IsLinux)
        {
            return new Internals.LinuxJavaFinder();
        }

        throw new PlatformNotSupportedException($"Unsupported operating system: {Internals.PlatformHelper.CurrentPlatform}");
    }

    /// <summary>
    /// 检查当前平台是否支持Java查找
    /// </summary>
    /// <returns>是否支持</returns>
    public static bool IsPlatformSupported()
    {
        return Internals.PlatformHelper.IsWindows || 
               Internals.PlatformHelper.IsMacOS || 
               Internals.PlatformHelper.IsLinux;
    }
}