namespace MFToolkit.Avaloniaui.Routes.Core.Exceptions;

/// <summary>
/// 路由异常基类
/// </summary>
public abstract class RouteException : Exception
{
    /// <summary>
    /// 路由路径
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    protected RouteException(string route, string message) : base(message)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
    }
}

/// <summary>
/// 路由未找到异常
/// </summary>
public class RouteNotFoundException : RouteException
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public RouteNotFoundException(string route) 
        : base(route, $"路由 '{route}' 未找到") { }
}

/// <summary>
/// 路由注册异常
/// </summary>
public class RouteRegistrationException : RouteException
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public RouteRegistrationException(string route, string message) 
        : base(route, message) { }
}

/// <summary>
/// 导航被阻止异常
/// </summary>
public class NavigationBlockedException : RouteException
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public NavigationBlockedException(string route, string reason) 
        : base(route, $"导航到路由 '{route}' 被阻止: {reason}") { }
}
    