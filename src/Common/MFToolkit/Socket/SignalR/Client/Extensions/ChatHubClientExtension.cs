using MFToolkit.CommonTypes.Enumerates;
using MFToolkit.Socket.SignalR.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace MFToolkit.Socket.SignalR.Client.Extensions;
public static class ChatHubClientExtension
{
    /// <summary>
    /// 启动自动重连
    /// </summary>
    /// <param name="connection">连接实例</param>
    /// <param name="connectionAction">重连时的状态
    /// <para>int: 连接次数</para>
    /// <para>HubConnectionState?: 连接状态</para>
    /// </param>
    /// <returns></returns>
    public static HubConnection Reconnection(this HubConnection connection, Action<int, HubConnectionState?> connectionAction = null)
    {
        // 如果已经启动自动重连
        if (ChatHubClient.ToIsStartReconnection()) return connection;
        connection.Closed += async (state) =>
        {
            await Console.Out.WriteLineAsync("尝试重连");
            await ChatHubClient.Reconnect(connectionAction);
        };
        return connection;
    }
    /// <summary>
    /// 注册接收信息
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static HubConnection ReceiveMessage(this HubConnection connection, Action<ChatMessageModel, CommunicationContactType> action)
    {
        ChatHubClient.ReceiveMessage = action;
        return connection;
    }
}
