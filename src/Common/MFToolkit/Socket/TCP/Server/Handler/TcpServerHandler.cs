using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MFToolkit.Socket.TCP.Server.Handler;
public class TcpServerHandler
{
    private static readonly ConcurrentDictionary<string, TcpClient> clients = [];
    //public async Task HandlerClient(string clientId, StreamReader reader)
    //{
    //    while (true)
    //    {
    //        try
    //        {
    //            var message = await reader.ReadLineAsync();
    //            if (message == null)
    //            {
    //                clients.TryRemove(clientId);
    //                Console.WriteLine($"Client {clientId} disconnected.");
    //                break;
    //            }

    //            Console.WriteLine($"Received from {clientId}: {message}");
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Error: {ex.Message}");
    //        }
    //    }
    //}
    //public async Task<TcpClient?> GetTcpClientAsync(string key)
    //{

    //}
}
