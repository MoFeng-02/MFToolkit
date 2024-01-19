﻿using MFToolkit.CommonTypes.Enumerates;
using MFToolkit.SignalRs.Models;

namespace MFToolkit.SignalRs.Interfaces.ChatInterfaces;
public interface IChatMessageService
{
    /// <summary>
    /// 接收信息的方法
    /// </summary>
    /// <param name="from">发送者</param>
    /// <param name="to">接收者</param>
    /// <param name="message">信息</param>
    /// <param name="chatContactType"></param>
    /// <returns></returns>
    Task ReceiveMessage(ChatMessageModel message, ChatContactType chatContactType);
    /// <summary>
    /// 发送信息方法
    /// </summary>
    /// <param name="from">发送者</param>
    /// <param name="to">接收者</param>
    /// <param name="message">信息</param>
    /// <returns></returns>
    Task SendMessageAsync(ChatMessageModel message);
    Task A(string a);
}
