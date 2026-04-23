using MFToolkit.Routing.Entities;

namespace MFToolkit.Routing;

/// <summary>
/// 导航事件参数，用于 Router 事件通知
/// </summary>
public class NavigationEventArgs : EventArgs
{
    /// <summary>
    /// 来源路由（上一个路由）
    /// </summary>
    public RouteEntry? From { get; }

    /// <summary>
    /// 目标路由（即将导航到的路由）
    /// </summary>
    public RouteEntry? To { get; }

    /// <summary>
    /// 导航状态
    /// </summary>
    public NavigationStatus Status { get; }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// 导航参数
    /// </summary>
    public Dictionary<string, object?>? Parameters { get; }

    /// <inheritdoc/>
    public NavigationEventArgs(RouteEntry? from, RouteEntry? to, NavigationStatus status, string? message = null, Dictionary<string, object?>? parameters = null)
    {
        From = from;
        To = to;
        Status = status;
        Message = message;
        Parameters = parameters;
    }

    /// <summary>
    /// 创建导航开始事件参数
    /// </summary>
    public static NavigationEventArgs Starting(RouteEntry? from, RouteEntry? to, Dictionary<string, object?>? parameters = null)
        => new(from, to, NavigationStatus.Success, null, parameters);

    /// <summary>
    /// 创建导航成功事件参数
    /// </summary>
    public static NavigationEventArgs Navigated(RouteEntry? from, RouteEntry? to)
        => new(from, to, NavigationStatus.Success);

    /// <summary>
    /// 创建导航失败事件参数
    /// </summary>
    public static NavigationEventArgs Failed(RouteEntry? from, RouteEntry? to, NavigationStatus status, string? message)
        => new(from, to, status, message);
}
