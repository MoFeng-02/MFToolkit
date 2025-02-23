using Avalonia.Controls;
using MFToolkit.Avaloniaui.BaseExtensions;
using MFToolkit.Avaloniaui.Routings.Helpers;

namespace MFToolkit.Avaloniaui.Routings;

/// <summary>
/// 导航服务（适配新版路由系统）
/// </summary>
public sealed class Navigation
{
    public static object? CurrentPage { get; private set; }

    private static Action<object?, RouteCurrentInfo>? NavigationTo { get; set; }

    /// <summary>
    /// 设置全局导航回调
    /// </summary>
    public static void SetRoutePageAction(Action<object?, RouteCurrentInfo> action)
    {
        NavigationTo = action;
    }

    /// <summary>
    /// 导航到指定路由（异步）
    /// </summary>
    /// <param name="route">路由路径（支持参数化路由和查询参数）</param>
    /// <returns>页面实例</returns>
    public static async Task<object?> GoToAsync(string route)
    {
        var page = await Routing.GoToAsync(route);
        return HandleNavigationResult(page);
    }

    /// <summary>
    /// 带参数导航（推荐使用）
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <param name="parameters">路由参数（自动合并路径参数和查询参数）</param>
    public static async Task<object?> GoToAsync(string route, Dictionary<string, object?> parameters)
    {
        // 合并路径参数到查询参数
        var mergedRoute = MergeRouteParameters(route, parameters);
        var page = await Routing.GoToAsync(mergedRoute);
        return HandleNavigationResult(page);
    }

    /// <summary>
    /// 通过页面类型导航
    /// </summary>
    public static async Task<object?> GoToAsync(Type pageType, Dictionary<string, object?> parameters = null!)
    {
        var route = Routing.PageTypeToRoute(pageType) ?? throw new Exception("未找到对应路由");
        return await GoToAsync(route, parameters);
    }


    /// <summary>
    /// 通过页面类型导航
    /// </summary>
    public static async Task<object?> GoToAsync<TType>(Dictionary<string, object?> parameters = null!)
    {
        var route = Routing.PageTypeToRoute(typeof(TType)) ?? throw new Exception("未找到对应路由");
        return await GoToAsync(route, parameters);
    }



    /// <summary>
    /// 处理导航结果
    /// </summary>
    private static object? HandleNavigationResult(RouteCurrentInfo? page)
    {
        if (page == null) return null;

        // 参数处理（新版路由系统已内置参数缓存）
        if (page.CurrentPage is Control control)
        {
            HandleViewModelParameters(control, page.Parameters);
        }

        if (page.CurrentPage is IQueryAttributable queryAttributable)
        {
            queryAttributable.ApplyQueryAttributes(page.Parameters ?? new());
        }

        // 触发全局导航事件
        NavigationTo?.Invoke(page.CurrentPage, page);
        CurrentPage = page.CurrentPage;
        return page.CurrentPage;
    }

    /// <summary>
    /// 处理ViewModel参数
    /// </summary>
    private static void HandleViewModelParameters(Control control, Dictionary<string, object?>? parameters)
    {
        if (control.DataContext is ViewModelBase vm)
        {
            // 增强类型转换
            var convertedParams = ConvertParameters(parameters);
            vm.ApplyQueryAttributes(convertedParams);
        }
    }

    /// <summary>
    /// 参数类型转换（字符串→实际类型）
    /// </summary>
    private static Dictionary<string, object?> ConvertParameters(Dictionary<string, object?>? parameters)
    {
        var result = new Dictionary<string, object?>();
        if (parameters == null) return result;

        foreach (var (key, value) in parameters)
        {
            result[key] = value switch
            {
                string str when int.TryParse(str, out var intVal) => intVal,
                string str when bool.TryParse(str, out var boolVal) => boolVal,
                string str when decimal.TryParse(str, out var decimalVal) => decimalVal,
                _ => value
            };
        }
        return result;
    }

    /// <summary>
    /// 合并路径参数到路由字符串
    /// </summary>
    private static string MergeRouteParameters(string route, Dictionary<string, object?> parameters)
    {
        var (path, query) = SplitRoute(route);
        var existingParams = QueryParser.Parse(query);

        // 合并参数（路径参数优先）
        var mergedParams = existingParams
            .Concat(parameters ?? [])
            .ToDictionary(p => p.Key, p => p.Value);

        // 构建查询字符串
        var queryString = string.Join("&",
            mergedParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value?.ToString() ?? "")}"));

        return string.IsNullOrEmpty(queryString) ? path : $"{path}?{queryString}";
    }

    /// <summary>
    /// 分离路径和查询参数
    /// </summary>
    private static (string Path, string Query) SplitRoute(string route)
    {
        var queryIndex = route.IndexOf('?');
        return queryIndex == -1
            ? (route, string.Empty)
            : (route[..queryIndex], route[(queryIndex + 1)..]);
    }

    /// <summary>
    /// 获取导航历史状态
    /// </summary>
    public static bool GetPrevRouting() => Routing.GetPrevRouting();
}