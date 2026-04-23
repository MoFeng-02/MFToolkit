namespace MFToolkit.Routing.Entities;

/// <summary>
/// 路由实体，表示一个路由的配置信息
/// </summary>
public class RouteEntity
{
    /// <summary>
    /// 路由唯一ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 路由路径，如 "/home/settings"
    /// </summary>
    public string? RoutePath { get; set; }

    /// <summary>
    /// 路由名称（默认取 RouteType.Name）
    /// </summary>
    public string? RouteName { get; set; }

    /// <summary>
    /// 路由实体类（页面类型）
    /// </summary>
    public required Type RouteType { get; set; }

    /// <summary>
    /// 视图模型类型（可选，用于 MVVM 模式）
    /// </summary>
    public Type? ViewModelType { get; set; }

    /// <summary>
    /// 默认路由参数
    /// </summary>
    public Dictionary<string, object?>? DefaultParameters { get; set; }

    /// <summary>
    /// 保持活跃，不被缓存清理
    /// </summary>
    public bool IsKeepalive { get; set; } = false;

    /// <summary>
    /// 是否为顶级路由
    /// </summary>
    public bool IsTop { get; set; } = false;

    /// <summary>
    /// 懒加载（按需实例化）
    /// </summary>
    public bool IsLazy { get; set; } = true;

    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 路由唯一键（用于字典索引）
    /// </summary>
    public string RouteKey => RoutePath ?? RouteType.Name;

    /// <summary>
    /// 获取路由显示名称
    /// </summary>
    public string DisplayName => RouteName ?? RouteType.Name;
}
