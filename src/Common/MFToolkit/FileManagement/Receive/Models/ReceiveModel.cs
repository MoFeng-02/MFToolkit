#nullable disable
namespace MFToolkit.FileManagement.Receive.Models;
/// <summary>
/// 接收文件模型
/// </summary>
public class ReceiveModel
{
    /// <summary>
    /// 文件区分key
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// 文件名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 文件总大小
    /// </summary>
    public long? SumSize { get; set; }
    /// <summary>
    /// 当前得到文件大小
    /// </summary>
    public long YetSize { get; set; }
    /// <summary>
    /// 文件保存路径
    /// </summary>
    public string FileSavePath { get; set; }
}
