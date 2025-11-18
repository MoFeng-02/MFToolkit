using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// 启动器配置
/// </summary>
public class LauncherOptions : BaseOptions
{
    public static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config", "launcher_config.json");

    /// <summary>
    /// 最后使用的版本ID
    /// </summary>
    public string? LastUsedVersion { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 窗口宽度
    /// </summary>
    public int WindowWidth { get; set; } = 854;

    /// <summary>
    /// 窗口高度
    /// </summary>
    public int WindowHeight { get; set; } = 480;

    /// <summary>
    /// 最小内存分配（MB）
    /// </summary>
    public int MinMemoryMB { get; set; } = 512;

    /// <summary>
    /// 最大内存分配（MB）
    /// </summary>
    public int MaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// Java路径
    /// </summary>
    public string? JavaPath { get; set; }

    /// <summary>
    /// 存储模式
    /// </summary>
    public StorageMode StorageMode { get; set; } = StorageMode.Global;

    /// <summary>
    /// 基础路径
    /// </summary>
    public string BasePath { get; set; } = ".minecraft";

    /// <summary>
    /// 是否启用全屏
    /// </summary>
    public bool Fullscreen { get; set; } = false;

    /// <summary>
    /// 是否启用高级OpenGL
    /// </summary>
    public bool AdvancedOpenGL { get; set; } = true;

    /// <summary>
    /// 游戏参数
    /// </summary>
    public List<string> GameArguments { get; set; } = new();

    /// <summary>
    /// JVM参数
    /// </summary>
    public List<string> JvmArguments { get; set; } = new();

    /// <summary>
    /// 创建存储选项
    /// </summary>
    /// <returns>存储选项</returns>
    public StorageOptions CreateStorageOptions()
    {
        return new StorageOptions
        {
            BasePath = BasePath,
            StorageMode = StorageMode
        };
    }

    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    public bool Validate()
    {
        return !string.IsNullOrEmpty(Username) &&
               WindowWidth > 0 &&
               WindowHeight > 0 &&
               MinMemoryMB > 0 &&
               MaxMemoryMB >= MinMemoryMB;
    }

    /// <summary>
    /// 保存当前选项类
    /// </summary>
    /// <returns></returns>
    public Task SaveAsync()
    {
        return base.SaveAsync(ConfigPath);
    }
}