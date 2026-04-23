using System.Diagnostics;
using System.Runtime.InteropServices;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Helpers;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// 游戏启动配置（简洁版，含语言配置），全局和本地分区
/// </summary>
public class GameLauncherOptions : BaseOptions
{
    /// <summary>
    /// 配置文件路径
    /// </summary>
    public static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config", "launcher_config.json");

    #region 核心配置
    /// <summary>
    /// 最后使用的版本ID（必填，如 "1.20.1"）
    /// </summary>
    public string? LastUsedVersion { get; set; }

    /// <summary>
    /// 游戏语言（默认自动获取系统语言，可通过 LanguageHelper.GetAllMinecraftLanguages() 获取所有支持的语言）
    /// </summary>
    public string Language { get; set; } = LanguageHelper.GetSystemLanguageCode();
    #endregion

    #region 窗口配置
    /// <summary>
    /// 窗口宽度
    /// </summary>
    public int WindowWidth { get; set; } = 1280;

    /// <summary>
    /// 窗口高度
    /// </summary>
    public int WindowHeight { get; set; } = 720;

    /// <summary>
    /// 是否全屏
    /// </summary>
    public bool Fullscreen { get; set; } = false;
    #endregion

    #region 内存配置（支持自动计算）
    /// <summary>
    /// 最小内存分配（MB），启用自动计算时忽略此值
    /// </summary>
    public int MinMemoryMB { get; set; } = 512;

    /// <summary>
    /// 最大内存分配（MB），启用自动计算时忽略此值
    /// </summary>
    public int MaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// 是否启用内存自动计算（基于系统内存）
    /// </summary>
    public bool AutoCalculateMemory { get; set; } = true;

    /// <summary>
    /// 自动计算时，最大内存占系统内存的比例（0.3-0.5）
    /// </summary>
    public float AutoMemoryRatio { get; set; } = 0.4f;
    #endregion

    #region Java 配置
    /// <summary>
    /// Java 路径（留空则自动检测）
    /// </summary>
    public string? JavaPath { get; set; }
    #endregion

    #region 启动优化配置
    /// <summary>
    /// 是否启用G1GC垃圾回收器
    /// </summary>
    public bool UseG1GC { get; set; } = true;
    
    /// <summary>
    /// 是否启用ZGC垃圾回收器（Java 11+）
    /// </summary>
    public bool UseZGC { get; set; } = false;
    
    /// <summary>
    /// 是否启用字符串去重
    /// </summary>
    public bool UseStringDeduplication { get; set; } = true;
    
    /// <summary>
    /// 最大GC暂停时间（毫秒）
    /// </summary>
    public int MaxGCPauseMillis { get; set; } = 200;
    
    /// <summary>
    /// 是否启用并行GC线程优化
    /// </summary>
    public bool OptimizeGCThreads { get; set; } = true;
    
    /// <summary>
    /// 是否启用系统类数据共享
    /// </summary>
    public bool UseClassDataSharing { get; set; } = true;
    #endregion

    #region 自定义参数
    /// <summary>
    /// 额外 JVM 参数（追加到默认参数后）
    /// </summary>
    public List<string> ExtraJvmArguments { get; set; } = new();

    /// <summary>
    /// 额外游戏参数（追加到默认参数后）
    /// </summary>
    public List<string> ExtraGameArguments { get; set; } = new();
    #endregion

    #region 存储配置
    /// <summary>
    /// 存储模式
    /// </summary>
    public StorageMode StorageMode { get; set; } = StorageMode.Global;

    /// <summary>
    /// 游戏基础路径
    /// </summary>
    public string BasePath { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ".minecraft"
    );
    #endregion
    /// <summary>
    /// 计算最终内存配置（根据自动计算开关）
    /// </summary>
    public void CalculateMemory()
    {
        if (!AutoCalculateMemory) return;

        // 获取系统总内存（MB）
        var totalMemoryMB = GetTotalSystemMemoryMB();

        // 计算最大内存（系统内存 * 比例，限制 1GB-16GB）
        var calculatedMax = (int)Math.Clamp(
            totalMemoryMB * AutoMemoryRatio,
            1024,
            16384
        );

        // 计算最小内存（最大内存的 1/4，限制 512MB-最大内存）
        var calculatedMin = (int)Math.Clamp(
            calculatedMax * 0.25,
            512,
            calculatedMax
        );

        MinMemoryMB = calculatedMin;
        MaxMemoryMB = calculatedMax;
    }

    /// <summary>
    /// 获取系统总内存（MB），跨平台兼容
    /// </summary>
    private static long GetTotalSystemMemoryMB()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows 平台
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "wmic",
                        Arguments = "OS get TotalVisibleMemorySize /Value",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var parts = output.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && long.TryParse(parts[1].Trim(), out var totalBytes))
                {
                    return totalBytes / 1024; // 转换为 MB
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux 平台
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cat",
                        Arguments = "/proc/meminfo",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var totalKb))
                        {
                            return totalKb / 1024; // 转换为 MB
                        }
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS 平台
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sysctl",
                        Arguments = "hw.memsize",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var parts = output.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && long.TryParse(parts[1].Trim(), out var totalBytes))
                {
                    return totalBytes / (1024 * 1024); // 转换为 MB
                }
            }
        }
        catch (Exception ex)
        {
            // 异常时返回默认值 8GB
            System.Diagnostics.Debug.WriteLine($"获取系统内存失败: {ex.Message}");
        }
        return 8192;
    }

    /// <summary>
    /// 获取基础 JVM 参数（默认参数 + 额外参数）
    /// </summary>
    public List<string> GetBaseJvmArguments()
    {
        // 计算内存配置
        CalculateMemory();

        var jvmArgs = new List<string>
        {
            // 基础内存参数
            $"-Xms{MinMemoryMB}M",
            $"-Xmx{MaxMemoryMB}M"
        };
        
        // 垃圾回收器配置
        if (UseZGC)
        {
            jvmArgs.Add("-XX:+UseZGC");
        }
        else if (UseG1GC)
        {
            jvmArgs.Add("-XX:+UseG1GC");
            jvmArgs.Add($"-XX:MaxGCPauseMillis={MaxGCPauseMillis}");
        }
        
        // 优化参数
        if (UseStringDeduplication)
        {
            jvmArgs.Add("-XX:+UseStringDeduplication");
        }
        
        if (OptimizeGCThreads)
        {
            jvmArgs.Add("-XX:ParallelGCThreads=0"); // 自动检测
            jvmArgs.Add("-XX:ConcGCThreads=0"); // 自动检测
        }
        
        if (UseClassDataSharing)
        {
            jvmArgs.Add("-Xshare:on");
        }
        
        // 通用优化参数
        jvmArgs.Add("-XX:+UnlockExperimentalVMOptions");
        jvmArgs.Add("-XX:+UnlockDiagnosticVMOptions");
        jvmArgs.Add("-XX:+AlwaysPreTouch");
        
        // 添加用户额外 JVM 参数（去重）
        if (ExtraJvmArguments.Any())
        {
            jvmArgs.AddRange(ExtraJvmArguments.Where(arg => !string.IsNullOrWhiteSpace(arg)));
        }

        return jvmArgs.Distinct().ToList();
    }

    // 简单验证语言代码格式（避免非法值）
    static bool IsValidLanguageCode(string code) =>
        !string.IsNullOrEmpty(code) && (code.Contains('_') || code.Length == 2) && code.Length <= 5;

    /// <summary>
    /// 验证配置有效性
    /// </summary>
    public bool Validate()
    {
        CalculateMemory();
        
        // 验证自动内存比例
        if (AutoMemoryRatio < 0.3f || AutoMemoryRatio > 0.5f)
        {
            AutoMemoryRatio = 0.4f; // 重置为默认值
        }
        
        // 验证GC配置
        if (UseG1GC && UseZGC)
        {
            UseZGC = false; // 只能选择一个GC
        }
        
        // 验证最大GC暂停时间
        if (MaxGCPauseMillis < 50 || MaxGCPauseMillis > 1000)
        {
            MaxGCPauseMillis = 200; // 重置为默认值
        }

        return !string.IsNullOrEmpty(LastUsedVersion) &&
               IsValidLanguageCode(Language) &&
               WindowWidth >= 640 &&
               WindowHeight >= 480 &&
               MinMemoryMB >= 512 &&
               MaxMemoryMB >= MinMemoryMB &&
               MaxMemoryMB <= 32768 && // 增加到32GB支持
               Directory.Exists(Path.GetDirectoryName(BasePath) ?? BasePath);
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public Task SaveAsync()
    {
        CalculateMemory(); // 保存时确保内存配置有效
        Validate(); // 保存前验证配置
        return base.SaveAsync(ConfigPath);
    }
}
