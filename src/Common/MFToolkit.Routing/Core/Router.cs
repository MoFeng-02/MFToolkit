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
    public IReadOnlyList<RouteEntity> RegisteredTopRoutes => _stackManager.RegisteredTopRoutes;

    /// <inheritdoc />
    public bool IsUsingDefaultTopRoute => _stackManager.IsUsingDefaultTopRoute;

    /// <summary>
    /// 路由注册表
    /// </summary>
    public RouteRegistry Registry => _registry;

    /// <summary>
    /// 栈管理器
    /// </summary>
    public RouteStackManager StackManager => _stackManager;

    /// <summary>
    /// 创建一个新的 Router 实例
    /// </summary>
    /// <param name="guards">路由守卫集合</param>
    /// <param name="serviceProvider">服务提供者（用于创建 ViewModel）</param>
    public Router(IEnumerable<IRouteGuard>? guards = null, IServiceProvider? serviceProvider = null)
    {
        _registry = new RouteRegistry();
        _stackManager = new RouteStackManager();
        _guards = guards ?? Enumerable.Empty<IRouteGuard>();
        _serviceProvider = serviceProvider;
    }

    // === 注册 ===

    /// <inheritdoc />
    public void RegisterRoute(RouteEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _registry.Register(entity);

        // 如果是顶级路由，同时注册到栈管理器
        if (entity.IsTop)
        {
            _stackManager.RegisterTopRoute(entity);
        }
    }

    /// <inheritdoc />
    public void RegisterRoutes(IEnumerable<RouteEntity> entities)
    {
        foreach (var entity in entities)
        {
            RegisterRoute(entity);
        }
    }

    // === 导航 ===

    /// <inheritdoc />
    public async Task<NavigationResult> NavigateAsync(string routeKey, Dictionary<string, object?>? parameters = null)
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

            return await NavigateInternalAsync(route, resolvedParams);
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
    public async Task<NavigationResult> NavigateAsync(Type pageType, Dictionary<string, object?>? parameters = null)
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

            return await NavigateInternalAsync(route, parameters);
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
    public Task<NavigationResult> NavigateAsync<T>(Dictionary<string, object?>? parameters = null) where T : class
    {
        return NavigateAsync(typeof(T), parameters);
    }

    private async Task<NavigationResult> NavigateInternalAsync(RouteEntity route, Dictionary<string, object?>? parameters)
    {
        var from = CurrentRoute;
        var toParameters = MergeParameters(route, parameters);

        // 2. 触发 NavigationStarting 事件
        var toEntry = new RouteEntry(route, toParameters);
        OnNavigationStarting(from, toEntry);

        // 3. 守卫检查
        foreach (var guard in _guards)
        {
            if (!await guard.CanNavigateAsync(route, toParameters))
            {
                await guard.OnNavigationBlockedAsync(route, toParameters);
                OnNavigationFailed(from, toEntry, NavigationStatus.Blocked, "导航被守卫阻止");
                return NavigationResult.Blocked(route);
            }
        }

        // 4. 处理当前页面 OnNavigatingFrom
        if (from?.PageInstance is INavigationAware currentAware)
        {
            currentAware.OnNavigatingFrom();
        }

        // 5. 创建 ViewModel（如果注册了）
        if (route.ViewModelType != null && _serviceProvider != null)
        {
            toEntry.ViewModelInstance = _serviceProvider.GetRequiredService(route.ViewModelType);

            // 调用 IQueryAttributable 参数注入
            if (toEntry.ViewModelInstance is IQueryAttributable attributable && toParameters != null)
            {
                attributable.ApplyQueryAttributes(toParameters);
            }
        }

        // 6. 入栈
        _stackManager.CurrentStack.Push(toEntry);

        // 7. 触发目标页面 OnNavigated
        if (toEntry.PageInstance is INavigationAware targetAware)
        {
            targetAware.OnNavigated(toParameters);
        }

        // 7.1 触发 Page 的 IQueryAttributable 参数注入（如果 Page 也实现了）
        if (toEntry.PageInstance is IQueryAttributable pageAttributable && toParameters != null)
        {
            pageAttributable.ApplyQueryAttributes(toParameters);
        }

        // 8. 触发 Navigated 事件
        OnNavigated(from, toEntry);

        return NavigationResult.Success(route);
    }

    // === 后退 ===

    /// <inheritdoc />
    public async Task<NavigationResult> GoBackAsync()
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
            OnNavigationStarting(from, null);

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
                    OnNavigationFailed(newTop, null, NavigationStatus.Cancelled, "需要重建页面实例");
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
            OnNavigated(popped, newTop);

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
    public async Task<NavigationResult> GoBackToRootAsync()
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

            OnNavigated(from, newTop);

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
    public void SwitchTopRoute(Guid topRouteId)
    {
        _stackManager.SwitchTopRoute(topRouteId);
    }

    // === 事件触发 ===

    private void OnNavigationStarting(RouteEntry? from, RouteEntry? to)
    {
        NavigationStarting?.Invoke(this, NavigationEventArgs.Starting(from, to, to?.Parameters));
    }

    private void OnNavigated(RouteEntry? from, RouteEntry? to)
    {
        Navigated?.Invoke(this, NavigationEventArgs.Navigated(from, to));
    }

    private void OnNavigationFailed(RouteEntry? from, RouteEntry? to, NavigationStatus status, string? message)
    {
        NavigationFailed?.Invoke(this, NavigationEventArgs.Failed(from, to, status, message));
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
}
