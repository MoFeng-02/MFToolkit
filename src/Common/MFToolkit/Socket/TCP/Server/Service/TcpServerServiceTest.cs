using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MFToolkit.Socket.TCP.Server.Service;
public abstract class TcpServerServiceTest : IDisposable
{
    private TcpListener _listener; // TCP监听器
    private readonly ConcurrentDictionary<string, TcpClient> _clientNames = new ConcurrentDictionary<string, TcpClient>(); // 存储客户端名称和对应连接的字典
    private readonly ConcurrentDictionary<TcpClient, byte[]> _clientBuffers = new ConcurrentDictionary<TcpClient, byte[]>(); // 存储每个客户端连接的读取缓冲区
    private bool _disposed; // 标记服务是否已释放资源

    /// <summary>
    /// 启动TCP服务并开始监听客户端连接。
    /// </summary>
    /// <param name="ipAddress">服务器绑定的IP地址。</param>
    /// <param name="port">服务器监听的端口号。</param>
    /// <returns>一个表示异步操作的任务。</returns>
    public virtual async Task StartAsync(IPAddress ipAddress, int port)
    {
        _listener = new TcpListener(ipAddress, port); // 创建TCP监听器
        _listener.Start(); // 启动监听器
        await AcceptClientsAsync(); // 开始异步接受客户端连接
    }

    /// <summary>
    /// 持续等待并接受新的客户端连接。
    /// </summary>
    /// <returns>一个表示异步操作的任务。</returns>
    private async Task AcceptClientsAsync()
    {
        while (!_disposed) // 当服务未被释放时，持续接受新的连接
        {
            var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false); // 异步接受新连接
            if (_disposed) break; // 如果服务已被释放，则停止接受新连接

            byte[] buffer = ArrayPool<byte>.Shared.Rent(256); // 从ArrayPool中租借缓冲区
            if (_clientBuffers.TryAdd(client, buffer)) // 尝试将客户端连接和缓冲区添加到字典中
            {
                await HandleClientAsync(client).ContinueWith(task => // 异步处理客户端连接
                 {
                     if (task.IsFaulted)
                         Console.WriteLine($"Error handling client: {task.Exception?.InnerException?.Message}"); // 如果处理过程中发生错误，记录日志
                     RemoveClient(client, buffer); // 处理完成后移除客户端连接和缓冲区
                 }, TaskScheduler.Default);
            }
            else
            {
                ArrayPool<byte>.Shared.Return(buffer); // 如果添加失败，归还缓冲区
                client.Close(); // 关闭客户端连接
            }
        }
    }

    /// <summary>
    /// 移除客户端连接并回收其资源。
    /// </summary>
    /// <param name="client">要移除的客户端连接。</param>
    /// <param name="buffer">该客户端连接使用的缓冲区。</param>
    private void RemoveClient(TcpClient client, byte[] buffer)
    {
        string key = GetClientKey(client); // 获取客户端的唯一标识符
        _clientNames.TryRemove(key, out _); // 从字典中移除客户端名称
        ArrayPool<byte>.Shared.Return(buffer); // 归还缓冲区到ArrayPool
        client.Close(); // 关闭客户端连接
    }
    private TcpClient FindClientByName(string name)
    {
        return _clientNames.FirstOrDefault(x => x.Value.Client.RemoteEndPoint.ToString().Contains(name)).Value; // 根据客户端名称查找客户端连接
    }
    /// <summary>
    /// 处理单个客户端的通信。
    /// </summary>
    /// <param name="client">要处理的客户端连接。</param>
    /// <returns>一个表示异步操作的任务。</returns>
    private async Task HandleClientAsync(TcpClient client)
    {
        var stream = client.GetStream(); // 获取客户端的网络流
        try
        {
            while (await ReadMessageAsync(stream, _clientBuffers[client])) // 循环读取消息，直到客户端断开连接
            {
                // 消息处理逻辑可以在这里实现...
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}"); // 如果处理过程中发生异常，记录日志
        }
    }

    /// <summary>
    /// 从给定的网络流中异步读取消息。
    /// </summary>
    /// <param name="stream">客户端的网络流。</param>
    /// <param name="buffer">用于读取消息的缓冲区。</param>
    /// <returns>一个表示异步操作的任务，返回值为true表示成功读取消息，false表示客户端已断开。</returns>
    private async Task<bool> ReadMessageAsync(NetworkStream stream, byte[] buffer)
    {
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false); // 异步读取数据
        if (bytesRead == 0) return false; // 如果没有读取到数据，表示客户端可能已断开连接

        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 将读取到的字节数据转换为字符串
        var messageParts = receivedData.Split(',', 2); // 分割消息为两部分：目标用户和消息内容
        if (messageParts.Length >= 2 && messageParts[0].StartsWith("TO:", StringComparison.Ordinal) && messageParts[1].StartsWith("MESSAGE:", StringComparison.Ordinal))
        {
            string targetClientName = messageParts[0][3..]; // 提取目标用户名
            string messageContent = messageParts[1][8..]; // 提取消息内容

            if (_clientNames.TryGetValue(targetClientName, out var targetClient)) // 根据目标用户名查找客户端连接
            {
                var targetStream = targetClient.GetStream(); // 获取目标客户端的网络流
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageContent); // 将消息内容转换为字节数据
                await targetStream.WriteAsync(messageBytes, 0, messageBytes.Length).ConfigureAwait(false); // 异步发送消息给目标客户端
            }
            else
            {
                var senderStream = stream; // 获取发送者的网络流
                byte[] errorMessageBytes = Encoding.UTF8.GetBytes("Error: Target client not found."); // 准备错误消息
                await senderStream.WriteAsync(errorMessageBytes, 0, errorMessageBytes.Length).ConfigureAwait(false); // 异步发送错误消息给发送者
            }
        }
        return true;
    }

    private string GetClientKey(TcpClient client)
    {
        return $"{client.Client.RemoteEndPoint}"; // 使用客户端的网络地址和端口号作为唯一标识符
    }

    /// <summary>
    /// 设置客户端的名称。
    /// </summary>
    /// <param name="client">客户端连接。</param>
    /// <param name="name">客户端名称。</param>
    /// <returns>是否成功设置名称。</returns>
    public bool SetClientName(TcpClient client, string name)
    {
        string key = GetClientKey(client); // 获取客户端的唯一标识符
        return _clientNames.TryAdd(key, client) && _clientNames.TryUpdate(key, client, null); // 尝试添加或更新客户端名称
    }

    /// <summary>
    /// 停止TCP服务并清理所有资源。
    /// </summary>
    /// <returns>一个表示异步操作的任务。</returns>
    public virtual async Task StopAsync()
    {
        _disposed = true; // 标记服务已释放资源
        _listener?.Stop(); // 停止监听新的连接
        foreach (var client in _clientBuffers.Keys.ToArray()) // 遍历所有客户端连接
        {
            RemoveClient(client, _clientBuffers[client]); // 移除客户端连接并回收资源
        }
        _clientBuffers.Clear(); // 清空客户端连接字典
        _clientNames.Clear(); // 清空客户端名称字典
    }

    #region IDisposable Support
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed) // 如果服务未释放资源
        {
            if (disposing) // 如果正在释放托管资源
            {
                _listener?.Stop(); // 停止监听器
                foreach (var client in _clientBuffers.Keys.ToArray()) // 遍历所有客户端连接
                {
                    RemoveClient(client, _clientBuffers[client]); // 移除客户端连接并回收资源
                }
            }

            _disposed = true; // 标记服务已释放资源
        }
    }

    /// <summary>
    /// 实现IDisposable接口，确保可以正确清理非托管资源。
    /// </summary>
    public void Dispose()
    {
        Dispose(true); // 释放托管和非托管资源
        GC.SuppressFinalize(this); // 告诉垃圾收集器不再调用终结器
    }
    #endregion
}
