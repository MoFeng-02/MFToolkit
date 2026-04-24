using MFToolkit.Routing.Entities;

namespace MFToolkit.Routing.Core.Interfaces;

/// <summary>
/// 路由导航接口
/// </summary>
public interface IRouter
{
    // === 事件通知（UI 框架订阅） ===

    /// <summary>
    /// 导航开始事件
    /// </summary>
    event EventHandler<NavigationEventArgs>? NavigationStarting;

    /// <summary>
    /// 导航完成事件
    /// </summary>
    event EventHandler<NavigationEventArgs>? Navigated;

    /// <summary>
    /// 导航失败事件
    /// </summary>
    event EventHandler<NavigationEventArgs>? NavigationFailed;

    // === 导航 ===

    /// <summary>
    /// 导航到指定路由
    /// </summary>
    /// <param name="routeKey">路由键</param>
    /// <param name="parameters">导航参数</param>
    Task<NavigationResult> NavigateAsync(string routeKey, Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// 导航到指定页面类型
    /// </summary>
    /// <param name="pageType">页面类型</param>
    /// <param name="parameters">导航参数</param>
    Task<NavigationResult> NavigateAsync(Type pageType, Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// 导航到指定页面类型（泛型版本）
    /// </summary>
    /// <typeparam name="T">页面类型</typeparam>
    /// <param name="parameters">导航参数</param>
    Task<NavigationResult> NavigateAsync<T>(Dictionary<string, object?>? parameters = null) where T : class;

    // === 后退 ===

    /// <summary>
    /// 返回上一页
    /// </summary>
    Task<NavigationResult> GoBackAsync();

    /// <summary>
    /// 返回到栈顶（清空栈）
    /// </summary>
    Task<NavigationResult> GoBackToRootAsync();

    // === 当前状态 ===

    /// <summary>
    /// 当前顶级路由ID
    /// </summary>
    Guid CurrentTopRouteId { get; }

    /// <summary>
    /// 当前路由条目
    /// </summary>
    RouteEntry? CurrentRoute { get; }

    /// <summary>
    /// 当前栈
    /// </summary>
    IReadOnlyList<RouteEntry> CurrentStack { get; }

    /// <summary>
    /// 是否可以返回
    /// </summary>
    bool CanGoBack { get; }

    // === 栈管理 ===

    /// <summary>
    /// 切换顶级路由
    /// </summary>
    void SwitchTopRoute(Guid topRouteId);

    /// <summary>
    /// 用户注册的顶级路由列表
    /// </summary>
    IReadOnlyList<RouteEntity> RegisteredTopRoutes { get; }

    /// <summary>
    /// 栈深度
    /// </summary>
    int StackDepth { get; }

    // === 扩展导航 ===

    /// <summary>
    /// 返回到指定路由（不包括该路由本身）
    /// </summary>
    Task<NavigationResult> GoBackToAsync(string routeKey);

    /// <summary>
    /// 返回到指定页面类型（不包括该页面本身）
    /// </summary>
    Task<NavigationResult> GoBackToAsync(Type pageType);

    /// <summary>
    /// 返回到指定页面类型（不包括该页面本身，泛型版本）
    /// </summary>
    Task<NavigationResult> GoBackToAsync<T>() where T : class;

    /// <summary>
    /// 替换当前页面
    /// </summary>
    Task<NavigationResult> ReplaceAsync(string routeKey, Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// 替换当前页面
    /// </summary>
    Task<NavigationResult> ReplaceAsync(Type pageType, Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// 替换当前页面（泛型版本）
    /// </summary>
    Task<NavigationResult> ReplaceAsync<T>(Dictionary<string, object?>? parameters = null) where T : class;

}
