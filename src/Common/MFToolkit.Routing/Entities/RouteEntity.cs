using System.Diagnostics.CodeAnalysis;

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
    /// 跳过自动 DI 注册（true：不自动注册到 DI 容器，由用户自行注册）
    /// </summary>
    /// <remarks>
    /// 默认 false，自动按 IsTop 推断生命周期注册到 DI。
    /// 设置为 true 时需确保该类型已在 DI 容器中注册，否则导航时会抛出异常。
    /// </remarks>
    public bool SkipAutoDI { get; set; } = false;

    /// <summary>
    /// 获取路由显示名称
    /// </summary>
    public string DisplayName => RouteName ?? RouteType.Name;

    /// <summary>
    /// 无参构造函数（用于反序列化或手动构建场景）
    /// </summary>
    public RouteEntity()
    {
    }

    /// <summary>
    /// 通过类型构造路由实体
    /// </summary>
    /// <param name="routeType">页面类型</param>
    /// <param name="routePath">路由路径（可选）</param>
    [method: SetsRequiredMembers]
    public RouteEntity(Type routeType, string? routePath = null)
    {
        RouteType = routeType;
        RoutePath = routePath;
    }

}

/// <summary>
/// 泛型路由实体，方便 fluent API 链式调用
/// </summary>
public class RouteEntity<TRoute> : RouteEntity
{
    /// <summary>
    /// 无参构造
    /// </summary>
    [method: SetsRequiredMembers]
    public RouteEntity()
    {

        RouteType = typeof(TRoute);
    }

    /// <summary>
    /// 通过路由路径构造
    /// </summary>
    /// <param name="routePath">路由路径</param>
    [method: SetsRequiredMembers]
    public RouteEntity(string? routePath)
    {
        RouteType = typeof(TRoute);
        RoutePath = routePath;
    }

    /// <summary>
    /// 标记为顶级路由
    /// </summary>
    public RouteEntity<TRoute> SetTop(bool isTop = true)
    {
        IsTop = isTop;
        return this;
    }

    /// <summary>
    /// 设置视图模型类型
    /// </summary>
    /// <typeparam name="TViewModel">视图模型类型</typeparam>
    /// <returns>返回新的 RouteEntity（泛型版本会自动转为基类）</returns>
    public RouteEntity WithViewModel<TViewModel>()
    {
        ViewModelType = typeof(TViewModel);
        return this;
    }

    /// <summary>
    /// 设置路由路径
    /// </summary>
    /// <param name="routePath">路由路径</param>
    /// <returns></returns>
    public RouteEntity<TRoute> SetPath(string routePath)
    {
        RoutePath = routePath;
        return this;
    }

    /// <summary>
    /// 设置 KeepAlive
    /// </summary>
    public RouteEntity<TRoute> SetKeepAlive(bool keepAlive = true)
    {
        IsKeepalive = keepAlive;
        return this;
    }

    /// <summary>
    /// 设置排序权重
    /// </summary>
    public RouteEntity<TRoute> SetOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        return this;
    }

    /// <summary>
    /// 跳过自动 DI 注册（由用户自行注册到 DI 容器）
    /// </summary>
    public RouteEntity<TRoute> SetSkipAutoDI()
    {
        base.SkipAutoDI = true;
        return this;
    }
}

/// <summary>
/// 泛型路由实体，同时指定页面类型和视图模型类型
/// </summary>
public class RouteEntity<TRoute, TViewModel> : RouteEntity<TRoute>
{
    /// <summary>
    /// 无参构造
    /// </summary>
    [method: SetsRequiredMembers]
    public RouteEntity()
    {
        RouteType = typeof(TRoute);
        ViewModelType = typeof(TViewModel);
    }

    /// <summary>
    /// 通过路由路径构造
    /// </summary>
    /// <param name="routePath">路由路径</param>
    [method: SetsRequiredMembers]
    public RouteEntity(string? routePath)
    {
        RouteType = typeof(TRoute);
        ViewModelType = typeof(TViewModel);
        RoutePath = routePath;
    }
}
