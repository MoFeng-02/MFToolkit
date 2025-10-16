using System.Collections.Immutable;
using MFToolkit.Avaloniaui.Routes.Core.Enumerates;
using MFToolkit.Avaloniaui.Routes.Infrastructure.Utils;

namespace MFToolkit.Avaloniaui.Routes.Core.Entities;

/// <summary>
/// 路由元数据类，用于存储路由的附加信息
/// </summary>
public class RoutingMeta
{
    /// <summary>
    /// 路由标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 路由图标
    /// </summary>
    public object? Icon { get; set; }

    /// <summary>
    /// 自定义数据字典
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// 路由段类，用于解析和存储路由的各个部分
/// </summary>
public class RouteSegment
{
    /// <summary>
    /// 段名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 是否为参数段（如 {id}）
    /// </summary>
    public bool IsParameter { get; }

    /// <summary>
    /// 是否为通配符段（如 *path）
    /// </summary>
    public bool IsWildcard { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">段名称</param>
    /// <param name="isParameter">是否为参数段</param>
    /// <param name="isWildcard">是否为通配符段</param>
    public RouteSegment(string name, bool isParameter, bool isWildcard)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        IsParameter = isParameter;
        IsWildcard = isWildcard;
    }
}

/// <summary>
/// 当前路由信息类，包含当前路由的详细信息
/// </summary>
public class RouteCurrentInfo
{
    /// <summary>
    /// 关联的路由模型
    /// </summary>
    public RoutingModel? RoutingModel { get; set; }

    /// <summary>
    /// 页面实例
    /// </summary>
    public object? Page { get; set; }

    /// <summary>
    /// 提供解析视图类型
    /// </summary>
    public Type? PageType => RoutingModel?.PageType;

    /// <summary>
    /// 视图模型类型
    /// </summary>
    public Type? ViewModelType => RoutingModel?.ViewModelType;

    /// <summary>
    /// 视图模型实例
    /// </summary>
    public object? ViewModel { get; set; }

    /// <summary>
    /// 路由参数
    /// </summary>
    public Dictionary<string, object?>? Parameters { get; set; }

    /// <summary>
    /// 路由路径
    /// </summary>
    public string? RoutePath { get; set; }
}

/// <summary>
/// 路由模型类，用于存储单个路由的配置信息
/// 包含页面类型、视图模型类型、路由路径等核心信息
/// </summary>
public class RoutingModel
{
    /// <summary>
    /// 默认路由ID
    /// </summary>
    public Guid RoutingId { get; } = Guid.NewGuid();

    /// <summary>
    /// 页面类型（如HomePage、DetailPage等）
    /// 不可为null，必须是有效的页面类型
    /// </summary>
    public Type PageType { get; }

    /// <summary>
    /// 视图模型类型（如HomeViewModel）
    /// 可为null，表示该页面不需要视图模型
    /// </summary>
    public Type? ViewModelType { get; }

    /// <summary>
    /// 路由路径（如"/home"、"/detail/{id}"）
    /// 用于导航时匹配页面
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// 是否为保活页面
    /// 保活页面在导航离开时不会被销毁，而是存入缓存，再次导航时直接复用
    /// </summary>
    public bool IsKeepAlive { get; }

    /// <summary>
    /// 是否为顶级导航页面
    /// 顶级页面导航时会清空之前的导航栈，成为新的根页面
    /// </summary>
    public bool IsTopNavigation { get; }

    /// <summary>
    /// 路由优先级（数值越高，匹配时优先级越高）
    /// 用于解决路由路径冲突问题（如"/detail"和"/detail/{id}"）
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// 路由元数据
    /// </summary>
    public RoutingMeta? Meta { get; set; }

    /// <summary>
    /// 指定生命周期，如果不提供则不自己进行DI注册
    /// </summary>
    public Lifetime? Lifetime { get; init; }

    /// <summary>
    /// 路由模型构造函数
    /// </summary>
    /// <param name="pageType">页面类型，不可为null</param>
    /// <param name="viewModelType">视图模型类型，可为null</param>
    /// <param name="route">路由路径</param>
    /// <param name="isKeepAlive">是否为保活页面</param>
    /// <param name="isTopNavigation">是否为顶级页面</param>
    /// <param name="priority">优先级</param>
    /// <param name="meta">路由元数据</param>
    /// <param name="lifetime">指定DI生命周期</param>
    public RoutingModel(
        Type pageType,
        Type? viewModelType = null,
        string? route = null,
        bool isKeepAlive = false,
        bool isTopNavigation = false,
        int priority = 0,
        RoutingMeta? meta = null,
        Lifetime? lifetime = null)
    {
        PageType = pageType ?? throw new ArgumentNullException(nameof(pageType));
        ViewModelType = viewModelType;
        Route = route ?? Guid.NewGuid().ToString();
        IsKeepAlive = isKeepAlive;
        IsTopNavigation = isTopNavigation;
        Priority = priority;
        Meta = meta;
        Lifetime = lifetime;
        // 预解析路由段
        if (!RouteParser.Default.TryParse(Route, out var segments))
        {
            segments = ImmutableArray<RouteSegment>.Empty;
        }
    }
}

/// <summary>
/// 泛型路由模型类，提供类型安全的路由定义
/// </summary>
/// <typeparam name="TPage">页面类型</typeparam>
/// <typeparam name="TViewModel">视图模型类型，可为null</typeparam>
public sealed class RoutingModel<TPage, TViewModel> : RoutingModel
    where TPage : class
{
    /// <summary>
    /// 泛型路由模型构造函数
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <param name="isKeepAlive">是否为保活页面</param>
    /// <param name="isTopNavigation">是否为顶级页面</param>
    /// <param name="priority">优先级</param>
    /// <param name="meta">路由元数据</param>
    /// <param name="lifetime">指定DI生命周期</param>
    public RoutingModel(
        string? route = null,
        bool isKeepAlive = false,
        bool isTopNavigation = false,
        int priority = 0,
        RoutingMeta? meta = null,
        Lifetime? lifetime = null)
        : base(typeof(TPage), typeof(TViewModel), route, isKeepAlive, isTopNavigation, priority, meta, lifetime)
    {
        // 继承父类构造函数
    }
}

/// <summary>
/// 无ViewModel的泛型路由模型类
/// </summary>
/// <typeparam name="TPage">页面类型</typeparam>
public sealed class RoutingModel<TPage> : RoutingModel
    where TPage : class
{
    /// <summary>
    /// 无ViewModel的泛型路由模型构造函数
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <param name="isKeepAlive">是否为保活页面</param>
    /// <param name="isTopNavigation">是否为顶级页面</param>
    /// <param name="priority">优先级</param>
    /// <param name="meta">路由元数据</param>
    /// <param name="lifetime">指定DI生命周期</param>
    public RoutingModel(
        string? route = null,
        bool isKeepAlive = false,
        bool isTopNavigation = false,
        int priority = 0,
        RoutingMeta? meta = null,
        Lifetime? lifetime = null)
        : base(typeof(TPage), null, route, isKeepAlive, isTopNavigation, priority, meta, lifetime)
    {
        // 继承父类构造函数，明确指定ViewModelType为null
    }
}