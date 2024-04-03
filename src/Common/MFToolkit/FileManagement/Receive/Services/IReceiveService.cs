namespace MFToolkit.FileManagement.Receive.Services;
public interface IReceiveService
{
    /// <summary>
    /// 初始化接收，即得到部分信息
    /// </summary>
    /// <returns></returns>
    Task<bool> InitReceiveAsync();
}
