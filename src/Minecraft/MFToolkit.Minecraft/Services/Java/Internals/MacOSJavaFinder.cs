using System.Diagnostics;
using System.Runtime.Versioning;
using MFToolkit.Minecraft.Entities.Java;
using MFToolkit.Minecraft.Enums.Java;

namespace MFToolkit.Minecraft.Services.Java.Internals;

/// <summary>
/// macOS平台Java查找器
/// </summary>
[SupportedOSPlatform("macos")]
public class MacOSJavaFinder : JavaFinderBase
    {
        /// <summary>
        /// 初始化macOS特定的Java路径
        /// </summary>
        public MacOSJavaFinder()
        {
            var home = Environment.GetEnvironmentVariable("HOME") ?? "/Users";
            
            CommonJavaPaths.AddRange([
                "/Library/Java/JavaVirtualMachines",
                "/System/Library/Java/JavaVirtualMachines",
                "/usr/libexec",
                "/usr/bin",
                $"{home}/.sdkman/candidates/java",
                "/opt/homebrew/opt/openjdk",
                "/usr/local/opt/openjdk",
                "/Applications/Xcode.app/Contents/Applications/Application Loader.app/Contents/MacOS/itms/java"
            ]);
            
            ProcessNames.AddRange(new[] { "java" });
        }

        /// <summary>
        /// 查找macOS系统中的所有Java安装
        /// </summary>
        public override async Task<List<JavaInstallation>> FindJavaInstallationsAsync()
        {
            var installations = new List<JavaInstallation>();
            
            // 1. 从常见目录查找
            installations.AddRange(await SearchInDirectoriesAsync());
            
            // 2. 从环境变量查找
            installations.AddRange(await FindJavaFromEnvironmentAsync());
            
            // 3. 使用 which 命令查找
            installations.AddRange(await FindJavaWithWhichCommandAsync());
            
            // 4. 从进程查找
            installations.AddRange(await FindJavaFromProcessesAsync());
            
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
        /// 使用 which 命令查找Java
        /// </summary>
        private async Task<List<JavaInstallation>> FindJavaWithWhichCommandAsync()
        {
            var installations = new List<JavaInstallation>();
            
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/which",
                    Arguments = "java",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start();
                var path = (await process.StandardOutput.ReadToEndAsync()).Trim();
                await process.WaitForExitAsync();
                
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
            catch
            {
                // 忽略错误
            }
            
            return installations;
        }
    }