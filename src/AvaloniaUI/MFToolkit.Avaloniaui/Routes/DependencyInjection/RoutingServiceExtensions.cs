using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Avaloniaui.Routes.DependencyInjection;

/// <summary>
/// 路由DI扩展方法
/// </summary>
public static class RoutingServiceExtensions
{
    /// <summary>
    /// 注册路由系统到 DI 容器
    /// <para>说明：若IsTopNavigation为true，则IsKeepAlive强制为True</para>
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureRoutes">路由配置委托（若使用委托AddRoute添加路由模型，则此优先级最高）</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRouting(
        this IServiceCollection services,
        Action<IRouteBuilder>? configureRoutes = null)
    {
        // 1. 注册路由核心服务
        services.AddSingleton<KeepAliveCache>();

        // 2. 配置路由集合
        var routes = Routing.GetRoutingModels();
        if (routes.Count == 0)
        {
            var routeBuilder = new RouteBuilder();
            configureRoutes?.Invoke(routeBuilder);
            services.AddSingleton(routeBuilder.Build());
            routes.Clear();
            var routingModels = routeBuilder.Build();
            Routing.RegisterRoutes([.. routingModels]);
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
        services.AddSingleton<Routing>();
        return services;
    }

    /// <summary>
    /// 注入Routing ServiceProvider
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IServiceProvider AddRoutingServiceProvider(this IServiceProvider serviceProvider)
    {
        Routing.ServiceProvider = serviceProvider;
        return serviceProvider;
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
    /// <summary>
    /// 新增路由
    /// </summary>
    /// <param name="route">路径</param>
    /// <param name="isTopNavigation">是否顶级路由</param>
    /// <param name="isKeepAlive">是否保活页（顶级路由强制保活）</param>
    /// <param name="priority">排序</param>
    /// <param name="meta">其他项</param>
    /// <typeparam name="T">页面</typeparam>
    /// <returns></returns>
    IRouteBuilder AddRoute<T>(string? route = null, bool isTopNavigation = false, bool isKeepAlive = false,
        int priority = 0, RoutingMeta? meta = null);

    /// <summary>
    /// 新增路由
    /// </summary>
    /// <param name="pageType">页面类型</param>
    /// <param name="route">路径</param>
    /// <param name="isTopNavigation">是否顶级路由</param>
    /// <param name="isKeepAlive">是否保活页（顶级路由强制保活）</param>
    /// <param name="priority">排序</param>
    /// <param name="meta">其他项</param>
    /// <returns></returns>
    IRouteBuilder AddRoute(Type pageType, string? route = null, bool isTopNavigation = false, bool isKeepAlive = false,
        int priority = 0, RoutingMeta? meta = null);
}

internal class RouteBuilder : IRouteBuilder
{
    private readonly List<RoutingModel> _routes = [];

    public IRouteBuilder AddRoute<T>(string? route = null, bool isTopNavigation = false, bool isKeepAlive = false,
        int priority = 0, RoutingMeta? meta = null)
    {
        return AddRoute(typeof(T), route, isTopNavigation, isKeepAlive, priority, meta);
    }

    public IRouteBuilder AddRoute(Type pageType, string? route = null, bool isTopNavigation = false,
        bool isKeepAlive = false, int priority = 0, RoutingMeta? meta = null)
    {
        _routes.Add(new RoutingModel(pageType, route, isKeepAlive, isTopNavigation, priority, meta));
        return this;
    }

    public IEnumerable<RoutingModel> Build() => _routes;
}