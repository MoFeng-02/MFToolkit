using MFToolkit.Avaloniaui.Routes.Core.Entities;
using MFToolkit.Avaloniaui.Routes.Core.Enumerates;
using MFToolkit.Avaloniaui.Routes.Core.Interfaces;
using MFToolkit.Avaloniaui.Routes.Infrastructure.Services;
using MFToolkit.Avaloniaui.Routes.Infrastructure.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MFToolkit.Avaloniaui.Routes.Infrastructure.DependencyInjection;

/// <summary>
/// 路由服务依赖注入扩展类，提供服务注册和路由配置功能
/// </summary>
public static class RoutingServiceExtensions
{
    /// <summary>
    /// 向服务集合添加路由相关服务并配置路由
    /// </summary>
    /// <param name="services">服务集合实例，不可为null</param>
    /// <param name="configureRoutes">路由配置委托，可为null</param>
    /// <returns>配置后的服务集合</returns>
    /// <exception cref="ArgumentNullException">当services为null时抛出</exception>
    public static IServiceCollection AddRoutingServices(
        this IServiceCollection services,
        Action<IRouteBuilder>? configureRoutes = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // 注册核心服务
        services.AddSingleton<IRouteParser, RouteParser>();
        services.AddSingleton<KeepAliveCache>();
        services.AddSingleton<IRoutingService, RoutingService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // 配置路由
        var routeBuilder = new RouteBuilder();
        configureRoutes?.Invoke(routeBuilder);

        // 将配置的路由注册到服务中
        var routes = routeBuilder.Build().Where(q => q.Lifetime != null);

        // 注册路由中定义的页面和视图模型
        foreach (var routeModel in routes)
        {
            switch (routeModel.Lifetime)
            {
                case Lifetime.Transient:
                    services.TryAddTransient(routeModel.PageType);
                    break;
                case Lifetime.Scoped:
                    services.TryAddScoped(routeModel.PageType);
                    break;
                case Lifetime.Singleton:
                    services.TryAddSingleton(routeModel.PageType);
                    break;
                default:
                    services.TryAddTransient(routeModel.PageType);
                    break;
            }

            if (routeModel.ViewModelType != null)
            {
                switch (routeModel.Lifetime)
                {
                    case Lifetime.Transient:
                        services.TryAddTransient(routeModel.ViewModelType);
                        break;
                    case Lifetime.Scoped:
                        services.TryAddScoped(routeModel.ViewModelType);
                        break;
                    case Lifetime.Singleton:
                        services.TryAddSingleton(routeModel.ViewModelType);
                        break;
                    default:
                        services.TryAddTransient(routeModel.ViewModelType);
                        break;
                }
            }
        }

        return services;
    }
}

/// <summary>
/// 路由构建器接口，用于配置路由信息
/// </summary>
public interface IRouteBuilder
{
    /// <summary>
    /// 添加带视图模型的路由配置
    /// </summary>
    /// <typeparam name="TPage">页面类型，必须是引用类型</typeparam>
    /// <typeparam name="TViewModel">视图模型类型，必须是引用类型</typeparam>
    /// <param name="route">路由路径，可为null（将使用页面类型名称作为默认路径）</param>
    /// <param name="isTopNavigation">是否为顶级导航页面</param>
    /// <param name="isKeepAlive">是否保持页面活动状态（不销毁）</param>
    /// <param name="priority">路由匹配优先级（值越高越先匹配）</param>
    /// <param name="meta">路由元数据，可为null</param>
    /// <returns>路由构建器实例，用于链式调用</returns>
    IRouteBuilder AddRoute<TPage, TViewModel>(string? route = null,
        bool isTopNavigation = false,
        bool isKeepAlive = false,
        int priority = 0,
        RoutingMeta? meta = null)
        where TPage : class
        where TViewModel : class;

    /// <summary>
    /// 添加不带视图模型的路由配置
    /// </summary>
    /// <typeparam name="TPage">页面类型，必须是引用类型</typeparam>
    /// <param name="route">路由路径，可为null（将使用页面类型名称作为默认路径）</param>
    /// <param name="isTopNavigation">是否为顶级导航页面</param>
    /// <param name="isKeepAlive">是否保持页面活动状态（不销毁）</param>
    /// <param name="priority">路由匹配优先级（值越高越先匹配）</param>
    /// <param name="meta">路由元数据，可为null</param>
    /// <returns>路由构建器实例，用于链式调用</returns>
    IRouteBuilder AddRoute<TPage>(string? route = null,
        bool isTopNavigation = false,
        bool isKeepAlive = false,
        int priority = 0,
        RoutingMeta? meta = null)
        where TPage : class;

    /// <summary>
    /// 手动处理的路由配置
    /// </summary>
    /// <param name="pageType">页面类型</param>
    /// <param name="viewModelType">视图类型</param>
    /// <param name="route">路由路径，可为null（将使用页面类型名称作为默认路径）</param>
    /// <param name="isTopNavigation">是否为顶级导航页面</param>
    /// <param name="isKeepAlive">是否保持页面活动状态（不销毁）</param>
    /// <param name="priority">路由匹配优先级（值越高越先匹配）</param>
    /// <param name="meta">路由元数据，可为null</param>
    /// <returns>路由构建器实例，用于链式调用</returns>
    IRouteBuilder AddRoute(Type pageType,
        Type? viewModelType = null,
        string? route = null,
        bool isTopNavigation = false,
        bool isKeepAlive = false,
        int priority = 0,
        RoutingMeta? meta = null);

    /// <summary>
    /// 构建并返回所有配置的路由
    /// </summary>
    /// <returns>路由模型集合</returns>
    IEnumerable<RoutingModel> Build();
}

/// <summary>
/// 路由构建器实现类，用于收集和管理路由配置
/// </summary>
public class RouteBuilder : IRouteBuilder
{
    private readonly List<RoutingModel> _routes = new List<RoutingModel>();

    /// <summary>
    /// 添加带视图模型的路由配置
    /// </summary>
    public IRouteBuilder AddRoute<TPage, TViewModel>(string? route = null,
        bool isTopNavigation = false,
        bool isKeepAlive = false,
        int priority = 0,
        RoutingMeta? meta = null)
        where TPage : class
        where TViewModel : class
    {
        var routePath = route ?? typeof(TPage).Name.ToLowerInvariant();
        var model = new RoutingModel<TPage, TViewModel>(
            routePath, isKeepAlive, isTopNavigation, priority, meta);

        _routes.Add(model);
        return this;
    }

    /// <summary>
    /// 添加不带视图模型的路由配置
    /// </summary>
    public IRouteBuilder AddRoute<TPage>(string? route = null,
        bool isTopNavigation = false,
        bool isKeepAlive = false,
        int priority = 0,
        RoutingMeta? meta = null)
        where TPage : class
    {
        var routePath = route ?? typeof(TPage).Name.ToLowerInvariant();
        var model = new RoutingModel<TPage>(
            routePath, isKeepAlive, isTopNavigation, priority, meta);

        _routes.Add(model);
        return this;
    }

    /// <inheritdoc />
    public IRouteBuilder AddRoute(Type pageType, Type? viewModelType = null, string? route = null,
        bool isTopNavigation = false,
        bool isKeepAlive = false, int priority = 0, RoutingMeta? meta = null)
    {
        var routePath = route ?? pageType.Name.ToLowerInvariant();
        var model = new RoutingModel(
            pageType, viewModelType, routePath, isKeepAlive, isTopNavigation, priority, meta);

        _routes.Add(model);
        return this;
    }

    /// <summary>
    /// 构建并返回所有配置的路由
    /// </summary>
    public IEnumerable<RoutingModel> Build()
    {
        // 按优先级排序路由
        return _routes.OrderByDescending(r => r.Priority).ToList();
    }
}