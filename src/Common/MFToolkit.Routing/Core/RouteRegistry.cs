using MFToolkit.Routing.Entities;

namespace MFToolkit.Routing;

/// <summary>
/// 路由注册表，负责管理所有路由的注册和查找（仅供内部使用）
/// </summary>
public class RouteRegistry
{
    /// <summary>
    /// RouteKey → RouteEntity 映射
    /// </summary>
    private readonly Dictionary<string, RouteEntity> _routeMap = [];

    /// <summary>
    /// Type → RouteEntity 映射
    /// </summary>
    private readonly Dictionary<Type, RouteEntity> _typeMap = [];

    /// <summary>
    /// 已注册的路由数量
    /// </summary>
    public int Count => _routeMap.Count;

    /// <summary>
    /// 注册单个路由
    /// </summary>
    internal void Register(RouteEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var routeKey = entity.RouteKey;

        // 如果已存在同名路由，覆盖之
        if (_routeMap.TryGetValue(routeKey, out var existing))
        {
            // 从类型映射中移除旧的
            _typeMap.Remove(existing.RouteType);
        }

        _routeMap[routeKey] = entity;
        _typeMap[entity.RouteType] = entity;
    }

    /// <summary>
    /// 批量注册路由
    /// </summary>
    internal void RegisterRange(IEnumerable<RouteEntity> entities)
    {
        foreach (var entity in entities)
        {
            Register(entity);
        }
    }

    /// <summary>
    /// 根据 RouteKey 查找路由
    /// </summary>
    public RouteEntity? FindByKey(string routeKey)
    {
        return _routeMap.TryGetValue(routeKey, out var entity) ? entity : null;
    }

    /// <summary>
    /// 根据 Type 查找路由
    /// </summary>
    public RouteEntity? FindByType(Type type)
    {
        return _typeMap.TryGetValue(type, out var entity) ? entity : null;
    }

    /// <summary>
    /// 根据 Type 查找路由（泛型版本）
    /// </summary>
    public RouteEntity? FindByType<T>() where T : class
    {
        return FindByType(typeof(T));
    }

    /// <summary>
    /// 检查指定 RouteKey 是否已注册
    /// </summary>
    public bool ContainsKey(string routeKey)
    {
        return _routeMap.ContainsKey(routeKey);
    }

    /// <summary>
    /// 检查指定 Type 是否已注册
    /// </summary>
    public bool ContainsType(Type type)
    {
        return _typeMap.ContainsKey(type);
    }

    /// <summary>
    /// 获取所有已注册的路由
    /// </summary>
    public IEnumerable<RouteEntity> GetAll()
    {
        return _routeMap.Values;
    }

    /// <summary>
    /// 获取所有顶级路由
    /// </summary>
    public IEnumerable<RouteEntity> GetTopRoutes()
    {
        return _routeMap.Values.Where(r => r.IsTop);
    }

    ///// <summary>
    ///// 清空所有注册
    ///// </summary>
    ////public void Clear()
    ////{
    ////    _routeMap.Clear();
    ////    _typeMap.Clear();
    ////}

    ///// <summary>
    ///// 移除指定路由
    ///// </summary>
    ////public bool Remove(string routeKey)
    ////{
    ////    if (_routeMap.TryGetValue(routeKey, out var entity))
    ////    {
    ////        _routeMap.Remove(routeKey);
    ////        _typeMap.Remove(entity.RouteType);
    ////        return true;
    ////    }
    ////    return false;
    ////}
}
