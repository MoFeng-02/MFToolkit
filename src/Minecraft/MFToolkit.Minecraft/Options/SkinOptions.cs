using System;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// Minecraft皮肤操作配置
/// </summary>
public class SkinOptions : BaseOptions
{
    public static readonly string ConfigPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "skin_config.json");

    /// <summary>
    /// 配置项名称
    /// </summary>
    public const string Skin = "Minecraft:Skin";

    /// <summary>
    /// 皮肤上传超时时间（秒）
    /// </summary>
    public int UploadTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// 皮肤最大尺寸（字节）
    /// </summary>
    public long MaxSizeBytes { get; set; } = 2 * 1024 * 1024; // 2MB

    /// <summary>
    /// 允许的皮肤文件类型
    /// </summary>
    public string[] AllowedContentTypes { get; set; } = { "image/png", "image/jpeg" };

    /// <summary>
    /// 允许的皮肤宽度
    /// </summary>
    public int[] AllowedWidths { get; set; } = { 64, 128 };

    /// <summary>
    /// 允许的皮肤高度
    /// </summary>
    public int[] AllowedHeights { get; set; } = { 64, 128 };

    /// <summary>
    /// 默认皮肤模型
    /// </summary>
    public string DefaultModel { get; set; } = "classic";

    /// <summary>
    /// 是否允许删除皮肤
    /// </summary>
    public bool AllowSkinDeletion { get; set; } = true;

    /// <summary>
    /// 保存当前选项类
    /// </summary>
    /// <returns></returns>
    public Task SaveAsync()
    {
        return base.SaveAsync(ConfigPath);
    }
}