using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Avaloniaui.Routings.DependencyInjection;

/// <summary>
/// 路由DI扩展方法
/// </summary>
public static class RoutingServiceExtensions
{
    /// <summary>
    /// 注册路由系统到 DI 容器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureRoutes">路由配置委托（若使用委托AddRoute添加路由模型，则此优先级最高）</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRouting(
        this IServiceCollection services,
        Action<IRouteBuilder>? configureRoutes = null)
    {
        // 1. 注册路由核心服务
        services.AddSingleton<IRouteParser, DefaultRouteParser>();
        services.AddSingleton<KeepAliveCache>();

        // 2. 配置路由集合
        var routes = Routing.GetRoutingModels();
        if (routes.Count == 0)
        {
            var routeBuilder = new RouteBuilder();
            configureRoutes?.Invoke(routeBuilder);
            services.AddSingleton(routeBuilder.Build());
            routes.Clear();
            var routings = routeBuilder.Build();
            Routing.RegisterRoutes([.. routings]);
            foreach (var route in routeBuilder.Build())
            {
                services.AddTransient(route.PageType);
            }
        }
        else
            foreach (var route in routes)
            {
                services.AddTransient(route.PageType);
            }
        // 3.设置 ServiceProvider
        services.AddSingleton(s =>
        {
            Routing.ServiceProvider = s;
            return s;
        });
        services.AddSingleton<Routing>();
        return services;
    }

    /// <summary>
    /// 注册导航服务到DI容器
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddNavigationService(this IServiceCollection services)
    {
        services.AddSingleton<KeepAliveCache>();
        services.AddSingleton<Navigation>();
        return services;
    }
}

/// <summary>
/// 路由构建器接口
/// </summary>
public interface IRouteBuilder
{
    IRouteBuilder AddRoute<T>(string? route = null, bool isTopNavigation = false, bool isKeepAlive = false, int priority = 0, RoutingMeta? meta = null);
    IRouteBuilder AddRoute(Type pageType, string? route = null, bool isTopNavigation = false, bool isKeepAlive = false, int priority = 0, RoutingMeta? meta = null);
}

internal class RouteBuilder : IRouteBuilder
{
    private readonly List<RoutingModel> _routes = [];

    public IRouteBuilder AddRoute<T>(string? route = null, bool isTopNavigation = false, bool isKeepAlive = false, int priority = 0, RoutingMeta? meta = null)
    {
        return AddRoute(typeof(T), route, isTopNavigation, isKeepAlive, priority, meta);
    }

    public IRouteBuilder AddRoute(Type pageType, string? route = null, bool isTopNavigation = false, bool isKeepAlive = false, int priority = 0, RoutingMeta? meta = null)
    {
        _routes.Add(new RoutingModel
        {
            PageType = pageType,
            Route = route ?? pageType.Name.ToLower(),
            IsTopNavigation = isTopNavigation,
            IsKeepAlive = isKeepAlive,
            Priority = priority,
            Meta = meta
        });
        return this;
    }

    public IEnumerable<RoutingModel> Build() => _routes;
}