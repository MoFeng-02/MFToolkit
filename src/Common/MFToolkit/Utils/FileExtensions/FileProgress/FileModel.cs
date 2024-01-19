namespace MFToolkit.Utils.FileExtensions.FileProgress;
/// <summary>
/// 文件保模型
/// </summary>
internal partial class FileModel
{
    /// <summary>
    /// 文件保存名称
    /// </summary>
    public string FileSaveName { get; set; }
    /// <summary>
    /// 文件保存路径
    /// </summary>
    public string FileSavePath { get; set; }
    /// <summary>
    /// 文件下载地址
    /// </summary>
    public string RequestDownloadUri { get; set; }
    /// <summary>
    /// 当前保存进度，总进度
    /// </summary>
    public Action<long, long>? SaveHandle { get; set; }
}
