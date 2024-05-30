using MFToolkit.CommonTypes.Enumerates;
using MFToolkit.Socket.SignalR.Interfaces.ChatInterfaces;
using MFToolkit.Socket.SignalR.Models;
using Microsoft.AspNetCore.SignalR;

namespace MFToolkit.Socket.SignalR.Server;
/// <summary>
/// 基本处理程序，请自行拓展
/// <para>https://learn.microsoft.com/zh-cn/aspnet/core/signalr/hubs?view=aspnetcore-8.0</para>
/// </summary>
public class ChatHubServer : Hub<IChatMessageService>, IChatMessageService
{

    public async Task ReceiveMessage(ChatMessageModel message, ChatContactType chatContactType)
    {
        await Clients.User(message.To).ReceiveMessage(message, chatContactType);
    }
    public async Task ReceiveGroupMessage(ChatMessageModel message)
    {
        await Clients.Group(message.To).ReceiveGroupMessage(message);
    }


    public async Task SendMessageAsync(ChatMessageModel message)
    {
        await Clients.User(message.To).SendMessageAsync(message);
    }
    public async Task SendGroupMessageAsync(ChatMessageModel message)
    {
        await Clients.Group(message.To).SendMessageAsync(message);
    }
}
