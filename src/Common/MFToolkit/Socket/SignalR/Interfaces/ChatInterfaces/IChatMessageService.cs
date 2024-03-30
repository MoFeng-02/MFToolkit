using MFToolkit.CommonTypes.Enumerates;
using MFToolkit.Socket.SignalR.Models;

namespace MFToolkit.Socket.SignalR.Interfaces.ChatInterfaces;
public interface IChatMessageService
{
    /// <summary>
    /// 接收信息的方法
    /// </summary>
    /// <param name="message">信息</param>
    /// <param name="chatContactType"></param>
    /// <returns></returns>
    Task ReceiveMessage(ChatMessageModel message, ChatContactType chatContactType);
    /// <summary>
    /// 发送信息方法
    /// </summary>
    /// <param name="message">信息</param>
    /// <returns></returns>
    Task SendMessageAsync(ChatMessageModel message);
}
