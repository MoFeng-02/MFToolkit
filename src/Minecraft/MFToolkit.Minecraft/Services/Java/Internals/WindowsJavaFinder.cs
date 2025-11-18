using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;
using MFToolkit.Minecraft.Entities.Java;
using MFToolkit.Minecraft.Enums.Java;

namespace MFToolkit.Minecraft.Services.Java.Internals;

/// <summary>
/// Windows平台Java查找器
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsJavaFinder : JavaFinderBase
{
    public WindowsJavaFinder()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var publicProfile = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);

        CommonJavaPaths.AddRange([
            Path.Combine(programFiles, "Java"),
            Path.Combine(programFilesX86, "Java"),
            Path.Combine(localAppData, "Packages"),
            Path.Combine(programFiles, "Eclipse Foundation"),
            Path.Combine(programFiles, "Amazon Corretto"),
            Path.Combine(programFiles, "Microsoft"),
            Path.Combine(programFiles, "AdoptOpenJDK"),
            Path.Combine(programFiles, "Eclipse Adoptium"),
            Path.Combine(programFiles, "BellSoft"),
            Path.Combine(programFiles, "GraalVM"),
            Path.Combine(programFiles, "JetBrains"),
            Path.Combine(programFiles, "RedHat"),
            Path.Combine(publicProfile, "Java"),
            Path.Combine(userProfile, "scoop", "apps"),
            Path.Combine(userProfile, "AppData", "Local", "Programs"),
            @"C:\Program Files\Zulu",
            @"C:\Program Files\Liberica"
        ]);

        ProcessNames.AddRange(new[] { "java.exe", "javaw.exe" });
    }

    public override async Task<List<JavaInstallation>> FindJavaInstallationsAsync()
    {
        var installations = new List<JavaInstallation>();

        try
        {
            // 1. 从环境变量查找
            installations.AddRange(await FindJavaFromEnvironmentAsync());

            // 2. 从常见目录查找
            installations.AddRange(await SearchInDirectoriesAsync());

            // 3. 从进程查找（可选）
            try
            {
                installations.AddRange(await FindJavaFromProcessesAsync());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"进程查找失败: {ex.Message}");
            }

            // 去重并返回
            return installations
                .GroupBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Java查找过程中发生错误: {ex.Message}");
            return installations;
        }
    }

    public override async Task<List<JavaInstallation>> FindJavaFromProcessesAsync()
    {
        var installations = new List<JavaInstallation>();

        try
        {
            var processNames = new[] { "java", "javaw" };

            foreach (var processName in processNames)
            {
                try
                {
                    var processes = Process.GetProcessesByName(processName);
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (process.HasExited) continue;
                            var possiblePaths = await FindPossibleJavaPathsAsync($"{processName}.exe");

                            foreach (var possiblePath in possiblePaths)
                            {
                                if (!File.Exists(possiblePath) ||
                                    !await ValidateJavaInstallationAsync(possiblePath)) continue;
                                var version = await GetJavaVersionAsync(possiblePath);
                                var (is64Bit, architecture) = await DetectArchitectureAsync(possiblePath);
                                var majorVersion = ParseMajorVersion(version);

                                installations.Add(new JavaInstallation
                                {
                                    Path = possiblePath,
                                    Version = version,
                                    Vendor = DetectVendor(version),
                                    Source = InstallationSource.ProcessSearch,
                                    Is64Bit = is64Bit,
                                    Architecture = architecture,
                                    MajorVersion = majorVersion
                                });
                                break;
                            }
                        }
                        catch (Win32Exception)
                        {
                            continue;
                        }
                        catch (InvalidOperationException)
                        {
                            continue;
                        }
                        finally
                        {
                            process?.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"获取进程 {processName} 失败: {ex.Message}");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"进程查找总体失败: {ex.Message}");
        }

        return installations;
    }

    private async Task<List<JavaInstallation>> FindJavaFromEnvironmentAsync()
    {
        var installations = new List<JavaInstallation>();

        try
        {
            // 从PATH环境变量查找
            var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(';') ?? Array.Empty<string>();
            foreach (var pathDir in pathDirs)
            {
                if (string.IsNullOrEmpty(pathDir) || !Directory.Exists(pathDir))
                    continue;

                try
                {
                    foreach (var processName in ProcessNames)
                    {
                        var javaPath = Path.Combine(pathDir, processName);
                        if (File.Exists(javaPath) && await ValidateJavaInstallationAsync(javaPath))
                        {
                            var version = await GetJavaVersionAsync(javaPath);
                            var (is64Bit, architecture) = await DetectArchitectureAsync(javaPath);
                            var majorVersion = ParseMajorVersion(version);

                            installations.Add(new JavaInstallation
                            {
                                Path = javaPath,
                                Version = version,
                                Vendor = DetectVendor(version),
                                Source = InstallationSource.PathEnvironment,
                                Is64Bit = is64Bit,
                                Architecture = architecture,
                                MajorVersion = majorVersion
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PATH目录 {pathDir} 处理失败: {ex.Message}");
                    continue;
                }
            }

            // 从JAVA_HOME环境变量查找
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
            {
                try
                {
                    foreach (var processName in ProcessNames)
                    {
                        var javaPath = Path.Combine(javaHome, "bin", processName);
                        if (File.Exists(javaPath) && await ValidateJavaInstallationAsync(javaPath))
                        {
                            var version = await GetJavaVersionAsync(javaPath);
                            var (is64Bit, architecture) = await DetectArchitectureAsync(javaPath);
                            var majorVersion = ParseMajorVersion(version);

                            installations.Add(new JavaInstallation
                            {
                                Path = javaPath,
                                Version = version,
                                Vendor = DetectVendor(version),
                                Source = InstallationSource.PathEnvironment,
                                Is64Bit = is64Bit,
                                Architecture = architecture,
                                MajorVersion = majorVersion
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JAVA_HOME {javaHome} 处理失败: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"环境变量查找失败: {ex.Message}");
        }

        return installations;
    }

    protected override async Task<List<JavaInstallation>> SearchInDirectoriesAsync()
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
                    if (foundPaths.Contains(javaExe))
                        continue;

                    if (await ValidateJavaInstallationAsync(javaExe))
                    {
                        var version = await GetJavaVersionAsync(javaExe);
                        var (is64Bit, architecture) = await DetectArchitectureAsync(javaExe);
                        var majorVersion = ParseMajorVersion(version);

                        foundPaths.Add(javaExe);

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
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
        }

        return installations;
    }

    private async Task<List<string>> FindPossibleJavaPathsAsync(string processName)
    {
        var possiblePaths = new List<string>();

        foreach (var basePath in CommonJavaPaths)
        {
            if (!Directory.Exists(basePath))
                continue;

            try
            {
                var executables = Directory.EnumerateFiles(basePath, processName, SearchOption.AllDirectories);
                possiblePaths.AddRange(executables);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }
        }

        return await Task.FromResult(possiblePaths);
    }
}