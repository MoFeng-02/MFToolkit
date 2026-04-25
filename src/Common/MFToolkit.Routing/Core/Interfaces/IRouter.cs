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
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.Push"/>）</param>
    Task<NavigationResult> NavigateAsync(string routeKey, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push);

    /// <summary>
    /// 导航到指定页面类型
    /// </summary>
    /// <param name="pageType">页面类型</param>
    /// <param name="parameters">导航参数</param>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.Push"/>）</param>
    Task<NavigationResult> NavigateAsync(Type pageType, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push);

    /// <summary>
    /// 导航到指定页面类型（泛型版本）
    /// </summary>
    /// <typeparam name="T">页面类型</typeparam>
    /// <param name="parameters">导航参数</param>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.Push"/>）</param>
    Task<NavigationResult> NavigateAsync<T>(Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push) where T : class;

    // === 后退 ===

    /// <summary>
    /// 返回上一页
    /// </summary>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.Pop"/>）</param>
    Task<NavigationResult> GoBackAsync(string action = NavigationActions.Pop);

    /// <summary>
    /// 返回到栈顶（清空栈）
    /// </summary>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.PopToRoot"/>）</param>
    Task<NavigationResult> GoBackToRootAsync(string action = NavigationActions.PopToRoot);

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
    /// 返回到指定路由
    /// </summary>
    /// <param name="routeKey">目标路由键</param>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.PopToPage"/>）</param>
    Task<NavigationResult> GoBackToAsync(string routeKey, string action = NavigationActions.PopToPage);

    /// <summary>
    /// 返回到指定页面类型
    /// </summary>
    /// <param name="pageType">目标页面类型</param>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.PopToPage"/>）</param>
    Task<NavigationResult> GoBackToAsync(Type pageType, string action = NavigationActions.PopToPage);

    /// <summary>
    /// 返回到指定页面类型（泛型版本）
    /// </summary>
    /// <typeparam name="T">目标页面类型</typeparam>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.PopToPage"/>）</param>
    Task<NavigationResult> GoBackToAsync<T>(string action = NavigationActions.PopToPage) where T : class;

    /// <summary>
    /// 替换当前页面
    /// </summary>
    /// <param name="routeKey">路由键</param>
    /// <param name="parameters">导航参数</param>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.Replace"/>）</param>
    Task<NavigationResult> ReplaceAsync(string routeKey, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace);

    /// <summary>
    /// 替换当前页面
    /// </summary>
    /// <param name="pageType">页面类型</param>
    /// <param name="parameters">导航参数</param>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.Replace"/>）</param>
    Task<NavigationResult> ReplaceAsync(Type pageType, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace);

    /// <summary>
    /// 替换当前页面（泛型版本）
    /// </summary>
    /// <typeparam name="T">页面类型</typeparam>
    /// <param name="parameters">导航参数</param>
    /// <param name="action">导航动作类型（默认 <see cref="NavigationActions.Replace"/>）</param>
    Task<NavigationResult> ReplaceAsync<T>(Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace) where T : class;

}
