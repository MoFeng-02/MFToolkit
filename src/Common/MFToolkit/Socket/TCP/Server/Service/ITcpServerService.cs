using System.Net;

namespace MFToolkit.Socket.TCP.Server.Service;
public interface ITcpServerService
{
    /// <summary>
    /// 启动TCP服务端
    /// </summary>
    /// <param name="iPAddress"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    Task StartAsync(IPAddress iPAddress, int port);
    /// <summary>
    /// 连接TCP客户端
    /// </summary>
    /// <returns></returns>
    Task ConnectionAsync();
}
