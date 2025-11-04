using System.Text.Json;
using MFToolkit.Minecraft.Core;
using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// 启动器配置
/// </summary>
public class LauncherConfig() : BaseOptions("launcher_config.json")
{
    private static readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "config", "launcher_config.json");
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
    /// 保存配置
    /// </summary>
    public void Save()
    {
        try
        {
            // 确保目录存在
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(this, MinecraftJsonSerializerContext.Default.LauncherConfig);
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存配置失败: {ex.Message}");
        }
    }

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
}
