#nullable disable
namespace MFToolkit.Download.Models;
/// <summary>
/// 下载结果
/// </summary>
public sealed class DownloadResult
{
    /// <summary>
    /// 保存地址
    /// </summary>
    public string SavePath { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }
    /// <summary>
    /// 是否下载完成
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// 提示信息
    /// </summary>
    public string Message { get; set; }
}
