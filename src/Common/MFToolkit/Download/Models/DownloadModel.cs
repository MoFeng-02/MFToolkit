using System.Text.Json.Serialization;

namespace MFToolkit.Download.Models;
/// <summary>
/// 文件保存模型
/// </summary>
public sealed class DownloadModel
{
    /// <summary>
    /// 文件保存路径
    /// </summary>
    public string FileSavePath { get; set; } = null!;
    /// <summary>
    /// 文件下载地址
    /// </summary>
    public string DownloadUrl { get; set; } = null!;
    /// <summary>
    /// 下载缓冲区(default: 2M)
    /// </summary>
    public int WirteSize { get; set; } = 1024 * 1024 * 2;
    /// <summary>
    /// 当前已经下载了多少了
    /// </summary>
    public long YetDownloadSize { get; internal set; }
    /// <summary>
    /// 一共多少
    /// </summary>
    public long? SumDownloadSize { get; internal set; }
    /// <summary>
    /// 当前保存进度，总进度，这个不会保存到详情，请自行处理
    /// </summary>
    //[JsonIgnore]
    //public Action<long, long>? SaveHandle { get; set; }
}
