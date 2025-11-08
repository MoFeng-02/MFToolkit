using System.Collections.Concurrent;
using System.Diagnostics;
using Avalonia.Controls;
using MFToolkit.Avaloniaui.Routes.Core.Abstractions;
using MFToolkit.Avaloniaui.Routes.Core.Entities;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;
using MFToolkit.Avaloniaui.Routes.Infrastructure.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Avaloniaui.Routes.Infrastructure.Services;

/// <summary>
/// 路由服务实现类，负责路由的管理和导航功能
/// </summary>
public class RoutingService : IRoutingService
{
    #region 静态成员（支持静态调用）

    private static RoutingService? DefaultInstance;

    private static readonly List<RoutingModel> _routingModels = [];

    /// <summary>
    /// 当前顶级导航ID
    /// </summary>
    private static Guid _thisTopNavigationId = Guid.Empty;

    /// <summary>
    /// 路由集合的顶级ID
    /// </summary>
    private static readonly ConcurrentDictionary<string, RouteCurrentInfo> TopNavigations = [];

    /// <summary>
    /// 路由集合
    /// Guid: RoutingId -> TopNavigations
    /// List: RouteInfos
    /// </summary>
    private static readonly ConcurrentDictionary<Guid, Stack<RouteCurrentInfo>> NavigationRoutes = [];

    private static Stack<RouteCurrentInfo> NavigationStack =>
        NavigationRoutes.FirstOrDefault(q => q.Key == _thisTopNavigationId).Value;

    /// <summary>
    /// 静态默认实例，用于不使用依赖注入的场景
    /// </summary>
    public static readonly RoutingService Default = DefaultInstance ??= new RoutingService(
        serviceProvider: null,
        routeParser: new RouteParser(),
        keepAliveCache: new KeepAliveCache());

    /// <summary>
    /// 静态方式注册带ViewModel的路由
    /// </summary>
    public static void RegisterRoute<TPage, TViewModel>(
        string? route = null,
        bool isKeepAlive = false,
        bool isTopNavigation = false,
        int priority = 0)
        where TPage : class
        where TViewModel : class
    {
        Default.RegisterRoute(new RoutingModel(typeof(TPage), typeof(TViewModel), route, isKeepAlive, isTopNavigation,
            priority));
    }

    /// <summary>
    /// 静态方式注册不带ViewModel的路由
    /// </summary>
    public static void RegisterRoute<TPage>(
        string? route = null,
        bool isKeepAlive = false,
        bool isTopNavigation = false,
        int priority = 0)
        where TPage : class
    {
        Default.RegisterRoute(new RoutingModel(typeof(TPage), null, route, isKeepAlive, isTopNavigation, priority));
    }

    /// <summary>
    /// 静态方式注册非泛型
    /// </summary>
    public static void RegisterRoute(
        Type pageType,
        Type? viewModelType = null,
        string? route = null,
        bool isKeepAlive = false,
        bool isTopNavigation = false,
        int priority = 0)
    {
        Default.RegisterRoute(new RoutingModel(pageType, viewModelType, route, isKeepAlive, isTopNavigation, priority));
    }

    #endregion

    private readonly IRouteParser _routeParser;
    private readonly IServiceProvider? _serviceProvider;
    private readonly KeepAliveCache _keepAliveCache;


    // 路由守卫集合
    private readonly List<IRouteGuard> _routeGuards = [];


    /// <summary>
    /// 构造函数（用于依赖注入）
    /// </summary>
    /// <param name="serviceProvider">服务提供器</param>
    /// <param name="routeParser">路由解析器</param>
    /// <param name="keepAliveCache">保活缓存</param>
    public RoutingService(
        IServiceProvider? serviceProvider,
        IRouteParser routeParser,
        KeepAliveCache? keepAliveCache)
    {
        _serviceProvider = serviceProvider;
        _routeParser = routeParser ?? throw new ArgumentNullException(nameof(routeParser));
        _keepAliveCache = keepAliveCache ?? new KeepAliveCache();
        DefaultInstance = this;
        // 从DI容器中获取所有路由守卫
        if (_serviceProvider == null) return;
        var guards = _serviceProvider.GetServices<IRouteGuard>();
        foreach (var guard in guards)
        {
            _routeGuards.Add(guard);
        }
    }

    #region 路由守卫管理

    /// <summary>
    /// 添加路由守卫（实例方法）
    /// </summary>
    /// <param name="guard">路由守卫实例，不可为null</param>
    /// <exception cref="ArgumentNullException">当guard为null时抛出</exception>
    private void AddRouteGuard(IRouteGuard guard)
    {
        if (guard == null)
            throw new ArgumentNullException(nameof(guard));

        _routeGuards.Add(guard);
    }

    /// <summary>
    /// 添加路由守卫（静态方法）
    /// </summary>
    /// <param name="guard">路由守卫实例，不可为null</param>
    public static void AddStaticRouteGuard(IRouteGuard guard)
    {
        Default.AddRouteGuard(guard);
    }

    /// <summary>
    /// 清除所有路由守卫
    /// </summary>
    public void ClearRouteGuards()
    {
        _routeGuards.Clear();
    }

    /// <summary>
    /// 清除所有路由守卫
    /// </summary>
    public static void ClearStaticRouteGuards()
    {
        Default.ClearRouteGuards();
    }

    #endregion

    /// <summary>
    /// 注册路由模型
    /// </summary>
    /// <param name="model">路由模型，不可为null</param>
    public void RegisterRoute(RoutingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        // 检查路由是否已存在
        if (_routingModels.Any(m => string.Equals(m.Route, model.Route, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"路由 '{model.Route}' 已存在");
        }

        _routingModels.Add(model);

        // 按优先级排序，确保高优先级路由先匹配
        _routingModels.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    /// <summary>
    /// 页面类型获取路由
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string? PageTypeToRoute(Type type)
    {
        var query = _routingModels.FirstOrDefault(q => q.PageType == type);
        return query?.Route;
    }

    /// <summary>
    /// 导航到指定路由
    /// </summary>
    /// <param name="route">目标路由</param>
    /// <param name="parameters">路由参数，可为null</param>
    /// <returns>当前路由信息</returns>
    public async Task<RouteCurrentInfo?> GoToAsync(string route, Dictionary<string, object?>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return null;
        }

        // 查找匹配的路由模型
        var (matchingModel, routeParameters) = FindMatchingRoute(route);
        if (matchingModel == null)
        {
            return null;
        }

        // 合并所有参数（路由参数 + 额外参数）
        var mergedParameters = MergeParameters(routeParameters, parameters);
        // 3. 执行路由守卫检查（核心新增逻辑）
        if (!await CheckRouteGuardsAsync(matchingModel, mergedParameters))
            return null; // 守卫阻止导航

        // 创建或获取页面和视图模型
        var routeInfo = await CreateOrGetRouteInfo(matchingModel, route, mergedParameters);
        if (routeInfo == null)
        {
            return null;
        }

        // 添加到导航栈
        // NavigationStack.Push(routeInfo);

        return routeInfo;
    }

    /// <summary>
    /// 替换当前路由
    /// </summary>
    /// <param name="route">目标路由</param>
    /// <param name="parameters">路由参数，可为null</param>
    /// <returns>当前路由信息</returns>
    public async Task<RouteCurrentInfo?> ReplaceAsync(string route, Dictionary<string, object?>? parameters = null)
    {
        if (NavigationStack.Count > 0)
        {
            // 移除当前页面
            var current = NavigationStack.Pop();

            // 如果当前页面不是保活页面，从缓存中移除
            if (current.RoutingModel?.IsKeepAlive == false)
            {
                _keepAliveCache.RemoveCache(current.RoutePath ?? string.Empty);
            }
        }

        // 导航到新页面
        return await GoToAsync(route, parameters);
    }

    /// <summary>
    /// 返回上一页
    /// </summary>
    /// <returns>当前路由信息</returns>
    public Task<RouteCurrentInfo?> GoBackAsync()
    {
        if (NavigationStack.Count <= 1)
        {
            // 如果只有一个页面或没有页面，返回null
            return Task.FromResult<RouteCurrentInfo?>(null);
        }

        // 移除当前页面
        var current = NavigationStack.Pop();

        // 如果当前页面不是保活页面，从缓存中移除
        if (current.RoutingModel is { IsKeepAlive: false, IsTopNavigation: false })
        {
            _keepAliveCache.RemoveCache(current.RoutePath ?? string.Empty);
        }

        // 返回栈顶页面
        return Task.FromResult(NavigationStack.Peek())!;
    }

    /// <summary>
    /// 返回根页面
    /// </summary>
    /// <returns>当前路由信息</returns>
    public Task<RouteCurrentInfo?> GoToRootAsync()
    {
        if (NavigationStack.Count == 0)
        {
            return Task.FromResult<RouteCurrentInfo?>(null);
        }

        // 保留根页面，移除其他所有页面
        var root = NavigationStack.Last();
        NavigationStack.Clear();
        NavigationStack.Push(root);

        return Task.FromResult(root)!;
    }

    /// <summary>
    /// 获取当前路由信息
    /// </summary>
    /// <returns>当前路由信息，可为null</returns>
    public RouteCurrentInfo? GetCurrentRouteInfo()
    {
        return NavigationStack.Count > 0 ? NavigationStack.Peek() : null;
    }

    /// <summary>
    /// 清除路由历史
    /// </summary>
    public void ClearHistory()
    {
        // 保留当前页面，清除历史
        if (NavigationStack.Count > 0)
        {
            var current = NavigationStack.Pop();
            NavigationStack.Clear();
            NavigationStack.Push(current);
        }
        else
        {
            NavigationStack.Clear();
        }
    }

    /// <summary>
    /// 执行所有路由守卫检查
    /// </summary>
    /// <param name="targetRoute">目标路由</param>
    /// <param name="parameters">导航参数</param>
    /// <returns>是否允许导航</returns>
    private async Task<bool> CheckRouteGuardsAsync(RoutingModel targetRoute, Dictionary<string, object?>? parameters)
    {
        foreach (var guard in _routeGuards)
        {
            // 调用守卫的检查方法
            if (!await guard.CanNavigateAsync(targetRoute, parameters))
            {
                // 执行导航被阻止的处理
                await guard.OnNavigationBlockedAsync(targetRoute, parameters);
                return false; // 只要有一个守卫阻止，就返回false
            }
        }

        return true; // 所有守卫都允许导航
    }

    /// <summary>
    /// 查找匹配的路由模型
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <returns>匹配的路由模型和解析到的参数</returns>
    private (RoutingModel? Model, Dictionary<string, object?> Parameters) FindMatchingRoute(string route)
    {
        var parameters = new Dictionary<string, object?>();

        foreach (var model in _routingModels)
        {
            if (_routeParser.Match(route, model.Route, out var routeParams))
            {
                // 转换参数为对象字典
                if (routeParams != null)
                {
                    foreach (var (key, value) in routeParams)
                    {
                        parameters[key] = value;
                    }
                }

                // 解析查询参数
                var queryParams = RouteParameterHelper.ParseQueryParameters(route);
                foreach (var (key, value) in queryParams)
                {
                    parameters[key] = value;
                }

                return (model, parameters);
            }
        }

        return (null, parameters);
    }

    /// <summary>
    /// 合并多个参数字典
    /// </summary>
    /// <param name="dictionaries">参数字典数组</param>
    /// <returns>合并后的参数字典</returns>
    private Dictionary<string, object?> MergeParameters(params Dictionary<string, object?>?[] dictionaries)
    {
        var result = new Dictionary<string, object?>();

        foreach (var dict in dictionaries)
        {
            if (dict == null) continue;

            foreach (var (key, value) in dict)
            {
                result[key] = value;
            }
        }

        return result;
    }

    /// <summary>
    /// 创建或获取路由信息（包括页面和视图模型）
    /// 优化点：检查当前顶级路由栈中是否已有相同页面模型实例，避免重复入栈
    /// </summary>
    /// <param name="model">路由模型</param>
    /// <param name="route">路由路径</param>
    /// <param name="parameters">参数字典</param>
    /// <returns>路由信息（已存在则返回现有实例，否则返回新实例）</returns>
    private async Task<RouteCurrentInfo?> CreateOrGetRouteInfo(RoutingModel model, string route,
        Dictionary<string, object?> parameters)
    {
        object? page = null;
        object? viewModel = null;
        // 顶级路由直接返回其栈中最新实例（栈顶元素）
        if (model.IsTopNavigation)
        {
            _thisTopNavigationId = model.RoutingId;
            if (NavigationRoutes.TryGetValue(model.RoutingId, out var topStack) && topStack.Count > 0)
            {
                var t = topStack.Peek(); // 修复：使用Peek()获取栈顶元素（最新实例）
                page = t.Page;
                viewModel = t.ViewModel;
            }
        }

        if (page == null)
        {
            // 1. 处理保活页面（从缓存获取或创建新实例）
            if (model.IsKeepAlive && _keepAliveCache.TryGet(route, out var cachedPage, out var cachedViewModel))
            {
                page = cachedPage;
                viewModel = cachedViewModel;
            }
            else
            {
                // 创建新页面实例（失败则返回null）
                page = CreatePageInstance(model);
                if (page == null) return null;

                // 创建视图模型并关联到页面
                viewModel = CreateViewModelInstance(model);
                if (page is Control control && viewModel != null)
                {
                    control.DataContext = viewModel;
                }

                // 保活页面添加到缓存
                if (model.IsKeepAlive)
                {
                    _keepAliveCache.AddOrUpdate(route, page, viewModel);
                }
            }
        }

        // 2. 传递参数（优先ViewModel，其次页面）
        ApplyParametersToTarget(viewModel, page, parameters);

        // 3. 创建路由信息实例
        var routeInfo = new RouteCurrentInfo
        {
            RoutingModel = model,
            Page = page,
            ViewModel = viewModel,
            Parameters = parameters,
            RoutePath = route
        };

        // 4. 处理导航栈（核心优化：检查当前顶级栈是否已有相同页面模型）
        var targetStackId = model.IsTopNavigation ? model.RoutingId : _thisTopNavigationId;

        // 初始化目标栈（顶级路由栈或当前关联的顶级栈）
        var targetStack = NavigationRoutes.GetOrAdd(targetStackId, _ => new Stack<RouteCurrentInfo>());

        // 检查栈中是否已有相同页面模型的实例（核心逻辑）
        var existingRoute = targetStack.FirstOrDefault(
            r => r.RoutingModel != null
                 && r.RoutingModel.PageType == model.PageType
                 && string.Equals(r.RoutingModel.Route, model.Route, StringComparison.OrdinalIgnoreCase)
        );

        // 5. 无重复实例，添加到目标栈
        if (model.IsTopNavigation)
        {
            // 顶级路由：切换当前顶级ID，压入新实例
            _thisTopNavigationId = model.RoutingId;
        }
        else
        {
            // 普通路由：确保关联到有效顶级ID，压入新实例
            if (_thisTopNavigationId == Guid.Empty)
            {
                _thisTopNavigationId = model.RoutingId;
            }
        }

        if (existingRoute != null)
        {
            // 已有相同页面模型实例，更新参数后返回（修复：添加参数更新逻辑）
            ApplyParametersToTarget(existingRoute.ViewModel, existingRoute.Page, parameters);
            existingRoute.Parameters = new Dictionary<string, object?>(parameters); // 深拷贝新参数
            return existingRoute;
        }

        targetStack.Push(routeInfo);

        return await Task.FromResult(routeInfo);
    }

    /// <summary>
    /// 向目标（视图模型或页面）应用参数
    /// </summary>
    private void ApplyParametersToTarget(object? viewModel, object? page, Dictionary<string, object?> parameters)
    {
        if (viewModel is IQueryAttributable queryAttributable)
        {
            queryAttributable.ApplyQueryAttributes(parameters);
        }
        else if (page is IQueryAttributable pageQueryAttributable)
        {
            pageQueryAttributable.ApplyQueryAttributes(parameters);
        }
    }

    /// <summary>
    /// 创建页面实例
    /// </summary>
    /// <param name="model">路由模型</param>
    /// <returns>页面实例，可为null</returns>
    private object? CreatePageInstance(RoutingModel model)
    {
        try
        {
            // 优先使用DI容器创建实例
            if (_serviceProvider != null)
            {
                return ActivatorUtilities.CreateInstance(_serviceProvider, model.PageType);
            }

            // 否则使用反射创建实例
            return Activator.CreateInstance(model.PageType);
        }
        catch (Exception ex)
        {
            // 记录异常日志
            Debug.WriteLine($"创建页面实例失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 创建视图模型实例（支持null）
    /// </summary>
    /// <param name="model">路由模型</param>
    /// <returns>视图模型实例，可为null</returns>
    private object? CreateViewModelInstance(RoutingModel model)
    {
        // 如果视图模型类型为null，直接返回null
        if (model.ViewModelType == null)
        {
            return null;
        }

        try
        {
            // 优先使用DI容器创建实例
            if (_serviceProvider != null)
            {
                return ActivatorUtilities.CreateInstance(_serviceProvider, model.ViewModelType);
            }

            // 否则使用反射创建实例
            return Activator.CreateInstance(model.ViewModelType);
        }
        catch (Exception ex)
        {
            // 记录异常日志
            Debug.WriteLine($"创建视图模型实例失败: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc/>
    public bool CanGoBack()
    {
        return NavigationStack.Count > 1;
    }
}