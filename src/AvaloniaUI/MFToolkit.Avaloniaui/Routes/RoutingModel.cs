using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace MFToolkit.Avaloniaui.Routes;

/// <summary>
/// 路由配置模型
/// </summary>
public class RoutingModel
{

    /// <summary>
    /// 页面
    /// </summary>
    public Type PageType { get; init; }

    /// <summary>
    /// 页面路由
    /// </summary>
    public string Route { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 预解析的路由段集合（使用不可变数组提升性能）
    /// </summary>
    public ImmutableArray<RouteSegment> Segments { get; init; }

    /// <summary>
    /// 是否为保活页面
    /// </summary>
    public bool IsKeepAlive { get; set; }

    /// <summary>
    /// 是否为顶级页面，顶级页面代表不能在子页面里面，顶级页面页默认就是保活页
    /// </summary>
    public bool IsTopNavigation { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// 其他项
    /// </summary>
    public RoutingMeta? Meta { get; set; }

    /// <summary>
    /// 路由模型构造函数
    /// </summary>
    /// <param name="pageType">页面</param>
    /// <param name="route">路由路径</param>
    /// <param name="isKeepAlive">是否保活</param>
    /// <param name="isTopNavigation">是否顶级路由</param>
    /// <param name="priority">优先级</param>
    /// <param name="meta">其他项</param>
    /// <exception cref="ArgumentNullException"></exception>
    public RoutingModel(Type pageType, string? route = null, bool isKeepAlive = false, bool isTopNavigation = false, int priority = 0, RoutingMeta? meta = null)
    {
        PageType = pageType ?? throw new ArgumentNullException(nameof(pageType));
        Route = route ?? Guid.NewGuid().ToString();
        IsKeepAlive = isKeepAlive;
        IsTopNavigation = isTopNavigation;
        Priority = priority;
        Meta = meta;
    }
}
/// <summary>
/// 泛型路由配置模型
/// </summary>
/// <remarks>
/// 路由模型构造函数
/// </remarks>
/// <typeparam name="TPage"></typeparam>
/// <param name="route">路由路径</param>
/// <param name="isKeepAlive">是否保活</param>
/// <param name="isTopNavigation">是否顶级路由</param>
/// <param name="priority">优先级</param>
/// <param name="meta">其他项</param>
/// <exception cref="ArgumentNullException"></exception>
public sealed class RoutingModel<TPage>(string route, bool isKeepAlive = false, bool isTopNavigation = false, int priority = 0, RoutingMeta? meta = null) : RoutingModel(typeof(TPage), route, isKeepAlive, isTopNavigation, priority, meta)
{
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
    public Dictionary<string, object?> Parameters { get; internal set; } = [];

    /// <summary>
    /// 其他项
    /// </summary>
    public RoutingMeta? Meta { get; set; }

    /// <summary>
    /// 标记当前页面是否在导航堆栈中
    /// </summary>
    public bool IsInNavigationStack { get; set; }

    /// <summary>
    /// 最后一次访问时间（UTC）
    /// </summary>
    public DateTime LastAccessedTime { get; set; } = DateTime.UtcNow;
}


public class RoutePathParameter
{
    /// <summary>
    /// 路径集合
    /// </summary>
    public List<string> Routes { get; set; } = [];

    /// <summary>
    /// 原参数
    /// </summary>
    public string? OriginParameter { get; set; }

    /// <summary>
    /// 参数集合
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = [];

    /// <summary>
    /// 路由检查是否通过
    /// </summary>
    public bool IsRouteOk { get; set; }

    /// <summary>
    /// 路由错误体提醒
    /// </summary>
    public string ErrorMessage { get; set; } = null!;
}

/// <summary>
/// 路由其它想
/// </summary>
public class RoutingMeta
{
    /// <summary>
    /// 是否为基础布局
    /// </summary>
    public bool IsBaseLayout { get; set; }
    /// <summary>
    /// 布局类型
    /// </summary>
    public object? LayoutType { get; set; }
}