namespace MFToolkit.Avaloniaui.Routes;

/// <summary>
/// 拓展跳转方法
/// </summary>
public partial class Navigation
{
    #region Push 追加路由

    /// <summary>
    /// 追加跳转到指定路由（默认导航行为）
    /// </summary>
    /// <param name="route">目标路由路径（例如 "/home"）</param>
    /// <returns>导航后的页面实例（可能为 null）</returns>
    public static Task<object?> PushAsync(string route) => GoToAsync(route);

    /// <summary>
    /// 带参数追加跳转到指定路由
    /// </summary>
    /// <param name="route">目标路由路径</param>
    /// <param name="parameters">路由参数（自动合并到查询字符串）</param>
    /// <returns>导航后的页面实例</returns>
    public static Task<object?> PushAsync(string route, Dictionary<string, object?> parameters) =>
        GoToAsync(route, parameters);

    /// <summary>
    /// 通过页面类型追加跳转（自动推导路由路径）
    /// </summary>
    /// <param name="pageType">目标页面类型（需提前注册路由）</param>
    /// <param name="parameters">可选的路由参数</param>
    /// <returns>导航后的页面实例</returns>
    /// <exception cref="InvalidOperationException">当页面类型未注册路由时抛出</exception>
    public static Task<object?> PushAsync(Type pageType, Dictionary<string, object?> parameters = null!) =>
        GoToAsync(pageType, parameters);

    /// <summary>
    /// 泛型版本页面类型追加跳转
    /// </summary>
    /// <typeparam name="TPage">目标页面类型</typeparam>
    /// <param name="parameters">可选的路由参数</param>
    /// <returns>导航后的页面实例</returns>
    public static Task<object?> PushAsync<TPage>(Dictionary<string, object?> parameters = null!) =>
        GoToAsync<TPage>(parameters);

    #endregion

    #region Replace 替换路由

    /// <summary>
    /// 替换当前页面到指定路由（导航堆栈中替换当前页）
    /// </summary>
    /// <param name="route">目标路由路径</param>
    /// <returns>导航后的页面实例</returns>
    public static async Task<object?> ReplaceAsync(string route)
    {
        var page = await Routing.ReplaceAsync(route);
        return HandleNavigationResult(page);
    }

    /// <summary>
    /// 带参数替换当前页面
    /// </summary>
    /// <param name="route">目标路由路径</param>
    /// <param name="parameters">路由参数</param>
    /// <returns>导航后的页面实例</returns>
    public static async Task<object?> ReplaceAsync(string route, Dictionary<string, object?> parameters)
    {
        // 合并路径参数到查询参数
        var mergedRoute = MergeRouteParameters(route, parameters);
        var page = await Routing.ReplaceAsync(mergedRoute);
        return HandleNavigationResult(page);
    }

    /// <summary>
    /// 通过页面类型替换当前页面
    /// </summary>
    /// <param name="pageType">目标页面类型</param>
    /// <param name="parameters">可选的路由参数</param>
    /// <returns>导航后的页面实例</returns>
    public static async Task<object?> ReplaceAsync(Type pageType, Dictionary<string, object?> parameters = null!)
    {
        var route = Routing.PageTypeToRoute(pageType) ?? throw new Exception("未找到对应路由");
        return await ReplaceAsync(route, parameters);
    }

    /// <summary>
    /// 泛型版本页面类型替换
    /// </summary>
    /// <typeparam name="TPage">目标页面类型</typeparam>
    /// <param name="parameters">可选的路由参数</param>
    /// <returns>导航后的页面实例</returns>
    public static async Task<object?> ReplaceAsync<TPage>(Dictionary<string, object?> parameters = null!)
    {
        var route = Routing.PageTypeToRoute(typeof(TPage)) ?? throw new Exception("未找到对应路由");
        return await ReplaceAsync(route, parameters);
    }

    #endregion

    #region Pop 返回操作

    /// <summary>
    /// 返回上一页（导航堆栈弹出当前页）
    /// </summary>
    /// <returns>无返回页面（通常为 null）</returns>
    public static Task<object?> PopAsync() => GoToAsync("..");

    /// <summary>
    /// 返回到导航堆栈的根页面（清空堆栈）
    /// </summary>
    /// <returns>根页面实例</returns>
    public static Task<object?> PopToRootAsync()
        => GoToAsync("//");

    #endregion
}