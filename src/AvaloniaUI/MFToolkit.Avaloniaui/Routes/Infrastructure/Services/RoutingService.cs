using System.Collections.Concurrent;
using System.Reflection;
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

    private static RoutingService? _defaultInstance;

    private static readonly List<RoutingModel> RoutingModels = [];

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
    public static readonly RoutingService Default = _defaultInstance ??= new RoutingService(
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
        _defaultInstance = this;
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
        if (RoutingModels.Any(m => string.Equals(m.Route, model.Route, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"路由 '{model.Route}' 已存在");
        }

        RoutingModels.Add(model);

        // 按优先级排序，确保高优先级路由先匹配
        RoutingModels.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    /// <summary>
    /// 页面类型获取路由
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string? PageTypeToRoute(Type type)
    {
        var query = RoutingModels.FirstOrDefault(q => q.PageType == type);
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

        foreach (var model in RoutingModels)
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
        // 顶级路由直接返回其栈中最新实例（栈顶元素）
        if (model.IsTopNavigation)
        {
            _thisTopNavigationId = model.RoutingId;
            if (NavigationRoutes.TryGetValue(model.RoutingId, out var topStack) && topStack.Count > 0)
            {
                var t = topStack.Peek(); // 修复：使用Peek()获取栈顶元素（最新实例）
                ApplyParametersToTarget(t.ViewModel, t.Page, parameters);
                return t;
            }
        }

        object? page = null;
        object? viewModel = null;
        
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
        // if (model.IsTopNavigation)
        // {
        //     TopNavigations.TryAdd(model.RoutingId, routeInfo);
        // }
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
    private static void ApplyParametersToTarget(object? viewModel, object? page, Dictionary<string, object?> parameters)
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

    /// <inheritdoc/>
    public bool CanGoBack()
    {
        return NavigationStack.Count > 1;
    }

    /// <summary>
    /// 创建页面实例
    /// </summary>
    /// <param name="model">路由模型，包含页面类型信息</param>
    /// <returns>页面实例</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="model"/> 为 null 时抛出</exception>
    /// <exception cref="InvalidOperationException">当页面创建失败时抛出</exception>
    private object CreatePageInstance(RoutingModel model)
    {
        // 严格的 null 检查
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(model.PageType);

        // 如果没有DI容器，直接使用反射创建
        if (_serviceProvider == null)
        {
            return CreateInstanceDirectly(model.PageType);
        }

        try
        {
            // 首先尝试直接从DI容器获取（快速路径）
            return _serviceProvider.GetRequiredService(model.PageType);
        }
        catch (InvalidOperationException)
        {
            // 如果页面类型本身未在DI容器中注册，需要手动构造
            return CreateUnregisteredTypeWithDependencies(model.PageType);
        }
    }

    /// <summary>
    /// 创建视图模型实例
    /// </summary>
    /// <param name="model">路由模型，包含视图模型类型信息</param>
    /// <returns>视图模型实例，如果 ViewModelType 为 null 则返回 null</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="model"/> 为 null 时抛出</exception>
    /// <exception cref="InvalidOperationException">当视图模型创建失败时抛出</exception>
    private object? CreateViewModelInstance(RoutingModel model)
    {
        // 严格的 null 检查
        ArgumentNullException.ThrowIfNull(model);

        // 如果未指定视图模型类型，直接返回 null
        if (model.ViewModelType is null)
        {
            return null;
        }

        // 如果没有DI容器，直接使用反射创建
        if (_serviceProvider == null)
        {
            return CreateInstanceDirectly(model.ViewModelType);
        }

        try
        {
            // 首先尝试直接从DI容器获取（快速路径）
            return _serviceProvider.GetRequiredService(model.ViewModelType);
        }
        catch (InvalidOperationException)
        {
            // 如果视图模型类型本身未在DI容器中注册，手动构造
            return CreateUnregisteredTypeWithDependencies(model.ViewModelType);
        }
    }

    /// <summary>
    /// 直接使用 Activator 创建实例（无依赖注入）
    /// </summary>
    /// <param name="type">要创建实例的类型</param>
    /// <returns>创建的实例</returns>
    /// <exception cref="InvalidOperationException">当创建实例失败时抛出</exception>
    private static object CreateInstanceDirectly(Type type)
    {
        try
        {
            return Activator.CreateInstance(type)
                   ?? throw new InvalidOperationException($"Activator.CreateInstance 返回 null 对于类型: {type}");
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException($"类型 {type} 没有公共的无参构造函数或依赖无法满足", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"创建 {type} 实例失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 为未在DI容器中注册的类型创建实例，同时正确处理其依赖关系
    /// </summary>
    /// <param name="type">要创建实例的类型</param>
    /// <returns>创建的实例</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="type"/> 为 null 时抛出</exception>
    /// <exception cref="InvalidOperationException">当无法解析必需依赖或创建实例失败时抛出</exception>
    private object CreateUnregisteredTypeWithDependencies(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // 获取所有公共构造函数，选择参数最多的一个
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        if (constructors.Length == 0)
        {
            throw new InvalidOperationException($"类型 {type} 没有公共构造函数");
        }

        // 选择参数最多的构造函数（通常是主要的DI构造函数）
        var constructor = constructors
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        var parameters = constructor.GetParameters();

        // 如果没有参数，直接创建实例
        if (parameters.Length == 0)
        {
            return CreateInstanceDirectly(type);
        }

        var arguments = new object[parameters.Length];

        // 逐个解析构造函数参数
        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            arguments[i] = ResolveParameter(parameter);
        }

        // 使用解析好的参数创建实例
        try
        {
            return constructor.Invoke(arguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            // 解包 TargetInvocationException 以显示真实的异常信息
            throw new InvalidOperationException($"创建 {type} 实例失败: {ex.InnerException.Message}", ex.InnerException);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"创建 {type} 实例失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 解析单个构造函数参数
    /// </summary>
    /// <param name="parameter">参数信息</param>
    /// <returns>解析后的参数值</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="parameter"/> 为 null 时抛出</exception>
    /// <exception cref="InvalidOperationException">当无法解析必需依赖时抛出</exception>
    private object ResolveParameter(ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        var parameterType = parameter.ParameterType;

        // 尝试从DI容器获取服务
        var service = _serviceProvider!.GetService(parameterType);

        if (service is not null)
        {
            return service;
        }

        // DI容器中没有找到服务，检查是否有默认值
        if (parameter.HasDefaultValue)
        {
            return parameter.DefaultValue!;
        }

        // 检查参数是否可为null
        if (IsNullableParameter(parameter))
        {
            return null!;
        }

        // 无法解析必需依赖，抛出详细错误信息
        throw new InvalidOperationException(
            $$"""
              无法解析必需依赖: {{parameterType}}
              位置: {{parameter.Member.DeclaringType?.Name}} 的构造函数参数 "{{parameter.Name}}"
              解决方案:
                1. 在DI容器中注册 {{parameterType}} 服务
                2. 为参数 {{parameter.Name}} 提供默认值
                3. 将参数 {{parameter.Name}} 改为可空类型 ({{parameterType}}?)
              """);
    }

    /// <summary>
    /// 检查参数是否可为null
    /// </summary>
    /// <param name="parameter">参数信息</param>
    /// <returns>如果参数可为null返回true，否则返回false</returns>
    private static bool IsNullableParameter(ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        var parameterType = parameter.ParameterType;

        // 检查 Nullable<T> 泛型类型
        if (Nullable.GetUnderlyingType(parameterType) is not null)
        {
            return true;
        }

        // 对于引用类型，默认可为null（除非有NotNullAttribute）
        if (!parameterType.IsValueType)
        {
            // 在 .NET 8+ 中，可以使用 NullabilityInfoContext 进行更精确的检查
            try
            {
                var nullabilityInfo = new NullabilityInfoContext().Create(parameter);
                return nullabilityInfo.WriteState is not NullabilityState.NotNull;
            }
            catch
            {
                // 如果 NullabilityInfoContext 失败，回退到基本检查
                return true;
            }
        }

        return false;
    }
}