using Common;
using Grpc.Core;
using MFToolkit.CommonTypes.Enumerates;

namespace MFToolkit.Socket.Grpc.GrpcManagers;
public sealed class ClientInfoQueue
{
    public string CID { get; set; } = null!;
    public DateTime LastConnect { get; set; } = DateTime.Now;
    public DateTime LastDisconnect { get; set; } = DateTime.Now;
    public CommonConnectionState ConnectionState { get; set; }
    public CancellationTokenSource CancellationTokenSource { get; set; } = new();
    public IAsyncStreamReader<ClientInfo> RequestStream { get; set; } = null!;
    public IServerStreamWriter<CommunicationSendMessage> ResponseStream { get; set; } = null!;
}
