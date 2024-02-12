using System.Collections.Concurrent;
using Avalonia.Controls;
using MFToolkit.Avaloniaui.BaseExtensions;

namespace MFToolkit.Avaloniaui.Routings;

/// <summary>
/// 基于Routing类的导航快捷操作
/// </summary>
public sealed class Navigation
{
    private static Action<object?, RouteCurrentInfo>? NavigationTo { get; set; }
    private static Action<object?, RouteCurrentInfo?>? RoutePage { get; set; }

    /// <summary>
    /// 缓存字符串路由的参数
    /// </summary>
    private static ConcurrentDictionary<string, Dictionary<string, object?>?> CacheStringRouteParameter = new();

    public static void SetRoutePageAction(Action<object?, RouteCurrentInfo> action)
    {
        NavigationTo = action;
    }

    /// <summary>
    /// 设置页面返回处理
    /// </summary>
    /// <param name="action"></param>
    public static void SetPageAction(Action<object?, RouteCurrentInfo> action)
    {
        NavigationTo = action;
    }

    /// <summary>
    /// 跳转页面
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    public static async Task<object?> GoToAsync(string route)
    {
        var page = await Routing.GoToAsync(route);
        if (page == null) return null;
        var parameters = new Dictionary<string, object?>();
        if (CacheStringRouteParameter.TryGetValue(page.Route!, out var query))
        {
            parameters = query;
        }

        if (page.CurrentPage is Control control)
        {
            var vm = control.DataContext as ViewModelBase;
            // 调用子类的接收参数方法
#if NET8_0_OR_GREATER
            vm?.ApplyQueryAttributes(page.Parameters ?? parameters ?? []);
#else
            vm?.ApplyQueryAttributes(page.Parameters ?? parameters ?? new());
#endif
        }

        if (page.CurrentPage is IQueryAttributable queryAttributable)
        {
            queryAttributable?.ApplyQueryAttributes(page.Parameters ?? parameters ?? new());
        }

        NavigationTo?.Invoke(page.CurrentPage, page);
        return page?.CurrentPage;
    }

    /// <summary>
    /// 根据类型来进行页面跳转
    /// </summary>
    /// <param name="route">页面路由</param>
    /// <param name="parameters">要传递给页面的参数</param>
    /// <returns></returns>
    public static async Task<object?> GoToAsync(string route, Dictionary<string, object?>? parameters)
    {
        var page = await Routing.GoToAsync(route);
        if (page == null) return null;
        if (!CacheStringRouteParameter.TryGetValue(route, out var query))
        {
            CacheStringRouteParameter.AddOrUpdate(route, parameters, (_, _) => parameters);
        }

        if (parameters?.Count == 0)
        {
            CacheStringRouteParameter.AddOrUpdate(route, parameters, (_, _) => parameters);
        }

        if (page.CurrentPage is Control control)
        {
            var vm = control.DataContext as ViewModelBase;
            // 调用子类的接收参数方法
#if NET8_0_OR_GREATER
            vm?.ApplyQueryAttributes(parameters ?? []);
#else
            vm?.ApplyQueryAttributes(parameters ?? new());
#endif
        }

        if (page.CurrentPage is IQueryAttributable queryAttributable)
        {
            queryAttributable.ApplyQueryAttributes(parameters ?? new());
        }

        NavigationTo?.Invoke(page.CurrentPage, page);
        return page?.CurrentPage;
    }

    /// <summary>
    /// 根据注册页面类型来进行页面跳转
    /// </summary>
    /// <param name="pageType">注册的页面类型</param>
    /// <param name="parameters">要传递给页面的参数</param>
    /// <returns></returns>
    public static async Task<object?> GoToAsync(Type pageType, Dictionary<string, object?>? parameters = null)
    {
        var route = Routing.PageTypeToRoute(pageType);
        if (string.IsNullOrWhiteSpace(route)) throw new("您的页面未找到");
        var page = await Routing.GoToAsync(route);
        if (page == null) return null;

        if (!CacheStringRouteParameter.TryGetValue(route, out var query))
        {
            CacheStringRouteParameter.AddOrUpdate(route, parameters, (_, _) => parameters);
        }

        if (parameters?.Count == 0)
        {
            CacheStringRouteParameter.AddOrUpdate(route, parameters, (_, _) => parameters);
        }

        if (page.CurrentPage is Control control)
        {
            var vm = control.DataContext as ViewModelBase;
            // 调用子类的接收参数方法
#if NET8_0_OR_GREATER
            vm?.ApplyQueryAttributes(parameters ?? []);
#else
            vm?.ApplyQueryAttributes(parameters ?? new());
#endif
        }

        if (page.CurrentPage is IQueryAttributable queryAttributable)
        {
            queryAttributable.ApplyQueryAttributes(parameters ?? new());
        }

        NavigationTo?.Invoke(page.CurrentPage, page);
        return page?.CurrentPage;
    }

    /// <summary>
    /// 获取是否还有上一页
    /// </summary>
    /// <returns></returns>
    public static bool GetPrevRouting() => Routing.GetPrevRouting();
}