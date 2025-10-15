using System.Collections.Concurrent;
using MFToolkit.Avaloniaui.Routes.Extensions;
using MFToolkit.Avaloniaui.Routes.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Avaloniaui.Routes;

/// <summary>
/// 路由类
/// </summary>
public sealed class Routing
{
    /// <summary>
    /// 当前顶级导航ID
    /// </summary>
    private static Guid _thisTopNavigationId = Guid.Empty;

    /// <summary>
    /// DI 服务提供
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; internal set; }

    /// <summary>
    /// 当前路由
    /// </summary>
    private static string? _thisRoute;

    private static RouteCurrentInfo? CurrentInfo { get; set; }

    /// <summary>
    /// 路由集合的顶级ID
    /// </summary>
    private static readonly ConcurrentDictionary<string, RouteCurrentInfo> TopNavigations = [];

    /// <summary>
    /// 路由集合
    /// Guid: RoutingId
    /// List: RouteInfos
    /// </summary>
    private static readonly ConcurrentDictionary<Guid, List<RouteCurrentInfo>> NavigationRoutes = [];

    /// <summary>
    /// 新增缓存管理
    /// </summary>
    private static readonly KeepAliveCache KeepAliveCache = new();

    /// <summary>
    /// 分割处理符号
    /// </summary>
    private static readonly char[] Separator = ['/'];

    /// <summary>
    /// 路由信息
    /// </summary>
    private static List<RoutingModel> RoutingModels { get; } = [];

    /// <summary>
    /// 分隔符数组
    /// </summary>
    private static readonly char[] SeparatorArray = ['?'];

    /// <summary>
    /// 获取路由详情信息
    /// </summary>
    /// <returns></returns>
    public static List<RoutingModel> GetRoutingModels() => RoutingModels;

    /// <summary>
    /// 获取当前路由信息
    /// </summary>
    public static RouteCurrentInfo? DefaultCurrentInfo => CurrentInfo;

    /// <summary>
    /// 注册路由
    /// <para>说明：若IsTopNavigation为true，则IsKeepAlive强制为True</para>
    /// </summary>
    /// <param name="routings"></param>
    /// <exception cref="Exception"></exception>
    public static void RegisterRoutes(params RoutingModel[] routings)
    {
        foreach (var item in routings.OrderByDescending(q => q.Priority))
        {
            if (RoutingModels.Any(q => q.Route == item.Route))
                throw new Exception($"路由已存在: {item.Route}");
            if (item.IsTopNavigation) item.IsKeepAlive = true;
            RoutingModels.Add(item);
        }
    }

    /// <summary>
    /// 注册路由
    /// <para>说明：若IsTopNavigation为true，则IsKeepAlive强制为True</para>
    /// </summary>
    /// <param name="pageType">页面</param>
    /// <param name="route">路由</param>
    /// <param name="isTopNavigation">是否顶级菜单页</param>
    /// <param name="isKeepAlive">是否保活页</param>
    /// <param name="priority">页面优先级（排序）</param>
    /// <param name="meta">Meta</param>
    public static void RegisterRoute(Type pageType, string? route = null, bool isTopNavigation =
        false, bool isKeepAlive = false, int priority = 0, RoutingMeta? meta = null)
    {
        // 如果路由为空，则设置随机路由
        route ??= Guid.NewGuid().ToString();
        if (RoutingModels.Any(q => q.Route == route))
            throw new Exception($"Route already exists: {route}");
        if (isTopNavigation) isKeepAlive = true;
        RoutingModels.Add(new RoutingModel(pageType, route, isKeepAlive, isTopNavigation, priority, meta));

        // 按优先级排序
        RoutingModels.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    /// <summary>
    /// 根据路由注册的页面类型来获取路由
    /// <para>通常用于未设置路由而随机生成路由的类</para>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string? PageTypeToRoute(Type type)
    {
        var query = RoutingModels.FirstOrDefault(q => q.PageType == type);
        return query?.Route;
    }

    /// <summary>
    /// 获取上一页的信息
    /// </summary>
    /// <returns></returns>
    public static async Task<RouteCurrentInfo?> PrevPageAsync()
    {
        return await PrevRoutingAsync();
    }

    /// <summary>
    /// 获取路径后面的参数
    /// </summary>
    /// <param name="queryString">路由参数</param>
    /// <returns></returns>
    private static Dictionary<string, object?> QueryParameter(string? queryString)
    {
        var parameters = new Dictionary<string, object?>();
        if (string.IsNullOrWhiteSpace(queryString)) return parameters;

        var querySpan = queryString.AsSpan();
        while (querySpan.TrySplit('&', out var pairSpan, out querySpan))
        {
            if (pairSpan.TrySplit('=', out var keySpan, out var valueSpan))
            {
                var key = Uri.UnescapeDataString(keySpan.ToString());
                var value = valueSpan.IsEmpty ? null : Uri.UnescapeDataString(valueSpan.ToString());
                parameters[key] = value;
            }
        }

        return parameters;
    }
    // private static Dictionary<string, object?> QueryParameter(string? queryString)
    // {
    //     // 检查查询字符串是否为空或仅包含空白字符
    //     if (string.IsNullOrWhiteSpace(queryString)) return [];
    //
    //     // 将查询字符串拆分为键值对数组，并创建字典
    //     var queryPairs = queryString.Split('&')
    //         .Select(pair => pair.Split('='))
    //         .ToDictionary(
    //             // 键是已解码的键部分
    //             pair => Uri.UnescapeDataString(pair[0]),
    //             // 值是已解码的值部分，如果值不存在，则设为 null
    //             pair => pair.Length > 1 ? (object)Uri.UnescapeDataString(pair[1]) : default
    //         );
    //
    //     // 返回包含解析后键值对的字典
    //     return queryPairs;
    // }

    /// <summary>
    /// 路由解析
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static RoutePathParameter ParsePath(string path)
    {
        var isOkRoute = true;
        var thisStr = string.Empty;
        RoutePathParameter routePathParameter = new();
        var parts = path.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        //var queryParts = parts.Last().Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries);

        var result = new List<string>();
        foreach (var part in parts)
        {
            if (thisStr != string.Empty && thisStr != ".." && part == "..")
            {
                isOkRoute = false;
                break;
            }

            thisStr = part;
            if (part.Contains('?'))
            {
                var subParts = part.Split(SeparatorArray, StringSplitOptions.RemoveEmptyEntries);
                result.AddRange(subParts.Take(1));
                if (subParts.Length > 1)
                {
                    //result.AddRange(subParts.Skip(1).SelectMany(p => p.Split(new[] { '&' })));
                    routePathParameter.OriginParameter = subParts.Skip(1).FirstOrDefault();
                    routePathParameter.Parameters = QueryParameter(subParts.Skip(1).FirstOrDefault());
                }
            }
            else
            {
                result.Add(part);
            }
        }

        if (!isOkRoute) routePathParameter.ErrorMessage = $"Route error: {path}";
        routePathParameter.IsRouteOk = isOkRoute;
        routePathParameter.Routes = result;
        return routePathParameter;
    }

    /// <summary>
    /// 获取是否还有上一页
    /// </summary>
    /// <returns></returns>
    internal static bool GetPrevRouting()
    {
        if (!NavigationRoutes.TryGetValue(_thisTopNavigationId, out var navigations)) return false;
        return navigations.Count > 1;
    }

    /// <summary>
    /// 返回上一页
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    private static async Task<RouteCurrentInfo?> PrevToAsync(string route)
    {
        var routePathParameter = ParsePath(route);
        RouteCurrentInfo? prev = null;
        var reRoute = string.Empty;
        foreach (var item in routePathParameter.Routes)
            if (item == "..")
                prev = await PrevRoutingAsync(prev?.Route);
            else
                reRoute = item;

        if (prev != null && NavigationRoutes.TryGetValue(_thisTopNavigationId, out var list))
        {
            var prevIndex = -1;
            foreach (var item in list)
            {
                prevIndex++;
                if (item == prev) break;
            }

            var removeIndex = list.Count - prevIndex;
            for (var i = 0; i < removeIndex; i++)
            {
                var item = list[removeIndex - (i + 1)];
                if (item != prev)
                {
                    // 标记为已移除导航堆栈
                    item.IsInNavigationStack = false;
                    // 触发页面停用生命周期
                    await InvokeDeactivateLifecycleAsync(item);
                    list.Remove(item);
                }
                else break;
            }
        }

        if (string.IsNullOrEmpty(reRoute) && prev != null)
        {
            _thisRoute = prev.Route;
            return prev;
        }

        var toPath = reRoute + "?" + routePathParameter.OriginParameter;
        var result = await GoToAsync(toPath, true) ?? throw new("未找到页面");
        result.Parameters = routePathParameter.Parameters;
        return result;
    }

    /// <summary>
    /// 导航到指定路由
    /// </summary>
    /// <param name="route">目标路由（支持路径参数和查询参数）</param>
    /// <param name="isThisAction">是否内部操作（防止循环调用）</param>
    /// <returns>路由信息对象</returns>
    /// <exception cref="Exception">路由未注册或初始化失败</exception>
    internal static async Task<RouteCurrentInfo?> GoToAsync(string route, bool isThisAction = false)
    {
        if (string.IsNullOrWhiteSpace(route)) return CurrentInfo;
        if (RoutingModels.Count == 0) throw new Exception("未注册路由");
        // 返回根页面
        if (route == "//") return await PopToRootAsync();
        // 如果需要返回上一页的话
        var queryIndex = route.IndexOf('.');
        if (queryIndex == 0)
        {
            var result = await PrevToAsync(route);
            return result;
        }

        // 解析路径和查询参数
        var (path, query) = ParseRoute(route);
        var parameters = QueryParser.Parse(query);

        // 检查缓存（新增逻辑）
        if (KeepAliveCache.TryGetPage(route: path, parameters, out var cachedInfo))
        {
            await InvokeReactivateLifecycleAsync(cachedInfo!);
            UpdateNavigationState(cachedInfo!, isThisAction);
            return cachedInfo!.IsTopNavigation
                ?
                // 如果是顶级页，直接返回最后一个页面
                NavigationRoutes[_thisTopNavigationId].LastOrDefault()
                : cachedInfo;
        }

        // 路由匹配（使用修改后的方法）
        var routingModel = FindBestMatchRoute(path, out var pathParameters) ?? throw new Exception($"路由未注册: {path}");

        // 合并路径参数和查询参数
        var mergedParameters = new Dictionary<string, object?>(pathParameters);
        if (parameters.Count != 0)
        {
            foreach (var kvp in parameters)
            {
                mergedParameters[kvp.Key] = kvp.Value;
            }
        }

        // 创建实例（兼容原有逻辑）
        var instance = CreatePageInstance(routingModel, mergedParameters, ServiceProvider);
        var routeInfo = BuildRouteInfo(routingModel, path, mergedParameters, instance);
        routeInfo.Meta = routingModel.Meta;
        // 生命周期处理，激活新页面触发
        await InvokeActivateLifecycleAsync(routeInfo);

        // 更新导航状态（保留原有逻辑）
        UpdateNavigationState(routeInfo, isThisAction);

        // 缓存处理（新增逻辑）
        if (routeInfo.IsKeepAlive)
        {
            KeepAliveCache.CachePage(routeInfo);
            if (routingModel.IsTopNavigation)
            {
                TopNavigations.AddOrUpdate(path, routeInfo, (_, _) => routeInfo);
            }
        }

        return routeInfo;
    }

    #region Replace

    /// <summary>
    /// 替换当前页面（不保留历史记录）
    /// </summary>
    /// <param name="route">目标路由（支持路径参数和查询参数）</param>
    /// <returns>路由信息对象</returns>
    /// <exception cref="Exception">路由未注册或初始化失败</exception>
    internal static async Task<RouteCurrentInfo?> ReplaceAsync(string route)
    {
        if (string.IsNullOrWhiteSpace(route)) return CurrentInfo;
        if (RoutingModels.Count == 0) throw new Exception("未注册路由");

        // 解析路径和查询参数
        var (path, query) = ParseRoute(route);
        var parameters = QueryParser.Parse(query);

        // 检查缓存（新增逻辑）
        if (KeepAliveCache.TryGetPage(route: path, parameters, out var cachedInfo))
        {
            await InvokeReactivateLifecycleAsync(cachedInfo!);
            ReplaceNavigationState(cachedInfo!);
            return cachedInfo!.IsTopNavigation
                ?
                // 如果是顶级页，直接返回最后一个页面
                NavigationRoutes[_thisTopNavigationId].LastOrDefault()
                : cachedInfo;
        }

        // 路由匹配（增强逻辑）
        // 路由匹配（使用修改后的方法）
        var routingModel = FindBestMatchRoute(path, out var pathParameters) ?? throw new Exception($"路由未注册: {path}");

        // 合并路径参数和查询参数
        var mergedParameters = new Dictionary<string, object?>(pathParameters);
        if (parameters.Count != 0)
        {
            foreach (var kvp in parameters)
            {
                mergedParameters[kvp.Key] = kvp.Value;
            }
        }

        // 创建实例（兼容原有逻辑）
        var instance = CreatePageInstance(routingModel, mergedParameters, ServiceProvider);
        var routeInfo = BuildRouteInfo(routingModel, path, mergedParameters, instance);
        routeInfo.Meta = routingModel.Meta;

        // 生命周期处理，激活新页面触发
        await InvokeActivateLifecycleAsync(routeInfo);

        // 替换导航状态（不保留历史记录）
        ReplaceNavigationState(routeInfo);

        // 缓存处理（新增逻辑）
        if (routeInfo.IsKeepAlive)
        {
            KeepAliveCache.CachePage(routeInfo);
            if (routingModel.IsTopNavigation)
            {
                TopNavigations.AddOrUpdate(path, routeInfo, (_, _) => routeInfo);
            }
        }

        return routeInfo;
    }

    /// <summary>
    /// 替换导航状态（不保留历史记录）
    /// </summary>
    /// <param name="info">当前路由信息</param>
    private static void ReplaceNavigationState(RouteCurrentInfo info)
    {
        // 更新当前路由状态
        CurrentInfo = info;
        _thisRoute = info.Route;

        // 如果是顶级导航页，替换整个导航栈
        if (info.IsTopNavigation)
        {
            _thisTopNavigationId = info.RoutingId;
            NavigationRoutes.AddOrUpdate(
                info.RoutingId,
                _ => [info],
                (_, existing) =>
                {
                    foreach (var routeCurrentInfo in existing)
                    {
                        routeCurrentInfo.IsInNavigationStack = false;
                    }

                    existing.Clear();
                    existing.Add(info);
                    return existing;
                }
            );
        }
        else
        {
            // 如果是子导航页，替换当前导航栈的最后一个页面
            if (NavigationRoutes.TryGetValue(_thisTopNavigationId, out var navigations))
            {
                if (navigations.Count > 0)
                {
                    // 触发旧页面的停用生命周期
                    var lastPage = navigations.Last();
                    if (lastPage.CurrentPage is IPageLifecycle lifecycle)
                    {
                        lastPage.IsInNavigationStack = false;
                        lifecycle.OnDeactivatedAsync();
                    }

                    // 替换最后一个页面
                    navigations[^1] = info;
                }
                else
                {
                    navigations.Add(info);
                }
            }
            else
            {
                NavigationRoutes[_thisTopNavigationId] = [info];
            }
        }
    }

    #endregion

    #region PopToRoot

    /// <summary>
    /// 返回到导航栈的根页面（通常是顶级导航页），并清空所有子页面
    /// </summary>
    /// <returns>根页面的路由信息对象</returns>
    private static async Task<RouteCurrentInfo?> PopToRootAsync()
    {
        // 获取当前顶级导航ID
        if (_thisTopNavigationId == Guid.Empty)
        {
            throw new InvalidOperationException("当前没有有效的顶级导航栈");
        }

        // 获取当前顶级导航栈
        if (!NavigationRoutes.TryGetValue(_thisTopNavigationId, out var navigations) || navigations.Count == 0)
        {
            throw new InvalidOperationException("导航栈为空或无效");
        }

        // 获取根页面（通常是导航栈的第一个页面）
        var rootPage = navigations.FirstOrDefault();
        if (rootPage == null)
        {
            throw new InvalidOperationException("导航栈中没有有效的根页面");
        }

        // 触发所有子页面的停用生命周期
        for (var i = 1; i < navigations.Count; i++)
        {
            var page = navigations[i];
            page.IsInNavigationStack = false;
            if (page.CurrentPage is IPageLifecycle lifecycle)
            {
                await lifecycle.OnDeactivatedAsync();
            }
        }

        rootPage.IsInNavigationStack = true;

        // 清空导航栈，只保留根页面
        navigations.Clear();
        navigations.Add(rootPage);

        // 更新当前路由状态
        CurrentInfo = rootPage;
        _thisRoute = rootPage.Route;

        // 触发根页面的重新激活生命周期
        if (rootPage.CurrentPage is IPageLifecycle rootLifecycle)
        {
            await rootLifecycle.OnReactivatedAsync(rootPage.Parameters);
        }

        return rootPage;
    }

    #endregion

    #region 新增辅助方法

    /// <summary>
    /// 解析路由路径和查询参数
    /// </summary>
    /// <param name="route">完整路由字符串</param>
    /// <returns>返回路径和查询参数</returns>
    private static (string Path, string Query) ParseRoute(string route)
    {
        var queryIndex = route.IndexOf('?');
        return queryIndex == -1
            ? (route.Trim('/'), string.Empty)
            : (route[..queryIndex].Trim('/'), route[(queryIndex + 1)..]);
    }

    /// <summary>
    /// 查找最佳匹配的路由配置
    /// </summary>
    /// <param name="path">请求路径</param>
    /// <param name="pathParameters">路径参数</param>
    /// <returns>匹配的路由配置，未找到返回null</returns>
    private static RoutingModel? FindBestMatchRoute(string path, out Dictionary<string, object?> pathParameters)
    {
        pathParameters = new Dictionary<string, object?>();
        var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // 遍历所有路由配置，找到第一个匹配项
        foreach (var routingModel in RoutingModels.OrderByDescending(r => r.Priority))
        {
            var (isMatch, parameters) = IsRouteMatch(routingModel.Route, pathSegments);
            if (isMatch)
            {
                pathParameters = parameters;
                return routingModel;
            }
        }

        return null;
    }

    /// <summary>
    /// 判断路由模板是否匹配请求路径，并返回路径参数
    /// </summary>
    /// <param name="routeTemplate">路由模板</param>
    /// <param name="pathSegments">请求路径分段</param>
    /// <returns>是否匹配 + 路径参数字典</returns>
    private static (bool IsMatch, Dictionary<string, object?> Parameters) IsRouteMatch(
        string routeTemplate,
        string[] pathSegments)
    {
        var parameters = new Dictionary<string, object?>();
        var templateSegments = routeTemplate.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // 分段数量必须一致
        if (templateSegments.Length != pathSegments.Length)
            return (false, parameters);

        // 逐段匹配
        for (var i = 0; i < templateSegments.Length; i++)
        {
            var templateSegment = templateSegments[i];
            var pathSegment = pathSegments[i];

            // 如果是参数段（如 {id}）
            if (templateSegment.StartsWith('{') && templateSegment.EndsWith('}'))
            {
                var paramName = templateSegment[1..^1];
                parameters[paramName] = pathSegment;
                continue;
            }

            // 静态段必须完全匹配（忽略大小写）
            if (!string.Equals(templateSegment, pathSegment, StringComparison.OrdinalIgnoreCase))
                return (false, parameters);
        }

        return (true, parameters);
    }

    /// <summary>
    /// 创建页面实例
    /// </summary>
    /// <param name="model">路由配置</param>
    /// <param name="parameters">参数</param>
    /// <param name="serviceProvider">DI</param>
    /// <returns>页面实例</returns>
    private static object CreatePageInstance(RoutingModel model, Dictionary<string, object?>? parameters,
        IServiceProvider? serviceProvider = null)
    {
        // 1. 如果是顶级导航页，检查 TopNavigations
        if (model.IsTopNavigation && TopNavigations.TryGetValue(model.Route, out var existing))
        {
            return existing.CurrentPage!;
        }

        // 2. 尝试从 KeepAliveCache 获取（支持非顶级导航的保活页）
        if (model.IsKeepAlive && KeepAliveCache.TryGetPage(model.Route, parameters, out var cachedInstance))
        {
            return cachedInstance!.CurrentPage!;
        }

        // DI 模式：通过服务提供者创建实例
        return serviceProvider != null
            ? ActivatorUtilities.CreateInstance(serviceProvider, model.PageType)
            : Activator.CreateInstance(model.PageType)!;
    }

    /// <summary>
    /// 构建路由信息对象
    /// </summary>
    /// <param name="model">路由配置</param>
    /// <param name="path">请求路径</param>
    /// <param name="parameters">查询参数</param>
    /// <param name="instance">页面实例</param>
    /// <returns>路由信息对象</returns>
    private static RouteCurrentInfo BuildRouteInfo(
        RoutingModel model,
        string path,
        Dictionary<string, object?> parameters,
        object instance)
    {
        return new RouteCurrentInfo
        {
            Route = path,
            CurrentPage = instance,
            Parameters = parameters,
            IsKeepAlive = model.IsKeepAlive,
            IsTopNavigation = model.IsTopNavigation,
            RoutingId = Guid.NewGuid()
        };
    }

    /// <summary>
    /// 触发页面激活生命周期
    /// </summary>
    /// <param name="info">路由信息</param>
    private static async Task InvokeActivateLifecycleAsync(RouteCurrentInfo info)
    {
        if (info.CurrentPage is IPageLifecycle lifecycle)
        {
            await lifecycle.OnActivatedAsync(info.Parameters);
        }
    }

    /// <summary>
    /// 触发页面重新激活生命周期
    /// </summary>
    /// <param name="info">路由信息</param>
    private static async Task InvokeReactivateLifecycleAsync(RouteCurrentInfo info)
    {
        if (info.CurrentPage is IPageLifecycle lifecycle)
        {
            await lifecycle.OnReactivatedAsync(info.Parameters);
        }
    }

    /// <summary>
    /// 触发页面停用生命周期
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private static Task InvokeDeactivateLifecycleAsync(RouteCurrentInfo info)
    {
        if (info.CurrentPage is IPageLifecycle lifecycle)
        {
            lifecycle.OnDeactivatedAsync();
        }

        return Task.CompletedTask;
    }

    #endregion

    #region 原有逻辑适配

    /// <summary>
    /// 更新导航状态
    /// </summary>
    /// <param name="info">当前路由信息</param>
    /// <param name="isThisAction">是否内部操作</param>
    private static void UpdateNavigationState(RouteCurrentInfo info, bool isThisAction)
    {
        if (isThisAction) return;

        // 新增页面时标记为在导航堆栈中
        info.IsInNavigationStack = true;

        // 保留原有导航栈逻辑
        if (info.IsTopNavigation)
        {
            _thisTopNavigationId = info.RoutingId;
            // NavigationRoutes.AddOrUpdate(info.RoutingId, [info], (_, list) =>
            // {
            //     list.Clear();
            //     list.Add(info);
            //     return list;
            // });
            // 上面这小段有Bug，下面为修复代码
            NavigationRoutes.AddOrUpdate(
                info.RoutingId,
                _ => [info],
                (_, existing) =>
                {
                    var res = existing.Count == 0 ? [info] : existing;
                    return res;
                }
            );
        }
        else
        {
            if (NavigationRoutes.TryGetValue(_thisTopNavigationId, out var navigations))
            {
                navigations.Add(info);
            }
            else
            {
                NavigationRoutes[_thisTopNavigationId] = [info];
            }
        }

        CurrentInfo = info;
        _thisRoute = info.Route;
    }

    #endregion

    /// <summary>
    /// 获取上一页
    /// </summary>
    /// <param name="paramRoute">指定导航路由</param>
    /// <returns>返回上一页的信息，若指定导航路由则获取指定导航路由的上一个，若此页已经是顶级路由，且找不到指定路由就返回当前空</returns>
    private static Task<RouteCurrentInfo?> PrevRoutingAsync(string? paramRoute = null)
    {
        // 首先获取本页是不是属于菜单页
        var findInfo = NavigationRoutes[_thisTopNavigationId].FirstOrDefault(q => q.Route == (paramRoute ?? _thisRoute)) ??
                       throw new Exception($"此路由不存在：{paramRoute}");
        var isTopNavigation = findInfo.IsTopNavigation;
        if (isTopNavigation)
        {
            var res = !TopNavigations.TryGetValue(findInfo.Route!, out var result) ? null : result;
            return Task.FromResult(res);
        }

        // 根据当前菜单Id和它的路由来查找当前所在位置
        if (_thisTopNavigationId == Guid.Empty) throw new Exception("顶级路由ID错误");
        // 当前循环所在下标
        var thisIndex = -1;
        if (!NavigationRoutes.TryGetValue(_thisTopNavigationId, out var navigations))
            return Task.FromResult(CurrentInfo);
        // 获取自身下标
        foreach (var route in navigations)
        {
            thisIndex++;
            if (route.Route == _thisRoute) break;
        }

        // 如果自身下标大于0，就代表有上一页，否则就是菜单页
        if (thisIndex > 0)
        {
            // 获取上一页
            var findPrev = navigations[thisIndex - 1];
            return Task.FromResult<RouteCurrentInfo?>(findPrev);
        }

        // 否则返回菜单页面
        var findTopNavigation = TopNavigations.FirstOrDefault(q => q.Value.RoutingId == _thisTopNavigationId).Value;

        return Task.FromResult<RouteCurrentInfo?>(findTopNavigation);
    }

    /// <summary>
    /// 清空所有路由
    /// </summary>
    public static void ClearRouting()
    {
        NavigationRoutes.Clear();
        TopNavigations.Clear();
        CurrentInfo = null;
        _thisRoute = null;
        _thisTopNavigationId = Guid.Empty;
    }
}