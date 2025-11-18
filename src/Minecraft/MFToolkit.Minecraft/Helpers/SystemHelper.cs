using System.Globalization;
using System.Runtime.InteropServices;

namespace MFToolkit.Minecraft.Helpers;

/// <summary>
/// 系统帮助类
/// </summary>
public class SystemHelper
{
    /// <summary>
    /// 获取当前系统平台名称
    /// </summary>
    /// <returns>系统平台名称</returns>
    public static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows())
            return "Windows";
        else if (OperatingSystem.IsLinux())
            return "Linux";
        else if (OperatingSystem.IsMacOS())
            return "macOS";
        else if (OperatingSystem.IsAndroid())
            return "Android";
        else if (OperatingSystem.IsIOS())
            return "iOS";
        else
            return "Unknown";
    }

    public static string GetPlatformDisplayName() => GetPlatformName().ToLowerInvariant();

    /// <summary>
    /// 获取系统架构信息
    /// </summary>
    /// <returns>系统架构</returns>
    public static string GetSystemArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture.ToString();
    }

    /// <summary>
    /// 获取可用内存大小（MB）
    /// </summary>
    /// <returns>可用内存大小（MB）</returns>
    public static long GetAvailableMemory()
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            return gcMemoryInfo.TotalAvailableMemoryBytes / 1024 / 1024;
        }
        
        return -1; // 不支持的系统返回-1
    }

    /// <summary>
    /// 检查是否有足够的内存运行Minecraft
    /// </summary>
    /// <param name="requiredMemoryMB">所需内存大小（MB）</param>
    /// <returns>是否足够</returns>
    public static bool HasSufficientMemory(int requiredMemoryMB = 2048)
    {
        var availableMemory = GetAvailableMemory();
        return availableMemory == -1 || availableMemory >= requiredMemoryMB;
    }

    /// <summary>
    /// 获取Java安装路径
    /// </summary>
    /// <returns>Java安装路径，如果未找到返回null</returns>
    public static string? GetJavaPath()
    {
        if (OperatingSystem.IsWindows())
        {
            // Windows系统查找Java
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java.exe");
                if (File.Exists(javaExe))
                    return javaExe;
            }

            // 尝试在PATH中查找
            return FindExecutableInPath("java.exe");
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // Linux/macOS系统查找Java
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java");
                if (File.Exists(javaExe))
                    return javaExe;
            }

            return FindExecutableInPath("java");
        }

        return null;
    }

    /// <summary>
    /// 在系统PATH中查找可执行文件
    /// </summary>
    /// <param name="executableName">可执行文件名</param>
    /// <returns>完整路径，如果未找到返回null</returns>
    private static string? FindExecutableInPath(string executableName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
            return null;

        var paths = path.Split(Path.PathSeparator);
        foreach (var pathDir in paths)
        {
            var fullPath = Path.Combine(pathDir, executableName);
            if (File.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    /// <summary>
    /// 检查是否安装了指定版本以上的Java
    /// </summary>
    /// <param name="minVersion">最低版本要求</param>
    /// <returns>是否满足要求</returns>
    public static bool IsJavaVersionInstalled(Version minVersion)
    {
        var javaPath = GetJavaPath();
        if (string.IsNullOrEmpty(javaPath))
            return false;

        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = javaPath;
            process.StartInfo.Arguments = "-version";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            var output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // 解析Java版本输出
            var versionMatch = System.Text.RegularExpressions.Regex.Match(output, @"version\s+\""(\d+\.\d+\.\d+)");
            if (versionMatch.Success && versionMatch.Groups.Count > 1)
            {
                var javaVersion = new Version(versionMatch.Groups[1].Value);
                return javaVersion >= minVersion;
            }
        }
        catch
        {
            // 忽略所有异常
        }

        return false;
    }

    /// <summary>
    /// 获取系统临时目录路径
    /// </summary>
    /// <returns>临时目录路径</returns>
    public static string GetTempDirectory()
    {
        return Path.GetTempPath();
    }

    /// <summary>
    /// 获取Minecraft默认游戏目录
    /// </summary>
    /// <returns>游戏目录路径</returns>
    public static string GetDefaultMinecraftDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
        }
        else if (OperatingSystem.IsLinux())
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft");
        }
        else if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "minecraft");
        }
        else
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft");
        }
    }

    /// <summary>
    /// 检查磁盘空间是否足够
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="requiredSpaceMB">所需空间大小（MB）</param>
    /// <returns>是否足够</returns>
    public static bool HasSufficientDiskSpace(string directoryPath, long requiredSpaceMB)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(directoryPath) ?? directoryPath);
            var availableSpaceMB = driveInfo.AvailableFreeSpace / 1024 / 1024;
            return availableSpaceMB >= requiredSpaceMB;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取系统语言代码
    /// </summary>
    /// <returns>语言代码</returns>
    public static string GetSystemLanguage()
    {
        return CultureInfo.CurrentCulture.Name;
    }

    /// <summary>
    /// 检查是否为64位操作系统
    /// </summary>
    /// <returns>是否为64位</returns>
    public static bool Is64BitOperatingSystem()
    {
        return Environment.Is64BitOperatingSystem;
    }

    /// <summary>
    /// 获取系统用户名
    /// </summary>
    /// <returns>用户名</returns>
    public static string GetSystemUsername()
    {
        return Environment.UserName;
    }
}