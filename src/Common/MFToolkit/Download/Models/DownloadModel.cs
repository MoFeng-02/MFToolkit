namespace MFToolkit.Download.Models;
/// <summary>
/// 文件保存模型
/// </summary>
public sealed class DownloadModel
{
    /// <summary>
    /// 下载Key
    /// </summary>
    public string Key { get; set; } = Guid.NewGuid().ToString();
    /// <summary>
    /// 文件保存路径
    /// </summary>
    public required string FileSavePath { get; set; }
    /// <summary>
    /// 文件下载地址
    /// </summary>
    public required string DownloadUrl { get; set; }
    /// <summary>
    /// 下载缓冲区(default: 2M)
    /// </summary>
    public int WriteSize { get; set; } = 1024 * 1024 * 2;
    /// <summary>
    /// 当前已经下载了多少了
    /// </summary>
    public long YetSize { get; set; }
    /// <summary>
    /// 一共多大
    /// </summary>
    public long? SumSize { get; set; }
    /// <summary>
    /// 当前保存进度，总进度，这个不会保存到详情，请自行处理
    /// </summary>
    //[JsonIgnore]
    //public Action<long, long>? SaveHandle { get; set; }
}
