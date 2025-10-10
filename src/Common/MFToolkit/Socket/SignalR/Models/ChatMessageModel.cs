#nullable disable
namespace MFToolkit.Socket.SignalR.Models;
public class ChatMessageModel
{
    /// <summary>
    /// 发送人
    /// </summary>
    public string From { get; set; }
    /// <summary>
    /// 接收者
    /// </summary>
    public string To { get; set; }
    /// <summary>
    /// 内容
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTimeOffset SendTime { get; set; } = DateTimeOffset.UtcNow;
}
