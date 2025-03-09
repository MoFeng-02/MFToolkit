namespace MFToolkit.Download.Constant;
/// <summary>
/// 下载常量类
/// </summary>
public class DownloadConstant
{
    /// <summary>
    /// 下载默认保存信息所在位置
    /// </summary>
    public readonly static string SaveFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Download");
}
