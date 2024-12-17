//using System.Buffers;
//using System.Collections.Concurrent;
//using System.Net;
//using System.Net.Sockets;

//namespace MFToolkit.Socket.TCP.Server.Service;
//public abstract class TcpServerService : IDisposable
//{
//    protected TcpListener? listener;
//    protected ConcurrentDictionary<string, TcpClient> tcpClients;
//    private readonly ConcurrentDictionary<TcpClient, byte[]> _clientBuffers = new ConcurrentDictionary<TcpClient, byte[]>(); // 存储每个客户端连接的读取缓冲区

//    private bool _isDisposable;

//    /// <summary>
//    /// 启动TCP服务
//    /// </summary>
//    /// <param name="host">启动IP</param>
//    /// <param name="port">启动端口</param>
//    /// <param name="secure">是否启用安全的，需要授权的</param>
//    /// <returns></returns>
//    public virtual async Task StartAsync(IPAddress host, int port, bool secure)
//    {
//        if (listener != null)
//        {
//            await Task.CompletedTask;
//            return;
//        }
//        listener = new TcpListener(host, port);
//        await HandleClientConnectionAsync();
//    }
//    private async Task HandleClientConnectionAsync()
//    {
//        if (listener == null)
//        {
//            await Task.CompletedTask;
//            return;
//        }
//        while (true)
//        {
//            var client = await listener.AcceptTcpClientAsync();
//            if (_isDisposable) break;
//            byte[] buffer = ArrayPool<byte>.Shared.Rent(256); // 从ArrayPool中租借缓冲区
//            if (_clientBuffers.TryAdd(client, buffer)) // 尝试将客户端连接和缓冲区添加到字典中
//            {
                
//            }
//            else
//            {
//                ArrayPool<byte>.Shared.Return(buffer); // 如果添加失败，归还缓冲区
//                client.Close(); // 关闭客户端连接
//            }
//        }
//    }

//    public void Dispose()
//    {
//        if (!tcpClients.IsEmpty)
//        {
//            foreach (var client in tcpClients)
//            {
//                client.Value.Close();
//                client.Value.Dispose();
//            }
//            tcpClients.Clear();
//        }
//        Dispose(true);
//        GC.SuppressFinalize(this);

//    }
//    protected virtual void Dispose(bool disposing)
//    {
//        if (disposing)
//        {
//            if (listener != null)
//            {
//                listener.Stop();
//                listener.Dispose();
//                listener = null;
//            }
//            _isDisposable = true;
//        }
//    }
//}
