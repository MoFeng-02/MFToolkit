namespace MFToolkit.FileManagement.Receive.Models;
/// <summary>
/// 分片接收模型
/// <para>主要应对大型文件的分片处理</para>
/// </summary>
public class ReceivePartModel : ReceiveModel
{
    /// <summary>
    /// 分片总Key，即代表所有分片文件的识别key，用于合成
    /// </summary>
    public string PartKey { get; set; }
    /// <summary>
    /// 分片所在下标
    /// </summary>
    public int PartIndex { get; set; }
    /// <summary>
    /// 所有分片文件总数量
    /// </summary>
    public int SumPartCount { get; set; }
}
