using Microsoft.Extensions.DependencyInjection;
using MFToolkit.Routing.Core.Interfaces;
using MFToolkit.Routing.Entities;

namespace MFToolkit.Routing.DependencyInjection;

/// <summary>
/// 路由服务 DI 扩展
/// </summary>
public static class RoutingExtensions
{

    /// <summary>
    /// 添加路由服务到 DI 容器（带配置）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">配置回调</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddRouting(this IServiceCollection services, Action<RouterOptions>? configure = null)
    {
        var options = new RouterOptions();
        configure?.Invoke(options);

        // 注册路由守卫（如果有）
        foreach (var guardType in options.GuardTypes)
        {
            services.AddSingleton(typeof(IRouteGuard), guardType);
        }

        // 注册路由守卫集合
        services.AddSingleton<IEnumerable<IRouteGuard>>(sp =>
        {
            return [.. sp.GetServices<IRouteGuard>()];
        });

        // 注册 Router（传入路由列表和服务提供者）
        services.AddSingleton<IRouter>(sp =>
        {
            var guards = sp.GetService<IEnumerable<IRouteGuard>>();
            var routeRegistration = sp.GetService<IStartupRouteRegistration>();
            var routes = routeRegistration?.Routes;
            return new Router(guards, routes, sp);
        });

        return services;
    }

    /// <summary>
    /// 添加路由服务到 DI 容器（使用指定的守卫类型）
    /// </summary>
    /// <typeparam name="TGuard">路由守卫类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddRouting<TGuard>(this IServiceCollection services) where TGuard : class, IRouteGuard
    {
        return services.AddRouting(options =>
        {
            options.GuardTypes.Add(typeof(TGuard));
        });
    }

    /// <summary>
    /// 注册路由集合
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="routes">路由实体集合</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddRoutes(this IServiceCollection services, IEnumerable<RouteEntity> routes)
    {
        var routeList = routes.ToList();

        // 根据路由配置注册 PageType 和 ViewModelType
        foreach (var route in routeList)
        {
            // 确定 PageType 的生命周期
            var lifetime = route.IsTop
                ? ServiceLifetime.Singleton
                : ServiceLifetime.Transient;

            // 注册 PageType
            services.Add(new ServiceDescriptor(route.RouteType, route.RouteType, lifetime));

            // 注册 ViewModelType（如果有）
            if (route.ViewModelType != null)
            {
                // ViewModel注册
                services.Add(new ServiceDescriptor(route.ViewModelType, route.ViewModelType, lifetime));
            }
        }

        services.AddSingleton<IStartupRouteRegistration>(new StartupRouteRegistration(routeList));
        return services;
    }

    /// <summary>
    /// 注册路由集合（使用构建器）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configure">路由配置回调</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddRoutes(this IServiceCollection services, Action<RouteRegistry> configure)
    {
        var registry = new RouteRegistry();
        configure(registry);
        var routes = registry.GetAll().ToList();
        return services.AddRoutes(routes);
    }

    /// <summary>
    /// 获取已注册的路由集合（用于初始化 Router）
    /// </summary>
    /// <param name="sp">服务提供者</param>
    /// <returns>路由实体集合</returns>
    public static IEnumerable<RouteEntity> GetRegisteredRoutes(this IServiceProvider sp)
    {
        var registration = sp.GetService<IStartupRouteRegistration>();
        return registration?.Routes ?? Enumerable.Empty<RouteEntity>();
    }
}

/// <summary>
/// 启动时路由注册标记接口（供 Router 内部使用）
/// </summary>
public interface IStartupRouteRegistration
{
    /// <summary>
    /// 已注册的所有路由集合
    /// </summary>
    IEnumerable<RouteEntity> Routes { get; }
}

/// <summary>
/// 启动时路由注册实现
/// </summary>
internal class StartupRouteRegistration : IStartupRouteRegistration
{
    public IEnumerable<RouteEntity> Routes { get; }

    public StartupRouteRegistration(IEnumerable<RouteEntity> routes)
    {
        Routes = routes;
    }
}

/// <summary>
/// 路由选项
/// </summary>
public class RouterOptions
{
    /// <summary>
    /// 路由守卫类型集合（按注册顺序执行，任一返回 false 即阻止导航）
    /// </summary>
    public List<Type> GuardTypes { get; set; } = [];
}
