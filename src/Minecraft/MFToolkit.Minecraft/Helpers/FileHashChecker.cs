using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace MFToolkit.Minecraft.Helpers;
/// <summary>
/// 提供文件哈希检测的工具类
/// </summary>
public static class FileHashChecker
{
    /// <summary>
    /// 异步检测文件的SHA1哈希是否与预期值匹配
    /// </summary>
    /// <param name="filePath">待检测文件的完整路径</param>
    /// <param name="expectedSha1">预期的SHA1哈希值（小写十六进制字符串，长度40）</param>
    /// <param name="cancellationToken">取消令牌（用于中断检测操作）</param>
    /// <returns>检测结果：true=匹配，false=不匹配；文件不存在或异常时返回false</returns>
    public static async Task<bool> CheckFileSha1Async(
        string filePath,
        string expectedSha1,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        // 1. 输入验证
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath), "文件路径不能为空");

        if (string.IsNullOrWhiteSpace(expectedSha1) || expectedSha1.Length != 40)
            throw new ArgumentException("预期SHA1值必须是40位小写十六进制字符串", nameof(expectedSha1));

        // 2. 检查文件是否存在
        if (!File.Exists(filePath))
            return false;

        // 3. 异步计算文件SHA1哈希
        string actualSha1;
        try
        {
            // 使用异步文件流+SHA1计算（适配大文件，避免一次性加载到内存）
            using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,  // 4KB缓冲区，平衡性能与内存占用
                useAsync: true);   // 启用异步IO

            using var sha1 = SHA1.Create();  // 线程安全，无需单例

            // 异步计算哈希（.NET 5+支持ComputeHashAsync，低版本需手动实现）
            byte[] hashBytes = await sha1.ComputeHashAsync(fileStream, cancellationToken).ConfigureAwait(false);

            // 转换字节数组为小写十六进制字符串
            actualSha1 = Convert.ToHexStringLower(hashBytes);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 取消操作时返回false（或根据需求抛出异常）
            logger?.LogWarning($"{filePath}文件取消验证，为Token令牌取消");
            return false;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            // 处理文件读写异常（如文件被占用、权限不足等）
            logger?.LogWarning($"{filePath}文件哈希计算失败：{ex.Message}");
            return false;
        }

        // 4. 比较哈希值（忽略大小写，统一转为小写比较）
        return actualSha1 == expectedSha1;
    }
}