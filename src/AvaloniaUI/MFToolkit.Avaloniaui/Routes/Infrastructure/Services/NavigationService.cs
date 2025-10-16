using MFToolkit.Avaloniaui.Routes.Core.Entities;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;

namespace MFToolkit.Avaloniaui.Routes.Infrastructure.Services;

/// <summary>
/// 导航服务实现类，提供上层导航功能
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IRoutingService _routingService;
    private static NavigationService? _defaultInstance;

    /// <summary>
    /// 静态默认实例，用于静态调用方式
    /// </summary>
    public static NavigationService Default
    {
        get
        {
            if (_defaultInstance == null)
            {
                // 创建默认实例，使用RoutingService的默认实例
                _defaultInstance = new NavigationService(RoutingService.Default);
            }

            return _defaultInstance;
        }
    }

    /// <summary>
    /// 构造函数（用于依赖注入）
    /// </summary>
    /// <param name="routingService">路由服务，不可为null</param>
    public NavigationService(IRoutingService routingService)
    {
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
    }

    /// <summary>
    /// 导航事件（值是页面）
    /// </summary>
    public Action<object?, RouteCurrentInfo?>? NavigationCompleted { get; set; }

    /// <summary>
    /// 推送新页面到导航栈
    /// </summary>
    /// <param name="route">目标路由</param>
    /// <param name="parameters">路由参数，可为null</param>
    /// <returns>页面实例，可为null</returns>
    public async Task<object?> GoToAsync(string route, Dictionary<string, object?>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return null;
        }

        var routeInfo = await _routingService.GoToAsync(route, parameters);
        NavigationCompleted?.Invoke(routeInfo?.Page, routeInfo);
        return routeInfo?.Page;
    }

    /// <summary>
    /// 推送新页面到导航栈
    /// </summary>
    public async Task<object?> GoToAsync<TPage>(Dictionary<string, object?>? parameters = null)
    {
        var route = _routingService.PageTypeToRoute(typeof(TPage)) ?? throw new Exception("Page type not registered");
        var routeInfo = await _routingService.GoToAsync(route, parameters);
        NavigationCompleted?.Invoke(routeInfo?.Page, routeInfo);
        return routeInfo?.Page;
    }

    /// <summary>
    /// 推送新页面到导航栈
    /// </summary>
    public async Task<object?> GoToAsync(Type pageType, Dictionary<string, object?>? parameters = null)
    {
        var route = _routingService.PageTypeToRoute(pageType) ?? throw new Exception("Page type not registered");
        var routeInfo = await _routingService.GoToAsync(route, parameters);
        NavigationCompleted?.Invoke(routeInfo?.Page, routeInfo);
        return routeInfo?.Page;
    }

    /// <summary>
    /// 替换当前页面
    /// </summary>
    /// <param name="route">目标路由</param>
    /// <param name="parameters">路由参数，可为null</param>
    /// <returns>页面实例，可为null</returns>
    public async Task<object?> ReplaceAsync(string route, Dictionary<string, object?>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return null;
        }

        var routeInfo = await _routingService.ReplaceAsync(route, parameters);
        NavigationCompleted?.Invoke(routeInfo?.Page, routeInfo);
        return routeInfo?.Page;
    }

    /// <summary>
    /// 返回上一页
    /// </summary>
    /// <returns>页面实例，可为null</returns>
    public async Task<object?> GoBackAsync()
    {
        var routeInfo = await _routingService.GoBackAsync();
        NavigationCompleted?.Invoke(routeInfo?.Page, routeInfo);
        return routeInfo?.Page;
    }

    /// <summary>
    /// 返回根页面
    /// </summary>
    /// <returns>页面实例，可为null</returns>
    public async Task<object?> GoToRootAsync()
    {
        var routeInfo = await _routingService.GoToRootAsync();
        NavigationCompleted?.Invoke(routeInfo?.Page, routeInfo);
        return routeInfo?.Page;
    }

    /// <summary>
    /// 获取当前页面
    /// </summary>
    /// <returns>页面实例，可为null</returns>
    public object? GetCurrentPage()
    {
        return _routingService.GetCurrentRouteInfo()?.Page;
    }

    /// <summary>
    /// 获取当前视图模型
    /// </summary>
    /// <returns>视图模型实例，可为null（如果没有视图模型）</returns>
    public object? GetCurrentViewModel()
    {
        return _routingService.GetCurrentRouteInfo()?.ViewModel;
    }
}