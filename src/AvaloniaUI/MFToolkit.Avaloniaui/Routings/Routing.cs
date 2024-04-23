using System.Collections.Concurrent;

namespace MFToolkit.Avaloniaui.Routings;

/// <summary>
/// 路由类
/// </summary>
public sealed class Routing
{
    /// <summary>
    /// 当前顶级导航ID
    /// </summary>
    private static Guid ThisTopNavigationId = Guid.Empty;

    /// <summary>
    /// 当前导航ID
    /// </summary>
    private static Guid ThisNavigationId = Guid.Empty;

    /// <summary>
    /// 当前路由
    /// </summary>
    private static string? ThisRoute;

    public static RouteCurrentInfo? CurrentInfo { get; private set; }
    /// <summary>
    /// 路由集合的顶级ID
    /// </summary>
#if NET8_0_OR_GREATER
    private static readonly ConcurrentDictionary<string, RouteCurrentInfo> TopNavigations = [];
#else
    private static readonly ConcurrentDictionary<string, RouteCurrentInfo> TopNavigations = new();
#endif
    /// <summary>
    /// 保活页面
    /// </summary>
#if NET8_0_OR_GREATER
    private static readonly ConcurrentDictionary<string, RouteCurrentInfo> KeepAlives = [];
#else
    private static readonly ConcurrentDictionary<string, RouteCurrentInfo> KeepAlives = new();
#endif
    /// <summary>
    /// 路由集合
    /// Guid: RoutingId
    /// List: RouteInfos
    /// </summary>
#if NET8_0_OR_GREATER
    private static readonly ConcurrentDictionary<Guid, List<RouteCurrentInfo>> NavigationRoutings = [];
#else
    private static readonly ConcurrentDictionary<Guid, List<RouteCurrentInfo>> NavigationRoutings = new();
#endif
    /// <summary>
    /// 分割处理符号
    /// </summary>
#if NET8_0_OR_GREATER
    private static readonly char[] separator = ['/'];
#else
    private static readonly char[] separator = { '/' };
#endif
    /// <summary>
    /// 路由信息
    /// </summary>
#if NET8_0_OR_GREATER
    private static List<RoutingModel> RoutingInfos { get; } = [];
#else
    private static List<RoutingModel> RoutingInfos { get; } = new();
#endif
    /// <summary>
    /// 获取路由详情信息
    /// </summary>
    /// <returns></returns>
    public static List<RoutingModel> GetRoutingInfos() => RoutingInfos;

    /// <summary>
    /// 注册路由
    /// </summary>
    /// <param name="routings"></param>
    /// <exception cref="Exception"></exception>
    public static void RegisterRoutes(params RoutingModel[] routings)
    {
        foreach (var item in routings)
        {
            item.Route ??= item.PageType.Name.ToLower();
            if (RoutingInfos.Any(q => q.Route == item.Route))
                throw new Exception($"路由已存在: {item.Route}");
            RoutingInfos.Add(item);
        }
    }

    /// <summary>
    /// 注册路由
    /// </summary>
    /// <param name="type">页面</param>
    /// <param name="route">路由</param>
    /// <param name="title">标题</param>
    /// <param name="isTopNavigation">是否顶级菜单页</param>
    /// <param name="isKeepAlive">是否保活页</param>
    /// <param name="imagePath">页面的图标</param>
    public static void RegisterRoute(Type type, string? route = null, string? title = null, bool isTopNavigation =
        false, bool isKeepAlive = false, string? imagePath = null)
    {
        // 如果路由为空，则设置随机路由
        route ??= type.Name.ToLower();
        if (RoutingInfos.Any(q => q.Route == route)) throw new Exception($"Route registration duplication: {route}");
        RoutingInfos.Add(new RoutingModel
        {
            Route = route,
            PageType = type,
            Title = title ?? string.Empty,
            IsTopNavigation = isTopNavigation,
            IsKeepAlive = isKeepAlive,
            ImagePath = imagePath
        });
    }

    /// <summary>
    /// 根据路由注册的页面类型来获取路由
    /// <para>通常用于未设置路由而随机生成路由的类</para>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string? PageTypeToRoute(Type type)
    {
        var query = RoutingInfos.FirstOrDefault(q => q.PageType == type);
        return query?.Route;
    }

    /// <summary>
    /// 获取路径后面的参数
    /// </summary>
    /// <param name="queryString">路由参数</param>
    /// <returns></returns>
    private static Dictionary<string, object?>? QueryParameter(string? queryString)
    {
        // 检查查询字符串是否为空或仅包含空白字符
        if (string.IsNullOrWhiteSpace(queryString)) return null;

        // 将查询字符串拆分为键值对数组，并创建字典
        var queryPairs = queryString.Split('&')
            .Select(pair => pair.Split('='))
            .ToDictionary(
                // 键是已解码的键部分
                pair => Uri.UnescapeDataString(pair[0]),
                // 值是已解码的值部分，如果值不存在，则设为 null
                pair => pair.Length > 1 ? (object)Uri.UnescapeDataString(pair[1]) : default
            );

        // 返回包含解析后键值对的字典
        return queryPairs;
    }

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
        var parts = path.Split(separator, StringSplitOptions.RemoveEmptyEntries);
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
                var subParts = part.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
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

        if (!isOkRoute) routePathParameter.ErrorMessage = $"错误路由：{path}";
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
        if (!NavigationRoutings.TryGetValue(ThisTopNavigationId, out var navigations)) return false;
        return navigations.Count != 0;
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
                prev = await PrevRouting(prev?.Route);
            else
                reRoute = item;

        if (prev != null && NavigationRoutings.TryGetValue(ThisTopNavigationId, out var list))
        {
            var prevIndex = -1;
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                prevIndex++;
                if (item == prev) break;
            }

            var removeIndex = list.Count - prevIndex;
            for (var i = 0; i < removeIndex; i++)
            {
                var item = list[removeIndex - (i + 1)];
                if (item != prev)
                {
                    list.Remove(item);
                }
                else break;
            }
        }

        if (string.IsNullOrEmpty(reRoute) && prev != null)
        {
            ThisRoute = prev.Route;
            ThisNavigationId = prev.RoutingId;
            return prev;
        }

        var toPath = reRoute + "?" + routePathParameter.OriginParameter;
        var result = await GoToAsync(toPath, true);
        result.Parameters = routePathParameter.Parameters;
        return result;
    }

    /// <summary>
    /// 跳转路由
    /// </summary>
    /// <param name="route"></param>
    /// <param name="isThisAction">是否是本类操作</param>
    /// <returns></returns>
    internal static async Task<RouteCurrentInfo?> GoToAsync(string route, bool isThisAction = false)
    {
        if (string.IsNullOrWhiteSpace(route)) return CurrentInfo;
        if (!RoutingInfos.Any()) throw new Exception("未注册路由");
        // 如果需要返回上一页的话
        var queryIndex = route.IndexOf('.');
        if (queryIndex == 0)
        {
            var result = await PrevToAsync(route);
            return result;
        }

        var findIndex = route.IndexOf('?');
        if (findIndex == 0) throw new Exception("符号“?”放置位置错误，错误示例：?a=1，正确示例：hemo?a=1");
        // NETSTANDARD2_0 标准已被剔除，只保留net >= 7 版本，暂不考虑支持Net Standard2.0标准
        //#if NETSTANDARD2_0
        //        // 格式化后的路由
        //        var formatRoute = findIndex == -1 ? route : route.Substring(0, findIndex);
        //        // 格式化后的参数值
        //        var formatData = QueryParameter(findIndex == -1 ? null : route.Substring(++findIndex));
        //#elif NET7_0 || NET8_0
        // 格式化后的路由
        var formatRoute = findIndex == -1 ? route : route[..findIndex];
        // 格式化后的参数值
        var formatData = QueryParameter(findIndex == -1 ? null : route[++findIndex..]);
        //#endif
        // 查找路由注册信息
        var findInfo = RoutingInfos.FirstOrDefault(q => q.Route == formatRoute)!;


        bool s = ThisRoute == formatRoute;
        if (RoutingInfos.All(q => q.Route != formatRoute)) throw new Exception("该路由未注册");
        if (ThisRoute == formatRoute && !isThisAction) return CurrentInfo;
        ThisRoute = formatRoute;
        var isTopNavigation = findInfo.IsTopNavigation;
        var isKeepAlive = findInfo.IsKeepAlive;
        RouteCurrentInfo routeCurrentInfo = new()
        {
            Route = formatRoute,
            IsKeepAlive = findInfo.IsKeepAlive,
            IsTopNavigation = findInfo.IsTopNavigation,
            Parameters = formatData
        };
        // 是否为顶级菜单页
        if (isTopNavigation)
        {
            // 如果是的话，且没用注册过则注册到顶级菜单页
            if (!TopNavigations.TryGetValue(formatRoute, out var topNavigation))
            {
                routeCurrentInfo.CurrentPage = Activator.CreateInstance(findInfo.PageType);
                TopNavigations.TryAdd(formatRoute, routeCurrentInfo);
                ThisTopNavigationId = routeCurrentInfo.RoutingId;
                ThisNavigationId = routeCurrentInfo.RoutingId;
                CurrentInfo = routeCurrentInfo;
                return routeCurrentInfo;
            }

            // 如果已经存在了
            ThisTopNavigationId = topNavigation.RoutingId;
            ThisNavigationId = topNavigation.RoutingId;
            // 查找子页面找到最后的页面
            if (NavigationRoutings.TryGetValue(topNavigation.RoutingId, out var navigations))
            {
                if (navigations.Count == 0)
                {
                    CurrentInfo = topNavigation;
                    return topNavigation;
                }

                var lastInfo = navigations.LastOrDefault()!;
                ThisNavigationId = lastInfo.RoutingId;
                CurrentInfo = lastInfo;
                return lastInfo;
            }

            CurrentInfo = topNavigation;
            return topNavigation;
        }

        // 如果不是顶级菜单，且为保活页
        if (!isTopNavigation && isKeepAlive)
        {
            // 如果不存在顶级导航页面
            if (ThisTopNavigationId == Guid.Empty) ThisTopNavigationId = Guid.NewGuid();
            // 如果现在不存在，则注册
            if (!KeepAlives.TryGetValue(formatRoute, out var keepAlive))
            {
                routeCurrentInfo.CurrentPage = Activator.CreateInstance(findInfo.PageType);
                ThisNavigationId = routeCurrentInfo.RoutingId;
                CurrentInfo = routeCurrentInfo;
                KeepAlives.TryAdd(formatRoute, routeCurrentInfo);
            }
            else
            {
                // 如果已经存在了
                ThisNavigationId = keepAlive.RoutingId;
                routeCurrentInfo.CurrentPage = keepAlive.CurrentPage;
                CurrentInfo = routeCurrentInfo;
            }
        }

        if (routeCurrentInfo.CurrentPage == null)
        {
            // 如果不存在顶级导航页面
            if (ThisTopNavigationId == Guid.Empty) ThisTopNavigationId = Guid.NewGuid();
            if (!NavigationRoutings.TryGetValue(ThisTopNavigationId, out var navigations))
            {
                // 如果没找到的话
#if NET8_0_OR_GREATER
                if (navigations == null || navigations.Count == 0) navigations = [];
#else
                if (navigations == null || navigations.Count == 0) navigations = new();
#endif
                routeCurrentInfo.CurrentPage = Activator.CreateInstance(findInfo.PageType);
                ThisNavigationId = routeCurrentInfo.RoutingId;
                navigations.Add(routeCurrentInfo);
                CurrentInfo = routeCurrentInfo;
                NavigationRoutings.AddOrUpdate(ThisTopNavigationId, navigations, (key, value) => navigations);
            }
            else
            {
                // 如果找到的话
                var lastInfo = navigations.LastOrDefault();
                var lastIsNull = lastInfo != null && lastInfo?.Route == formatRoute;
                if (lastIsNull)
                {
                    ThisNavigationId = lastInfo?.RoutingId ?? Guid.Empty;
                    routeCurrentInfo.CurrentPage = lastInfo?.CurrentPage;
                    CurrentInfo = routeCurrentInfo;
                }
                else
                {
                    routeCurrentInfo.CurrentPage ??= Activator.CreateInstance(findInfo.PageType);
                    ThisNavigationId = routeCurrentInfo.RoutingId;
                    navigations.Add(routeCurrentInfo);
                    CurrentInfo = routeCurrentInfo;
                    NavigationRoutings.AddOrUpdate(ThisTopNavigationId, navigations, (key, value) => navigations);
                }
            }
        }
        else
        {
            NavigationRoutings.TryGetValue(ThisTopNavigationId, out var navigations);
#if NET8_0_OR_GREATER
            if (navigations == null || navigations.Count == 0) navigations = [];
#else
            if (navigations == null || navigations.Count == 0) navigations = new();
#endif
            CurrentInfo ??= routeCurrentInfo;
            navigations.Add(routeCurrentInfo);
            NavigationRoutings.AddOrUpdate(ThisTopNavigationId, navigations, (key, value) => navigations);
        }

        return routeCurrentInfo;
    }

    /// <summary>
    /// 获取上一页
    /// </summary>
    /// <param name="_route">导航路由</param>
    /// <returns></returns>
    private static Task<RouteCurrentInfo?> PrevRouting(string? _route = null)
    {
        // 首先获取本页是不是属于菜单页
        var findInfo = RoutingInfos.FirstOrDefault(q => q.Route == (_route ?? ThisRoute)) ??
                       throw new Exception($"此路由不存在：{_route}");
        var isTopNavigation = findInfo.IsTopNavigation;
        if (isTopNavigation)
        {
            return Task.FromResult(!TopNavigations.TryGetValue(findInfo.Route!, out var result) ? CurrentInfo : result);
        }

        // 根据当前菜单Id和它的路由来查找当前所在位置
        if (ThisTopNavigationId == Guid.Empty) throw new Exception("顶级路由ID错误");
        // 当前循环所在下标
        var thisIndex = -1;
        if (!NavigationRoutings.TryGetValue(ThisTopNavigationId, out var navigations))
            return Task.FromResult(CurrentInfo);
        // 获取自身下标
        foreach (var route in navigations)
        {
            thisIndex++;
            if (route.Route == ThisRoute) break;
        }

        // 如果自身下标大于0，就代表有上一页，否则就是菜单页
        if (thisIndex > 0)
        {
            // 获取上一页
            var findPrev = navigations[thisIndex - 1];
            return Task.FromResult(findPrev);
        }

        // 否则返回菜单页面
        var findTopNavigation = TopNavigations.FirstOrDefault(q => q.Value.RoutingId == ThisTopNavigationId).Value;
        if (findTopNavigation == null) return null;
        return Task.FromResult(findTopNavigation);
    }

    /// <summary>
    /// 清空所有路由
    /// </summary>
    public static void ClearRouting()
    {
        RoutingInfos.Clear();
        NavigationRoutings.Clear();
        KeepAlives.Clear();
        TopNavigations.Clear();
        CurrentInfo = null;
        ThisRoute = null;
        ThisNavigationId = Guid.Empty;
        ThisTopNavigationId = Guid.Empty;
    }
}

public class RoutingModel
{
    public string Title { get; set; }

    /// <summary>
    /// 页面
    /// </summary>
    public Type PageType { get; set; }

    /// <summary>
    /// 页面路由
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 是否为保活页面
    /// </summary>
    public bool IsKeepAlive { get; set; }

    /// <summary>
    /// 是否为顶级页面，顶级页面代表不能在子页面里面，顶级页面页默认就是保活页
    /// </summary>
    public bool IsTopNavigation { get; set; }

    /// <summary>
    /// 页面的图标
    /// </summary>
    public string? ImagePath { get; set; }
}

/// <summary>
/// 路由创建内容
/// </summary>
public class RouteCurrentInfo
{
    /// <summary>
    /// 页面ID
    /// </summary>
    public Guid RoutingId { get; internal set; } = Guid.NewGuid();

    public string? Route { get; internal set; }
    public object? CurrentPage { get; internal set; }
    public bool IsKeepAlive { get; internal set; }
    public bool IsTopNavigation { get; internal set; }

    /// <summary>
    /// 参数集合
    /// </summary>
    public Dictionary<string, object?>? Parameters { get; internal set; }
}

public class RoutePathParameter
{
    /// <summary>
    /// 路径集合
    /// </summary>
    public List<string>? Routes { get; set; }

    /// <summary>
    /// 原参数
    /// </summary>
    public string? OriginParameter { get; set; }

    /// <summary>
    /// 参数集合
    /// </summary>
    public Dictionary<string, object?>? Parameters { get; set; }

    /// <summary>
    /// 路由检查是否通过
    /// </summary>
    public bool IsRouteOk { get; set; }

    /// <summary>
    /// 路由错误体提醒
    /// </summary>
    public string ErrorMessage { get; set; }
}