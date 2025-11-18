// MFToolkit.Minecraft.Services/Internals/JavaFinderBase.cs

using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MFToolkit.Minecraft.Entities.Java;
using MFToolkit.Minecraft.Enums.Java;
using MFToolkit.Minecraft.Services.Java.Interfaces;

namespace MFToolkit.Minecraft.Services.Java.Internals;

/// <summary>
/// Java查找器基类
/// 提供跨平台的通用实现
/// </summary>
public abstract partial class JavaFinderBase : IJavaFinder
{
    /// <summary>
    /// 常见的Java安装路径
    /// </summary>
    protected readonly List<string> CommonJavaPaths = new();

    /// <summary>
    /// Java进程名称列表
    /// </summary>
    protected readonly List<string> ProcessNames = new();

    /// <summary>
    /// 查找所有Java安装
    /// </summary>
    public abstract Task<List<JavaInstallation>> FindJavaInstallationsAsync();

    /// <summary>
    /// 从进程查找Java安装
    /// </summary>
    public abstract Task<List<JavaInstallation>> FindJavaFromProcessesAsync();

    /// <summary>
    /// 验证Java安装是否有效
    /// </summary>
    public virtual async Task<bool> ValidateJavaInstallationAsync(string javaPath)
    {
        if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            return false;

        try
        {
            var version = await GetJavaVersionAsync(javaPath);
            return !string.IsNullOrEmpty(version) && version.Contains("version", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException
                                       or Win32Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取Java版本信息
    /// AOT兼容：使用Process.Start而非反射
    /// </summary>
    public virtual async Task<string> GetJavaVersionAsync(string javaPath)
    {
        if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            return string.Empty;

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = javaPath,
                Arguments = "-version",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            // Java版本信息通常输出到标准错误流
            var output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output;
        }
        catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException
                                       or Win32Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 检测Java供应商
    /// </summary>
    protected virtual JavaVendor DetectVendor(string versionOutput)
    {
        if (string.IsNullOrEmpty(versionOutput))
            return JavaVendor.Unknown;

        var lowerOutput = versionOutput.ToLowerInvariant();

        if (lowerOutput.Contains("openjdk", StringComparison.OrdinalIgnoreCase))
            return JavaVendor.OpenJDK;
        if (lowerOutput.Contains("oracle", StringComparison.OrdinalIgnoreCase))
            return JavaVendor.Oracle;
        if (lowerOutput.Contains("microsoft", StringComparison.OrdinalIgnoreCase))
            return JavaVendor.Microsoft;
        if (lowerOutput.Contains("amazon", StringComparison.OrdinalIgnoreCase))
            return JavaVendor.Amazon;
        if (lowerOutput.Contains("eclipse", StringComparison.OrdinalIgnoreCase))
            return JavaVendor.Eclipse;
        if (lowerOutput.Contains("zulu", StringComparison.OrdinalIgnoreCase))
            return JavaVendor.Zulu;

        return JavaVendor.Unknown;
    }

    /// <summary>
    /// 在目录中搜索Java可执行文件
    /// </summary>
    protected virtual async Task<List<JavaInstallation>> SearchInDirectoriesAsync()
    {
        var installations = new List<JavaInstallation>();
        var foundPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var basePath in CommonJavaPaths)
        {
            if (!Directory.Exists(basePath))
                continue;

            try
            {
                var javaExecutables = FindJavaExecutablesInDirectory(basePath);
                foreach (var javaExe in javaExecutables)
                {
                    // 避免重复添加相同路径
                    if (foundPaths.Contains(javaExe))
                        continue;

                    if (await ValidateJavaInstallationAsync(javaExe))
                    {
                        var version = await GetJavaVersionAsync(javaExe);
                        foundPaths.Add(javaExe);
                        var (is64Bit, architecture) = await DetectArchitectureAsync(javaExe);
                        var majorVersion = ParseMajorVersion(version);

                        installations.Add(new JavaInstallation
                        {
                            Path = javaExe,
                            Version = version,
                            Vendor = DetectVendor(version),
                            Source = InstallationSource.CommonDirectories,
                            Is64Bit = is64Bit,
                            Architecture = architecture,
                            MajorVersion = majorVersion
                        });
                        installations.Add(new JavaInstallation
                        {
                            Path = javaExe,
                            Version = version,
                            Vendor = DetectVendor(version),
                            Source = InstallationSource.CommonDirectories
                        });
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 忽略无权限访问的目录
                continue;
            }
        }

        return installations;
    }

    /// <summary>
    /// 在目录中查找Java可执行文件
    /// </summary>
    protected virtual IEnumerable<string> FindJavaExecutablesInDirectory(string directory)
    {
        var executables = new List<string>();

        try
        {
            // 首先检查目录本身是否包含Java可执行文件
            foreach (var processName in ProcessNames)
            {
                var directPath = Path.Combine(directory, processName);
                if (File.Exists(directPath))
                {
                    executables.Add(directPath);
                }
            }

            // 查找bin目录
            var binDir = Path.Combine(directory, "bin");
            if (Directory.Exists(binDir))
            {
                foreach (var processName in ProcessNames)
                {
                    var binPath = Path.Combine(binDir, processName);
                    if (File.Exists(binPath))
                    {
                        executables.Add(binPath);
                    }
                }
            }

            // 递归查找子目录中的Java安装
            var subDirs = Directory.GetDirectories(directory);
            foreach (var subDir in subDirs)
            {
                var subBinDir = Path.Combine(subDir, "bin");
                if (Directory.Exists(subBinDir))
                {
                    foreach (var processName in ProcessNames)
                    {
                        var subBinPath = Path.Combine(subBinDir, processName);
                        if (File.Exists(subBinPath))
                        {
                            executables.Add(subBinPath);
                        }
                    }
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // 忽略无权限访问的目录
        }
        catch (DirectoryNotFoundException)
        {
            // 目录不存在，忽略
        }

        return executables;
    }

    /// <summary>
    /// 检测Java架构信息
    /// </summary>
    protected virtual async Task<(bool is64Bit, string architecture)> DetectArchitectureAsync(string javaPath)
    {
        if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            return (false, "Unknown");

        try
        {
            // 方法1: 通过文件头检测
            var fileArchitecture = DetectArchitectureFromFile(javaPath);
            if (!string.IsNullOrEmpty(fileArchitecture.architecture))
            {
                return fileArchitecture;
            }

            // 方法2: 通过Java命令检测
            return await DetectArchitectureFromJavaCommandAsync(javaPath);
        }
        catch
        {
            return (false, "Unknown");
        }
    }

    /// <summary>
    /// 通过文件头检测架构
    /// </summary>
    protected virtual (bool is64Bit, string architecture) DetectArchitectureFromFile(string javaPath)
    {
        try
        {
            // 读取文件前几个字节来检测架构
            using var fileStream = new FileStream(javaPath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            // PE文件头检测 (Windows)
            var dosHeader = reader.ReadUInt16();
            if (dosHeader == 0x5A4D) // "MZ"
            {
                reader.BaseStream.Seek(0x3C, SeekOrigin.Begin);
                var peHeaderOffset = reader.ReadInt32();
                reader.BaseStream.Seek(peHeaderOffset, SeekOrigin.Begin);
                var peSignature = reader.ReadUInt32();

                if (peSignature == 0x00004550) // "PE\0\0"
                {
                    var machineType = reader.ReadUInt16();

                    switch (machineType)
                    {
                        case 0x8664: // AMD64
                            return (true, "x64");
                        case 0x014C: // I386
                            return (false, "x86");
                        case 0xAA64: // ARM64
                            return (true, "arm64");
                    }
                }
            }

            // ELF文件头检测 (Linux/macOS)
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var elfHeader = reader.ReadUInt32();
            if (elfHeader == 0x464C457F) // "\x7FELF"
            {
                var elfClass = reader.ReadByte(); // 1 = 32-bit, 2 = 64-bit
                var elfData = reader.ReadByte(); // 1 = LE, 2 = BE
                reader.BaseStream.Seek(0x12, SeekOrigin.Begin);
                var elfMachine = reader.ReadUInt16();

                var is64Bit = elfClass == 2;
                var architecture = elfMachine switch
                {
                    0x03 => "x86",
                    0x3E => "x64",
                    0xB7 => "arm64",
                    _ => "unknown"
                };

                return (is64Bit, architecture);
            }
        }
        catch
        {
            // 忽略文件读取错误
        }

        return (false, "Unknown");
    }

    /// <summary>
    /// 通过Java命令检测架构
    /// </summary>
    protected virtual async Task<(bool is64Bit, string architecture)> DetectArchitectureFromJavaCommandAsync(
        string javaPath)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = javaPath,
                Arguments = "-XshowSettings:properties -version",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            var output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // 解析输出中的架构信息
            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("os.arch", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = line.Split('=');
                    if (parts.Length >= 2)
                    {
                        var arch = parts[1].Trim().ToLowerInvariant();
                        var is64Bit = arch.Contains("64") ||
                                      arch == "x64" ||
                                      arch == "amd64" ||
                                      arch == "arm64";

                        string architecture = arch switch
                        {
                            "x86" or "i386" or "i686" => "x86",
                            "x64" or "amd64" or "x86_64" => "x64",
                            "aarch64" or "arm64" => "arm64",
                            _ => arch
                        };

                        return (is64Bit, architecture);
                    }
                }
            }
        }
        catch
        {
            // 忽略进程执行错误
        }

        return (false, "Unknown");
    }

    /// <summary>
    /// 解析Java主版本号
    /// </summary>
    protected virtual int ParseMajorVersion(string versionOutput)
    {
        if (string.IsNullOrEmpty(versionOutput))
            return 0;

        try
        {
            var lines = versionOutput.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("version", StringComparison.OrdinalIgnoreCase))
                {
                    // 查找版本号模式
                    var versionMatch = VersionMatchRegex().Match(line);

                    if (versionMatch.Success && int.TryParse(versionMatch.Groups[1].Value, out int majorVersion))
                    {
                        // Java 9+ 使用单个主版本号，Java 8及以下使用1.x格式
                        if (majorVersion == 1 && versionMatch.Groups.Count > 2 &&
                            int.TryParse(versionMatch.Groups[2].Value, out int minorVersion))
                        {
                            return minorVersion; // Java 8返回8，Java 7返回7等
                        }

                        return majorVersion; // Java 9+ 直接返回版本号
                    }
                }
            }
        }
        catch
        {
            // 忽略解析错误
        }

        return 0;
    }

    [GeneratedRegex("""["']?(\d+)(?:\.(\d+))?(?:\.(\d+))?["']?""")]
    private static partial Regex VersionMatchRegex();
}