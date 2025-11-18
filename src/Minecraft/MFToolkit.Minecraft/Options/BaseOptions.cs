using System.Text.Json;
using MFToolkit.Minecraft.JsonExtensions;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// 基础选项类，提供保存选项支持
/// </summary>
public abstract class BaseOptions
{
    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <returns></returns>
    protected virtual async Task SaveAsync(string? savePath = null)
    {
        // 参数验证
        if (string.IsNullOrWhiteSpace(savePath))
        {
            throw new ArgumentNullException(nameof(savePath), "保存路径不能为空");
        }

        try
        {
            var directory = Path.GetDirectoryName(savePath);

            // 确保目录存在
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 使用临时文件避免写入过程中出现损坏
            var tempPath = $"{savePath}.tmp";

            await using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(
                    fileStream,
                    this,
                    MinecraftJsonSerializerContext.Default.Options
                );

                // 确保数据完全写入磁盘
                await fileStream.FlushAsync();
            }

            // 验证临时文件是否成功创建
            if (!File.Exists(tempPath) || new FileInfo(tempPath).Length == 0)
            {
                throw new IOException("临时文件创建失败或为空");
            }

            // 原子性替换原文件
            File.Move(tempPath, savePath, overwrite: true);
        }
        catch (Exception ex) when (ex is not (ArgumentNullException or IOException or UnauthorizedAccessException))
        {
            // 将其他异常包装为IOException
            throw new IOException($"保存配置文件到 '{savePath}' 失败", ex);
        }
    }
}