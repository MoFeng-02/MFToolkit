using System.Collections.Concurrent;
using Common;
using Grpc.Core;
using MFToolkit.CommonTypes.Enumerates;
using MFToolkit.Socket.Grpc.GrpcManagers;

namespace MFToolkit.Socket.Grpc;
public class GrpcServer : GrpcCommunicationHub.GrpcCommunicationHubBase
{
    private static readonly ConcurrentDictionary<string, ClientInfoQueue> _clientQueue = new();
    public override async Task ConnectAsync(IAsyncStreamReader<ClientInfo> requestStream, IServerStreamWriter<CommunicationSendMessage> responseStream, ServerCallContext context)
    {
        var clientInfo = requestStream.Current;
        var client = new ClientInfoQueue
        {
            CID = clientInfo.ClientId,
            ConnectionState = CommonConnectionState.Connected,
            RequestStream = requestStream,
            ResponseStream = responseStream,
        };
        _clientQueue.TryAdd(clientInfo.ClientId, client);
        try
        {
            while (!client.CancellationTokenSource.Token.IsCancellationRequested && await requestStream.MoveNext())
            {
                client.CancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }
        finally
        {
            _clientQueue.TryRemove(clientInfo.ClientId, out _);
            client.ConnectionState = CommonConnectionState.Disconnected;
            client.CancellationTokenSource.Cancel();
            client.CancellationTokenSource.Dispose();
        }
    }
    // 重新连接
    //public override async Task<CommunicationResult> ReConnectAsync(ClientInfo request, ServerCallContext context)
    //{
    //    var isGetOk = _clientQueue.TryGetValue(request.ClientId, out var client);
    //    if (!isGetOk)
    //    {
    //        await Task.CompletedTask;
    //        return new() { Message = "连接失败，无法重新连接，未找到客户端ID", Successed = false };
    //    }
    //    client!.ConnectionState = CommonConnectionState.Reconnection;
    //    await client.CancellationTokenSource.CancelAsync();
    //    _clientQueue.TryRemove(request.ClientId, out _);
    //}
    public override async Task<CommunicationResult> DisconnectAsync(ClientInfo request, ServerCallContext context)
    {
        var isGetOk = _clientQueue.TryGetValue(request.ClientId, out var client);
        if (!isGetOk)
        {
            return (new() { Message = "断开失败，未找到客户端ID", Successed = false });
        }
        await client!.CancellationTokenSource.CancelAsync();
        client.ConnectionState = CommonConnectionState.Disconnected;
        return new() { Message = "断开连接成功", Successed = true };
    }
    public override async Task<CommunicationResult> SendMessageAsync(CommunicationSendMessage request, ServerCallContext context)
    {
        var isGetOk = _clientQueue.TryGetValue(request.To, out var client);
        if (!isGetOk)
        {
            return new()
            {
                Message = "发送失败，未找到客户端",
                Successed = false
            };
        }
        await client!.ResponseStream.WriteAsync(request);
        return new()
        {
            Message = "发送成功",
            Successed = true
        };
    }
}
