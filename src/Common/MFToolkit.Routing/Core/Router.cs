using MFToolkit.Routing.Core;
using MFToolkit.Routing.Core.Interfaces;
using MFToolkit.Routing.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Routing;

/// <summary>
/// 路由导航器实现
/// </summary>
public class Router : IRouter
{
    private readonly RouteRegistry _registry;
    private readonly RouteStackManager _stackManager;
    private readonly IEnumerable<IRouteGuard> _guards;
    private readonly IServiceProvider? _serviceProvider;
    private readonly SemaphoreSlim _navigationLock = new(1, 1);

    // === 事件 ===

    /// <inheritdoc />
    public event EventHandler<NavigationEventArgs>? NavigationStarting;

    /// <inheritdoc />
    public event EventHandler<NavigationEventArgs>? Navigated;

    /// <inheritdoc />
    public event EventHandler<NavigationEventArgs>? NavigationFailed;

    // === 状态 ===

    /// <inheritdoc />
    public Guid CurrentTopRouteId => _stackManager.CurrentTopRouteId;

    /// <inheritdoc />
    public RouteEntry? CurrentRoute => _stackManager.CurrentStack.Current;

    /// <inheritdoc />
    public IReadOnlyList<RouteEntry> CurrentStack => _stackManager.CurrentStack.History;

    /// <inheritdoc />
    public bool CanGoBack => _stackManager.CurrentStack.CanGoBack;

    /// <inheritdoc />
    public int StackDepth => _stackManager.CurrentStack.Count;

    /// <inheritdoc />
    public IReadOnlyList<RouteEntity> RegisteredTopRoutes => _stackManager.RegisteredTopRoutes;

    /// <inheritdoc />
    public bool IsUsingDefaultTopRoute => _stackManager.IsUsingDefaultTopRoute;

    /// <summary>
    /// 栈管理器（仅供内部使用）
    /// </summary>
    internal RouteStackManager StackManager => _stackManager;

    /// <summary>
    /// 创建一个新的 Router 实例
    /// </summary>
    /// <param name="guards">路由守卫集合</param>
    /// <param name="routes">已注册的路由集合</param>
    /// <param name="serviceProvider">服务提供者（用于创建 ViewModel）</param>
    public Router(IEnumerable<IRouteGuard>? guards, IEnumerable<RouteEntity>? routes, IServiceProvider? serviceProvider)
    {
        _registry = new RouteRegistry();
        _stackManager = new RouteStackManager();
        _guards = guards ?? Enumerable.Empty<IRouteGuard>();
        _serviceProvider = serviceProvider;

        // 注册路由
        if (routes != null)
        {
            foreach (var route in routes)
            {
                _registry.Register(route);

                // 如果是顶级路由，注册到栈管理器
                if (route.IsTop)
                {
                    _stackManager.RegisterTopRoute(route);
                }
            }
        }
    }

    // === 导航 ===

    /// <inheritdoc />
    public async Task<NavigationResult> NavigateAsync(string routeKey, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push)
    {
        await _navigationLock.WaitAsync();
        try
        {
            RouteEntity? route;
            Dictionary<string, object?>? resolvedParams = null;

            // 尝试直接查找路由
            route = _registry.FindByKey(routeKey);
            if (route != null)
            {
                // 直接匹配，使用传入参数
                resolvedParams = parameters;
            }
            else
            {
                // 尝试路径参数匹配
                // 遍历所有路由尝试匹配
                foreach (var registered in _registry.GetAll())
                {
                    if (registered.RoutePath != null && RouteParser.HasParameters(registered.RoutePath))
                    {
                        var pathParams = RouteParser.ParseParameters(registered.RoutePath, routeKey);
                        if (pathParams != null)
                        {
                            route = registered;
                            // 合并路径参数和字典参数
                            resolvedParams = MergeParameters(pathParams, parameters);
                            break;
                        }
                    }
                }
            }

            if (route == null)
            {
                return NavigationResult.NotFound(routeKey);
            }

            return await NavigateInternalAsync(route, resolvedParams, action);
        }
        catch (Exception ex)
        {
            return NavigationResult.Error(ex);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<NavigationResult> NavigateAsync(Type pageType, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push)
    {
        await _navigationLock.WaitAsync();
        try
        {
            // 1. 查找路由
            var route = _registry.FindByType(pageType);
            if (route == null)
            {
                return NavigationResult.NotFound(pageType.Name);
            }

            return await NavigateInternalAsync(route, parameters, action);
        }
        catch (Exception ex)
        {
            return NavigationResult.Error(ex);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <inheritdoc />
    public Task<NavigationResult> NavigateAsync<T>(Dictionary<string, object?>? parameters = null, string action = NavigationActions.Push) where T : class
    {
        return NavigateAsync(typeof(T), parameters, action);
    }

    private async Task<NavigationResult> NavigateInternalAsync(RouteEntity route, Dictionary<string, object?>? parameters, string action = NavigationActions.Push)
    {
        var from = CurrentRoute;
        var toParameters = MergeParameters(route, parameters);

        // 触发 NavigationStarting 事件
        var toEntry = new RouteEntry(route, toParameters);
        OnNavigationStarting(action, from, toEntry);

        // 守卫检查
        foreach (var guard in _guards)
        {
            if (!await guard.CanNavigateAsync(route, toParameters))
            {
                await guard.OnNavigationBlockedAsync(route, toParameters);
                OnNavigationFailed(action, from, toEntry, NavigationStatus.Blocked, "导航被守卫阻止");
                return NavigationResult.Blocked(route);
            }
        }

        // === 判断是否需要切换顶级路由 ===
        if (route.IsTop)
        {
            // 如果已经是这个 IsTop 页面（即 CurrentTopRouteId == route.Id），直接返回不做任何操作
            if (CurrentTopRouteId == route.Id)
            {
                return NavigationResult.Success(route);
            }
            // 切换到目标顶级路由的栈
            _stackManager.SwitchTopRoute(route.Id);
            var targetStack = _stackManager.CurrentStack;

            if (targetStack.Count > 0)
            {
                // 栈已有页面，激活最后一页（切换行为）
                var lastEntry = targetStack.Current;
                if (lastEntry != null)
                {
                    // 触发 OnNavigatingFrom（当前页）
                    if (from?.PageInstance is INavigationAware currentAware)
                    {
                        currentAware.OnNavigatingFrom();
                    }

                    ActivateEntry(lastEntry);
                    OnNavigated(NavigationActions.SwitchTop, from, lastEntry);
                }
            }
            else
            {
                // 栈为空，Push 顶级路由页面（首次进入该顶级路由）
                // 触发 OnNavigatingFrom（当前页）
                if (from?.PageInstance is INavigationAware currentAware2)
                {
                    currentAware2.OnNavigatingFrom();
                }

                toEntry.PageInstance = route.RouteType != null && _serviceProvider != null
                    ? _serviceProvider.GetRequiredService(route.RouteType)
                    : null;

                toEntry.ViewModelInstance = route.ViewModelType != null && _serviceProvider != null
                    ? _serviceProvider.GetRequiredService(route.ViewModelType)
                    : null;

                if (toEntry.ViewModelInstance is IQueryAttributable vmAttr && toParameters != null)
                {
                    vmAttr.ApplyQueryAttributes(toParameters);
                }

                targetStack.Push(toEntry);
                ActivateEntry(toEntry);
                OnNavigated(NavigationActions.SwitchTop, from, toEntry);
            }

            return NavigationResult.Success(route);
        }

        // === 普通 Push 逻辑 ===
        // 处理当前页面 OnNavigatingFrom
        if (from?.PageInstance is INavigationAware currentAware3)
        {
            currentAware3.OnNavigatingFrom();
        }

        // 从 DI 获取 Page 实例
        if (route.RouteType != null && _serviceProvider != null)
        {
            toEntry.PageInstance = _serviceProvider.GetRequiredService(route.RouteType);
        }

        // 创建 ViewModel
        if (route.ViewModelType != null && _serviceProvider != null)
        {
            toEntry.ViewModelInstance = _serviceProvider.GetRequiredService(route.ViewModelType);

            if (toEntry.ViewModelInstance is IQueryAttributable attributable && toParameters != null)
            {
                attributable.ApplyQueryAttributes(toParameters);
            }
        }

        // 入栈
        _stackManager.CurrentStack.Push(toEntry);

        // 触发目标页面 OnNavigated
        ActivateEntry(toEntry);

        // 触发 Navigated 事件
        OnNavigated(action, from, toEntry);

        return NavigationResult.Success(route);
    }

    // === 后退 ===

    /// <inheritdoc />
    public async Task<NavigationResult> GoBackAsync(string action = NavigationActions.Pop)
    {
        await _navigationLock.WaitAsync();
        try
        {
            if (!CanGoBack)
            {
                return NavigationResult.Cancelled("无法返回：栈中只有一个条目");
            }

            var from = CurrentRoute;
            if (from == null)
            {
                return NavigationResult.Cancelled("当前没有路由");
            }

            // 触发 NavigationStarting 事件
            OnNavigationStarting(action, from, null);

            // 1. 触发 OnNavigatingFrom
            if (from.PageInstance is INavigationAware currentAware)
            {
                currentAware.OnNavigatingFrom();
            }

            // 2. 处理 KeepAlive
            if (!from.Entity.IsKeepalive)
            {
                // 触发 OnNavigatedFrom
                if (from.PageInstance is INavigationAware fromAware)
                {
                    fromAware.OnNavigatedFrom();
                }

                // 销毁实例
                DisposePage(from);
            }
            else
            {
                // KeepAlive 页面：只触发 OnNavigatedFrom，不销毁
                if (from.PageInstance is INavigationAware fromAware)
                {
                    fromAware.OnNavigatedFrom();
                }
            }

            // 3. 出栈
            var popped = _stackManager.CurrentStack.Pop();

            // 4. 激活新的栈顶
            var newTop = CurrentRoute;
            if (newTop != null)
            {
                // 如果实例为 null，需要通知框架重建
                // 框架应订阅 Navigated 事件来处理
                if (newTop.PageInstance == null)
                {
                    // 触发重建请求（框架侧处理）
                    OnNavigationFailed(NavigationActions.Pop, newTop, null, NavigationStatus.Cancelled, "需要重建页面实例");
                }
                else
                {
                    // 激活已有实例
                    if (newTop.PageInstance is INavigationAware newAware)
                    {
                        newAware.OnNavigated(newTop.Parameters);
                    }

                    // 触发 ViewModel 的 IQueryAttributable 参数注入
                    if (newTop.ViewModelInstance is IQueryAttributable vmAttributable && newTop.Parameters != null)
                    {
                        vmAttributable.ApplyQueryAttributes(newTop.Parameters);
                    }

                    // 触发 Page 的 IQueryAttributable 参数注入
                    if (newTop.PageInstance is IQueryAttributable pageAttributable && newTop.Parameters != null)
                    {
                        pageAttributable.ApplyQueryAttributes(newTop.Parameters);
                    }
                }
            }

            // 5. 触发 Navigated 事件
            OnNavigated(action, popped, newTop);

            return NavigationResult.Success(from.Entity);
        }
        catch (Exception ex)
        {
            return NavigationResult.Error(ex);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<NavigationResult> GoBackToRootAsync(string action = NavigationActions.PopToRoot)
    {
        await _navigationLock.WaitAsync();
        try
        {
            var from = CurrentRoute;
            if (from == null)
            {
                return NavigationResult.Cancelled("当前没有路由");
            }

            var stack = _stackManager.CurrentStack;

            // 逐个处理直到只剩一个
            while (stack.Count > 1)
            {
                var entry = stack.Current;
                if (entry == null) break;

                // 触发 OnNavigatingFrom
                if (entry.PageInstance is INavigationAware aware)
                {
                    aware.OnNavigatingFrom();
                }

                // 处理 KeepAlive
                if (!entry.Entity.IsKeepalive)
                {
                    if (entry.PageInstance is INavigationAware fromAware)
                    {
                        fromAware.OnNavigatedFrom();
                    }
                    DisposePage(entry);
                }
                else
                {
                    if (entry.PageInstance is INavigationAware fromAware)
                    {
                        fromAware.OnNavigatedFrom();
                    }
                }

                stack.Pop();
            }

            // 激活栈顶
            var newTop = CurrentRoute;
            if (newTop != null)
            {
                if (newTop.PageInstance is INavigationAware newAware)
                {
                    newAware.OnNavigated(newTop.Parameters);
                }

                // 触发 ViewModel 的 IQueryAttributable 参数注入
                if (newTop.ViewModelInstance is IQueryAttributable vmAttributable && newTop.Parameters != null)
                {
                    vmAttributable.ApplyQueryAttributes(newTop.Parameters);
                }

                // 触发 Page 的 IQueryAttributable 参数注入
                if (newTop.PageInstance is IQueryAttributable pageAttributable && newTop.Parameters != null)
                {
                    pageAttributable.ApplyQueryAttributes(newTop.Parameters);
                }
            }

            OnNavigated(action, from, newTop);

            return NavigationResult.Success(from.Entity);
        }
        catch (Exception ex)
        {
            return NavigationResult.Error(ex);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    // === 栈管理 ===

    /// <inheritdoc />
    public async Task<NavigationResult> GoBackToAsync(string routeKey, string action = NavigationActions.PopToPage)
    {
        await _navigationLock.WaitAsync();
        try
        {
            var from = CurrentRoute;
            if (from == null)
            {
                return NavigationResult.Cancelled("当前没有路由");
            }

            // 查找目标路由在栈中的位置
            var targetIndex = -1;
            for (int i = _stackManager.CurrentStack.Count - 1; i >= 0; i--)
            {
                var entry = _stackManager.CurrentStack.History[i];
                if (entry.Entity.RouteKey == routeKey || entry.Entity.RoutePath == routeKey)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex < 0)
            {
                return NavigationResult.NotFound(routeKey);
            }

            // 如果目标就是当前页，不需要操作
            if (targetIndex == _stackManager.CurrentStack.Count - 1)
            {
                return NavigationResult.Success(from.Entity);
            }

            // 触发 NavigationStarting 事件
            OnNavigationStarting(action, from, null);

            // 逐个弹出直到目标页
            while (_stackManager.CurrentStack.Count > targetIndex + 1)
            {
                var entry = _stackManager.CurrentStack.Current;
                if (entry == null) break;

                // 触发 OnNavigatingFrom
                if (entry.PageInstance is INavigationAware aware)
                {
                    aware.OnNavigatingFrom();
                }

                // 处理 KeepAlive
                if (!entry.Entity.IsKeepalive)
                {
                    if (entry.PageInstance is INavigationAware fromAware)
                    {
                        fromAware.OnNavigatedFrom();
                    }
                    DisposePage(entry);
                }
                else
                {
                    if (entry.PageInstance is INavigationAware fromAware)
                    {
                        fromAware.OnNavigatedFrom();
                    }
                }

                _stackManager.CurrentStack.Pop();
            }

            // 激活栈顶
            var newTop = CurrentRoute;
            if (newTop != null)
            {
                if (newTop.PageInstance is INavigationAware newAware)
                {
                    newAware.OnNavigated(newTop.Parameters);
                }

                // 触发 ViewModel 的 IQueryAttributable 参数注入
                if (newTop.ViewModelInstance is IQueryAttributable vmAttributable && newTop.Parameters != null)
                {
                    vmAttributable.ApplyQueryAttributes(newTop.Parameters);
                }

                // 触发 Page 的 IQueryAttributable 参数注入
                if (newTop.PageInstance is IQueryAttributable pageAttributable && newTop.Parameters != null)
                {
                    pageAttributable.ApplyQueryAttributes(newTop.Parameters);
                }
            }

            OnNavigated(action, from, newTop);

            return NavigationResult.Success(from.Entity);
        }
        catch (Exception ex)
        {
            return NavigationResult.Error(ex);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<NavigationResult> GoBackToAsync(Type pageType, string action = NavigationActions.PopToPage)
    {
        var route = _registry.FindByType(pageType);
        if (route == null)
        {
            return NavigationResult.NotFound(pageType.Name);
        }

        return await GoBackToAsync(route.RouteKey, action);
    }

    /// <inheritdoc />
    public Task<NavigationResult> GoBackToAsync<T>(string action = NavigationActions.PopToPage) where T : class
    {
        return GoBackToAsync(typeof(T), action);
    }

    /// <inheritdoc />
    public async Task<NavigationResult> ReplaceAsync(string routeKey, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace)
    {
        await _navigationLock.WaitAsync();
        try
        {
            var from = CurrentRoute;
            if (from == null)
            {
                return NavigationResult.Cancelled("当前没有路由");
            }

            RouteEntity? route;
            Dictionary<string, object?>? resolvedParams = null;

            // 尝试直接查找路由
            route = _registry.FindByKey(routeKey);
            if (route != null)
            {
                resolvedParams = parameters;
            }
            else
            {
                // 尝试路径参数匹配
                foreach (var registered in _registry.GetAll())
                {
                    if (registered.RoutePath != null && RouteParser.HasParameters(registered.RoutePath))
                    {
                        var pathParams = RouteParser.ParseParameters(registered.RoutePath, routeKey);
                        if (pathParams != null)
                        {
                            route = registered;
                            resolvedParams = MergeParameters(pathParams, parameters);
                            break;
                        }
                    }
                }
            }

            if (route == null)
            {
                return NavigationResult.NotFound(routeKey);
            }

            // 触发 NavigationStarting 事件
            var toEntry = new RouteEntry(route, resolvedParams);
            OnNavigationStarting(action, from, toEntry);

            // 守卫检查
            foreach (var guard in _guards)
            {
                if (!await guard.CanNavigateAsync(route, resolvedParams))
                {
                    await guard.OnNavigationBlockedAsync(route, resolvedParams);
                    OnNavigationFailed(action, from, toEntry, NavigationStatus.Blocked, "导航被守卫阻止");
                    return NavigationResult.Blocked(route);
                }
            }

            // 处理当前页面
            if (from.PageInstance is INavigationAware currentAware)
            {
                currentAware.OnNavigatingFrom();
            }

            if (!from.Entity.IsKeepalive)
            {
                if (from.PageInstance is INavigationAware fromAware)
                {
                    fromAware.OnNavigatedFrom();
                }
                DisposePage(from);
            }
            else
            {
                if (from.PageInstance is INavigationAware fromAware)
                {
                    fromAware.OnNavigatedFrom();
                }
            }

            // 出栈当前
            _stackManager.CurrentStack.Pop();

            // 从 DI 获取 Page 实例（如果已注册）
            if (route.RouteType != null && _serviceProvider != null)
            {
                toEntry.PageInstance = _serviceProvider.GetRequiredService(route.RouteType);
            }

            // 创建 ViewModel
            if (route.ViewModelType != null && _serviceProvider != null)
            {
                toEntry.ViewModelInstance = _serviceProvider.GetRequiredService(route.ViewModelType);

                if (toEntry.ViewModelInstance is IQueryAttributable attributable && resolvedParams != null)
                {
                    attributable.ApplyQueryAttributes(resolvedParams);
                }
            }

            // 入栈新页面
            _stackManager.CurrentStack.Push(toEntry);

            // 触发目标页面 OnNavigated
            if (toEntry.PageInstance is INavigationAware targetAware)
            {
                targetAware.OnNavigated(resolvedParams);
            }

            if (toEntry.PageInstance is IQueryAttributable pageAttributable && resolvedParams != null)
            {
                pageAttributable.ApplyQueryAttributes(resolvedParams);
            }

            OnNavigated(action, from, toEntry);

            return NavigationResult.Success(route);
        }
        catch (Exception ex)
        {
            return NavigationResult.Error(ex);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<NavigationResult> ReplaceAsync(Type pageType, Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace)
    {
        var route = _registry.FindByType(pageType);
        if (route == null)
        {
            return NavigationResult.NotFound(pageType.Name);
        }

        return await ReplaceAsync(route.RouteKey, parameters, action);
    }

    /// <inheritdoc />
    public Task<NavigationResult> ReplaceAsync<T>(Dictionary<string, object?>? parameters = null, string action = NavigationActions.Replace) where T : class
    {
        return ReplaceAsync(typeof(T), parameters, action);
    }

    /// <inheritdoc />
    public void SwitchTopRoute(Guid topRouteId)
    {
        _stackManager.SwitchTopRoute(topRouteId);
        OnNavigated(NavigationActions.SwitchTop, null, CurrentRoute);
    }

    // === 事件触发 ===

    private void OnNavigationStarting(string action, RouteEntry? from, RouteEntry? to)
    {
        NavigationStarting?.Invoke(this, NavigationEventArgs.Starting(action, from, to, to?.Parameters));
    }

    private void OnNavigated(string action, RouteEntry? from, RouteEntry? to)
    {
        Navigated?.Invoke(this, NavigationEventArgs.Navigated(action, from, to));
    }

    private void OnNavigationFailed(string action, RouteEntry? from, RouteEntry? to, NavigationStatus status, string? message)
    {
        NavigationFailed?.Invoke(this, NavigationEventArgs.Failed(action, from, to, status, message));
    }

    // === 辅助方法 ===

    private static Dictionary<string, object?>? MergeParameters(RouteEntity route, Dictionary<string, object?>? parameters)
    {
        // 合并默认参数和传入参数，传入参数优先
        var defaultParams = route.DefaultParameters;
        if (defaultParams == null && parameters == null)
            return null;

        var result = new Dictionary<string, object?>();

        if (defaultParams != null)
        {
            foreach (var kvp in defaultParams)
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result.Count > 0 ? result : null;
    }

    private static Dictionary<string, object?>? MergeParameters(Dictionary<string, object?>? primary, Dictionary<string, object?>? secondary)
    {
        if (primary == null && secondary == null)
            return null;

        var result = new Dictionary<string, object?>();

        if (primary != null)
        {
            foreach (var kvp in primary)
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        if (secondary != null)
        {
            foreach (var kvp in secondary)
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result.Count > 0 ? result : null;
    }

    private static void DisposePage(RouteEntry entry)
    {
        // 销毁 ViewModel
        if (entry.ViewModelInstance is IDisposable vmDisposable)
        {
            vmDisposable.Dispose();
        }
        entry.ViewModelInstance = null;

        // 销毁 Page
        if (entry.PageInstance is IDisposable pageDisposable)
        {
            pageDisposable.Dispose();
        }
        entry.PageInstance = null;
    }

    /// <summary>
    /// 激活一个路由条目（触发 OnNavigated 和 IQueryAttributable）
    /// </summary>
    private void ActivateEntry(RouteEntry entry)
    {
        if (entry.PageInstance is INavigationAware aware)
        {
            aware.OnNavigated(entry.Parameters);
        }

        if (entry.PageInstance is IQueryAttributable pageAttr && entry.Parameters != null)
        {
            pageAttr.ApplyQueryAttributes(entry.Parameters);
        }

        if (entry.ViewModelInstance is IQueryAttributable vmAttr && entry.Parameters != null)
        {
            vmAttr.ApplyQueryAttributes(entry.Parameters);
        }
    }
}
