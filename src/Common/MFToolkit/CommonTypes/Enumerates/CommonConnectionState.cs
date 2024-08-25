namespace MFToolkit.CommonTypes.Enumerates;
/// <summary>
/// 连接状态
/// </summary>
public enum CommonConnectionState
{
    /// <summary>
    /// 断开连接
    /// </summary>
    Disconnected,
    /// <summary>
    /// 连接中
    /// </summary>
    Connecting,
    /// <summary>
    /// 已连接
    /// </summary>
    Connected,
    /// <summary>
    /// 重新连接
    /// </summary>
    Reconnection,
    /// <summary>
    /// 连接异常
    /// </summary>
    ConnectionException
}
