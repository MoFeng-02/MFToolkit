namespace MFToolkit.Utility;
/// <summary>
/// 应用程序工具类
/// </summary>
public class AppUtil
{
    /// <summary>
    /// 计算下载进度（使用 double 类型）
    /// </summary>
    /// <param name="downloadedBytes">当前下载字节</param>
    /// <param name="totalBytes">总字节</param>
    /// <returns>下载进度百分比（0.00 到 100.00）</returns>
    /// <exception cref="ArgumentException">当 totalBytes 小于或等于 0 时抛出</exception>
    public static double CalculateDownloadProgress(long downloadedBytes, long totalBytes)
    {
        ValidateInput(downloadedBytes, totalBytes);
        return ((double)downloadedBytes / totalBytes) * 100;
    }

    /// <summary>
    /// 计算更精确的下载进度（使用 decimal 类型）
    /// </summary>
    /// <param name="downloadedBytes">当前下载字节</param>
    /// <param name="totalBytes">总字节</param>
    /// <returns>下载进度百分比（0.00 到 100.00）</returns>
    /// <exception cref="ArgumentException">当 totalBytes 小于或等于 0 时抛出</exception>
    public static decimal CalculateDownloadProgressDecimal(long downloadedBytes, long totalBytes)
    {
        ValidateInput(downloadedBytes, totalBytes);
        return ((decimal)downloadedBytes / totalBytes) * 100;
    }

    /// <summary>
    /// 验证输入参数
    /// </summary>
    /// <param name="downloadedBytes">当前下载字节</param>
    /// <param name="totalBytes">总字节</param>
    /// <exception cref="ArgumentException">当 totalBytes 小于或等于 0 时抛出</exception>
    private static void ValidateInput(long downloadedBytes, long totalBytes)
    {
        if (totalBytes <= 0)
        {
            throw new ArgumentException("总字节数必须大于0。请检查 totalBytes 参数。");
        }

        if (downloadedBytes < 0)
        {
            throw new ArgumentException("已下载字节数不能为负数。请检查 downloadedBytes 参数。");
        }
    }
}