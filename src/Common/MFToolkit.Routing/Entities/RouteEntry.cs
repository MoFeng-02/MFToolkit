namespace MFToolkit.Routing.Entities;

/// <summary>
/// 路由条目，表示栈中的一个路由实例
/// </summary>
public class RouteEntry
{
    /// <summary>
    /// 路由实体
    /// </summary>
    public RouteEntity Entity { get; }

    /// <summary>
    /// 当前导航参数
    /// </summary>
    public Dictionary<string, object?>? Parameters { get; }

    /// <summary>
    /// 导航时间
    /// </summary>
    public DateTime NavigatedAt { get; }

    /// <summary>
    /// 页面实例（由框架侧填充）
    /// </summary>
    public object? PageInstance { get; set; }

    /// <summary>
    /// 视图模型实例（由 Router 创建）
    /// </summary>
    public object? ViewModelInstance { get; set; }

    /// <summary>
    /// 是否已激活
    /// </summary>
    public bool IsActivated { get; set; }

    /// <inheritdoc/>
    public RouteEntry(RouteEntity entity, Dictionary<string, object?>? parameters = null)
    {
        Entity = entity;
        Parameters = parameters;
        NavigatedAt = DateTime.UtcNow;
        IsActivated = false;
    }
}
