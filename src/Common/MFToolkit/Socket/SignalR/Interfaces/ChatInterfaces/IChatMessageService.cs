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
    Task ReceiveMessage(ChatMessageModel message, CommunicationContactType chatContactType);
    /// <summary>
    /// 发送信息方法
    /// </summary>
    /// <param name="message">信息</param>
    /// <returns></returns>
    Task SendMessageAsync(ChatMessageModel message);


    /// <summary>
    /// 接收组信息的方法
    /// </summary>
    /// <param name="message">信息</param>
    /// <returns></returns>
    Task ReceiveGroupMessage(ChatMessageModel message);
    /// <summary>
    /// 发送组信息方法
    /// </summary>
    /// <param name="message">信息</param>
    /// <returns></returns>
    Task SendGroupMessageAsync(ChatMessageModel message);


}
