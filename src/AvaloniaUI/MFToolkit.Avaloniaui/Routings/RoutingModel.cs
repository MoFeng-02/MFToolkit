using System.Collections.Immutable;

namespace MFToolkit.Avaloniaui.Routings;

/// <summary>
/// 路由配置模型（线程安全设计）
/// </summary>
public class RoutingModel
{

    /// <summary>
    /// 页面
    /// </summary>
    public required Type PageType { get; init; }

    /// <summary>
    /// 页面路由
    /// </summary>
    public required string Route { get; init; } = Guid.NewGuid().ToString();

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