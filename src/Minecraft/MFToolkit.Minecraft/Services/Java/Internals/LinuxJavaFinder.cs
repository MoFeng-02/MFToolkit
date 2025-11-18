using System.Diagnostics;
using System.Runtime.Versioning;
using MFToolkit.Minecraft.Entities.Java;
using MFToolkit.Minecraft.Enums.Java;

namespace MFToolkit.Minecraft.Services.Java.Internals;

/// <summary>
/// Linux平台Java查找器
/// </summary>
[SupportedOSPlatform("linux")]
public class LinuxJavaFinder : JavaFinderBase
{
    /// <summary>
    /// 初始化Linux特定的Java路径
    /// </summary>
    public LinuxJavaFinder()
    {
        var home = Environment.GetEnvironmentVariable("HOME") ?? "/home";

        CommonJavaPaths.AddRange([
            "/usr/lib/jvm",
            "/usr/lib64/jvm",
            "/usr/java",
            "/opt/java",
            "/opt/jdk",
            "/opt/jre",
            $"{home}/.sdkman/candidates/java",
            "/snap",
            "/var/lib"
        ]);

        ProcessNames.AddRange(new[] { "java" });
    }

    /// <summary>
    /// 查找Linux系统中的所有Java安装
    /// </summary>
    public override async Task<List<JavaInstallation>> FindJavaInstallationsAsync()
    {
        var installations = new List<JavaInstallation>();

        // 1. 从常见目录查找
        installations.AddRange(await SearchInDirectoriesAsync());

        // 2. 从环境变量查找
        installations.AddRange(await FindJavaFromEnvironmentAsync());

        // 3. 使用 which/whereis 命令查找
        installations.AddRange(await FindJavaWithCommandsAsync());

        // 4. 从进程查找
        installations.AddRange(await FindJavaFromProcessesAsync());

        // 5. 使用 update-alternatives 查找
        installations.AddRange(await FindJavaWithAlternativesAsync());

        return installations
            .GroupBy(x => x.Path)
            .Select(g => g.First())
            .ToList();
    }

    /// <summary>
    /// 从进程查找Java安装
    /// </summary>
    public override async Task<List<JavaInstallation>> FindJavaFromProcessesAsync()
    {
        var installations = new List<JavaInstallation>();

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"ps aux | grep java | grep -v grep | awk '{print $11}'\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var paths = output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(p => File.Exists(p) && ProcessNames.Contains(Path.GetFileName(p)))
                .Distinct();

            foreach (var path in paths)
            {
                if (await ValidateJavaInstallationAsync(path))
                {
                    var version = await GetJavaVersionAsync(path);
                    installations.Add(new JavaInstallation
                    {
                        Path = path,
                        Version = version,
                        Vendor = DetectVendor(version),
                        Source = InstallationSource.ProcessSearch
                    });
                }
            }
        }
        catch
        {
            // 忽略错误
        }

        return installations;
    }

    /// <summary>
    /// 从环境变量查找Java安装
    /// </summary>
    private async Task<List<JavaInstallation>> FindJavaFromEnvironmentAsync()
    {
        var installations = new List<JavaInstallation>();

        // 从 PATH 环境变量查找
        var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? Array.Empty<string>();
        foreach (var pathDir in pathDirs)
        {
            if (Directory.Exists(pathDir))
            {
                foreach (var processName in ProcessNames)
                {
                    var javaPath = Path.Combine(pathDir, processName);
                    if (File.Exists(javaPath) && await ValidateJavaInstallationAsync(javaPath))
                    {
                        var version = await GetJavaVersionAsync(javaPath);
                        installations.Add(new JavaInstallation
                        {
                            Path = javaPath,
                            Version = version,
                            Vendor = DetectVendor(version),
                            Source = InstallationSource.PathEnvironment
                        });
                    }
                }
            }
        }

        return installations;
    }

    /// <summary>
    /// 使用系统命令查找Java
    /// </summary>
    private async Task<List<JavaInstallation>> FindJavaWithCommandsAsync()
    {
        var installations = new List<JavaInstallation>();

        // 使用 which 命令
        try
        {
            using var whichProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/which",
                    Arguments = "java",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            whichProcess.Start();
            var whichPath = (await whichProcess.StandardOutput.ReadToEndAsync()).Trim();
            await whichProcess.WaitForExitAsync();

            if (File.Exists(whichPath) && await ValidateJavaInstallationAsync(whichPath))
            {
                var version = await GetJavaVersionAsync(whichPath);
                installations.Add(new JavaInstallation
                {
                    Path = whichPath,
                    Version = version,
                    Vendor = DetectVendor(version),
                    Source = InstallationSource.PathEnvironment
                });
            }
        }
        catch
        {
            // 忽略错误
        }

        // 使用 whereis 命令
        try
        {
            using var whereisProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/whereis",
                    Arguments = "-b java",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            whereisProcess.Start();
            var whereisOutput = await whereisProcess.StandardOutput.ReadToEndAsync();
            await whereisProcess.WaitForExitAsync();

            var paths = whereisOutput.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1) // 跳过 "java:" 
                .Where(File.Exists);

            foreach (var path in paths)
            {
                if (await ValidateJavaInstallationAsync(path))
                {
                    var version = await GetJavaVersionAsync(path);
                    installations.Add(new JavaInstallation
                    {
                        Path = path,
                        Version = version,
                        Vendor = DetectVendor(version),
                        Source = InstallationSource.PathEnvironment
                    });
                }
            }
        }
        catch
        {
            // 忽略错误
        }

        return installations;
    }

    /// <summary>
    /// 使用 update-alternatives 查找Java
    /// </summary>
    private async Task<List<JavaInstallation>> FindJavaWithAlternativesAsync()
    {
        var installations = new List<JavaInstallation>();

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/update-alternatives",
                Arguments = "--list java",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var paths = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var path in paths)
            {
                if (File.Exists(path) && await ValidateJavaInstallationAsync(path))
                {
                    var version = await GetJavaVersionAsync(path);
                    installations.Add(new JavaInstallation
                    {
                        Path = path,
                        Version = version,
                        Vendor = DetectVendor(version),
                        Source = InstallationSource.PathEnvironment
                    });
                }
            }
        }
        catch
        {
            // 忽略错误
        }

        return installations;
    }
}