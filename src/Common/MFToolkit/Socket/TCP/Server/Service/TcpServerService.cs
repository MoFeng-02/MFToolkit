
using System.Net;
using System.Net.Sockets;

namespace MFToolkit.Socket.TCP.Server.Service;
public class TcpServerService : ITcpServerService
{
    private static TcpListener server = null!;

    public async Task ConnectionAsync()
    {

    }

    public async Task StartAsync(IPAddress iPAddress, int port)
    {
        if (server != null) return;
        server = new TcpListener(iPAddress, port);
        server.Start();
        await Console.Out.WriteLineAsync("TCP服务启动");
    }

}
