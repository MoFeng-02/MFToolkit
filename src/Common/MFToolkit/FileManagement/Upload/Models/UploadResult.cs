#nullable disable
using MFToolkit;

namespace MFToolkit.FileManagement.Upload.Models;
/// <summary>
/// 返回上传后的路径
/// </summary>
public class UploadResult
{
    /// <summary>
    /// 文件指纹key
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// 网路文件地址
    /// </summary>
    public string HttpPath { get; set; }
    /// <summary>
    /// 相对地址
    /// </summary>
    public string RelativePath { get; set; }
}
