namespace MFToolkit.Socket.TCP.Server;
public class TCPMessage
{
    /// <summary>
    /// 发送用户
    /// </summary>
    public string SendUser { get; set; }
    /// <summary>
    /// 接收用户
    /// </summary>
    public string[] ReceiveUser { get; set; }
    /// <summary>
    /// 发送类型
    /// </summary>
    public TCPMessageReceive ReceiveType { get; set; }
    /// <summary>
    /// 序号
    /// </summary>
    public string SequenceNumber { get; set; }
    /// <summary>
    /// 信息体
    /// </summary>
    public byte[] Payload { get; set; }
    /// <summary>
    /// 可选的尾部信息
    /// </summary>
    public string Checksum { get; set; }
}
public enum TCPMessageReceive
{
    /// <summary>
    /// 单个接收
    /// </summary>
    Single,
    /// <summary>
    /// 多个接收
    /// </summary>
    Many,
    /// <summary>
    /// 群信息
    /// </summary>
    Group,
    /// <summary>
    /// 信息广播，一般是管理员发布公告或者什么的等等，通常是发布给所有人
    /// </summary>
    Broadcast
}