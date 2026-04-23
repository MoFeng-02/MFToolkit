using MFToolkit.Routing.Entities;

namespace MFToolkit.Routing;

/// <summary>
/// 导航结果，表示一次导航操作的结果
/// </summary>
public class NavigationResult
{
    /// <summary>
    /// 导航状态
    /// </summary>
    public NavigationStatus Status { get; }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// 错误信息（如果状态为 ErrorEx）
    /// </summary>
    public Exception? ErrorEx { get; }

    /// <summary>
    /// 目标路由实体
    /// </summary>
    public RouteEntity? TargetRoute { get; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => Status == NavigationStatus.Success;

    /// <summary>
    /// 是否被取消
    /// </summary>
    public bool IsCancelled => Status == NavigationStatus.Cancelled;

    /// <summary>
    /// 是否被阻止
    /// </summary>
    public bool IsBlocked => Status == NavigationStatus.Blocked;

    /// <summary>
    /// 是否未找到
    /// </summary>
    public bool IsNotFound => Status == NavigationStatus.NotFound;

    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool IsError => Status == NavigationStatus.Error;

    /// <inheritdoc/>
    private NavigationResult(NavigationStatus status, string? message, Exception? error, RouteEntity? targetRoute)
    {
        Status = status;
        Message = message;
        ErrorEx = error;
        TargetRoute = targetRoute;
    }

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static NavigationResult Success(RouteEntity targetRoute, string? message = null)
        => new(NavigationStatus.Success, message, null, targetRoute);

    /// <summary>
    /// 创建取消结果
    /// </summary>
    public static NavigationResult Cancelled(string? message = null)
        => new(NavigationStatus.Cancelled, message, null, null);

    /// <summary>
    /// 创建被阻止结果
    /// </summary>
    public static NavigationResult Blocked(RouteEntity targetRoute, string? message = null)
        => new(NavigationStatus.Blocked, message ?? "导航被守卫阻止", null, targetRoute);

    /// <summary>
    /// 创建未找到结果
    /// </summary>
    public static NavigationResult NotFound(string routeKey, string? message = null)
        => new(NavigationStatus.NotFound, message ?? $"路由未找到: {routeKey}", null, null);

    /// <summary>
    /// 创建错误结果
    /// </summary>
    public static NavigationResult Error(Exception exception, string? message = null)
        => new(NavigationStatus.Error, message ?? exception.Message, exception, null);
}
