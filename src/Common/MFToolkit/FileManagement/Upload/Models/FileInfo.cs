#nullable disable
using MFToolkit;

namespace MFToolkit.FileManagement.Upload.Models;
public class FileInfo
{
    /// <summary>
    /// 文件唯一ID（单文件可以不传，但是分片需要用这个来确保分片正确）
    /// </summary>
    public string FileId { get; set; }
    /// <summary>
    /// 文件名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }
    /// <summary>
    /// 文件排序（一般用于分片上传的）
    /// </summary>
    public int SortIndex { get; set; }
    /// <summary>
    /// 分片起点
    /// </summary>
    public int StartIndex { get; set; }
    /// <summary>
    /// 分片终点
    /// </summary>
    public int EndIndex { get; set; }
    /// <summary>
    /// 哈希值
    /// </summary>
    public string HashValue { get; set; }
    /// <summary>
    /// 是否为分片
    /// </summary>
    public bool Sharding { get; set; }
}
