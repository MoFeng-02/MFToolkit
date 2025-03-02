﻿using System.Collections.Concurrent;
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
    /// <param name="type">页面</param>
    /// <param name="route">路由</param>
    /// <param name="isTopNavigation">是否顶级菜单页</param>
    /// <param name="isKeepAlive">是否保活页</param>
    /// <param name="priority">页面优先级（排序）</param>
    /// <param name="meta">Meta</param>
    public static void RegisterRoute(Type type, string? route = null, bool isTopNavigation =
        false, bool isKeepAlive = false, int priority = 0, RoutingMeta? meta = null)
    {
        // 如果路由为空，则设置随机路由
        route ??= Guid.NewGuid().ToString();
        if (RoutingModels.Any(q => q.Route == route))
            throw new Exception($"Route already exists: {route}");
        if (isTopNavigation) isKeepAlive = true;
        RoutingModels.Add(new RoutingModel
        {
            Route = route,
            PageType = type,
            IsTopNavigation = isTopNavigation,
            IsKeepAlive = isKeepAlive,
            Priority = priority,
            Meta = meta
        });

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
            if (CurrentInfo != null) await InvokeDeactivateLifecycleAsync(CurrentInfo);
            await InvokeReactivateLifecycleAsync(cachedInfo!);
            UpdateNavigationState(cachedInfo!, isThisAction);
            if (cachedInfo!.IsTopNavigation)
            {
                // 如果是顶级页，直接返回最后一个页面
                return NavigationRoutes[_thisTopNavigationId].LastOrDefault();
            }

            return cachedInfo;
        }

        // 路由匹配（增强逻辑）
        var routingModel = FindBestMatchRoute(path) ?? throw new Exception($"路由未注册: {path}");

        // 创建实例（兼容原有逻辑）
        var instance = CreatePageInstance(routingModel, parameters, ServiceProvider);
        var routeInfo = BuildRouteInfo(routingModel, path, parameters, instance);
        routeInfo.Meta = routingModel.Meta;
        // 生命周期处理，停用当前页面触发
        if (CurrentInfo != null) await InvokeDeactivateLifecycleAsync(CurrentInfo);
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
    /// <returns>匹配的路由配置，未找到返回null</returns>
    private static RoutingModel? FindBestMatchRoute(string path)
    {
        var pathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        return RoutingModels
            .OrderByDescending(r => r.Priority) // 按优先级降序
            .FirstOrDefault(r => IsRouteMatch(r.Route, pathSegments));
    }

    /// <summary>
    /// 判断路由模板是否匹配请求路径
    /// </summary>
    /// <param name="routeTemplate">路由模板</param>
    /// <param name="pathSegments">请求路径分段</param>
    /// <returns>是否匹配</returns>
    private static bool IsRouteMatch(string routeTemplate, string[] pathSegments)
    {
        var templateSegments = routeTemplate.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // 分段数量必须一致
        if (templateSegments.Length != pathSegments.Length) return false;

        // 逐段匹配
        for (var i = 0; i < templateSegments.Length; i++)
        {
            // 如果是参数段（如{id}），跳过匹配
            if (templateSegments[i].StartsWith('{') && templateSegments[i].EndsWith('}'))
                continue;

            // 静态段必须完全匹配（忽略大小写）
            if (!string.Equals(templateSegments[i], pathSegments[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
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
    private static async Task InvokeDeactivateLifecycleAsync(RouteCurrentInfo info)
    {
        if (info.CurrentPage is IPageLifecycle lifecycle)
        {
            await lifecycle.OnDeactivatedAsync();
        }
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
            // 获取顶级导航栈
            if (!NavigationRoutes.TryGetValue(info.RoutingId, out var _))
            {
                // 如果没有的话，就进行注册顶级导航
                NavigationRoutes.AddOrUpdate(info.RoutingId, [info], (_, list) =>
                {
                    list.Clear();
                    list.Add(info);
                    return list;
                });
            }
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
    /// <param name="paramRoute">导航路由</param>
    /// <returns></returns>
    private static async Task<RouteCurrentInfo?> PrevRoutingAsync(string? paramRoute = null)
    {
        // 首先获取本页是不是属于菜单页
        var findInfo = RoutingModels.FirstOrDefault(q => q.Route == (paramRoute ?? _thisRoute)) ??
                       throw new Exception($"此路由不存在：{paramRoute}");
        var isTopNavigation = findInfo.IsTopNavigation;
        if (isTopNavigation)
        {
            return !TopNavigations.TryGetValue(findInfo.Route, out var result) ? CurrentInfo : result;
        }

        // 根据当前菜单Id和它的路由来查找当前所在位置
        if (_thisTopNavigationId == Guid.Empty) throw new Exception("顶级路由ID错误");
        // 当前循环所在下标
        var thisIndex = -1;
        if (!NavigationRoutes.TryGetValue(_thisTopNavigationId, out var navigations))
            return CurrentInfo;
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
            return findPrev;
        }

        // 否则返回菜单页面
        var findTopNavigation = TopNavigations.FirstOrDefault(q => q.Value.RoutingId == _thisTopNavigationId).Value;
        if (findTopNavigation?.CurrentPage is IPageLifecycle lifecycle)
        {
            await lifecycle.OnDeactivatedAsync();
        }

        return findTopNavigation;
    }

    /// <summary>
    /// 清空所有路由
    /// </summary>
    public static void ClearRouting()
    {
        RoutingModels.Clear();
        NavigationRoutes.Clear();
        TopNavigations.Clear();
        CurrentInfo = null;
        _thisRoute = null;
        _thisTopNavigationId = Guid.Empty;
    }
}