using System.Text.Json;
using MFToolkit.Minecraft.JsonExtensions;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// 基础选项类，提供保存选项支持
/// </summary>
public class BaseOptions
{
    /// <summary>
    /// 配置路径
    /// </summary>
    public string ConfigFilePath { get; }

    public BaseOptions(string configFilePath, string? basePath = null)
    {
        if (basePath == null)
            ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "Configs", configFilePath);
        else
            ConfigFilePath = Path.Combine(basePath, configFilePath);
        Directory.CreateDirectory(basePath ?? Path.Combine(AppContext.BaseDirectory, "Configs"));
    }


    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    /// <returns></returns>
    public string GetConfigFilePath()
    {
        return ConfigFilePath;
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <returns></returns>
    protected async Task SaveAsync()
    {
        var directory = Path.GetDirectoryName(ConfigFilePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // 直接序列化到文件流
        await using var fileStream = File.Create(ConfigFilePath);
        await JsonSerializer.SerializeAsync(fileStream, this, MinecraftJsonSerializerContext.Default.Options);
    }
}
