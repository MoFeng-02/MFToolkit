using MFToolkit.App;
using MFToolkit.Download.Services;

namespace MFToolkit.Download.Utils;
/// <summary>
/// 文件
/// </summary>
public class DownloadUtil
{
    /// <summary>
    /// 获取下载文件实例
    /// </summary>
    /// <returns></returns>
    public static IDownloadService? DownloadService() => MFApp.GetService<IDownloadService>();
}
