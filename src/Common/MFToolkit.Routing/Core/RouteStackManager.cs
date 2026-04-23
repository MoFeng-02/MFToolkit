using MFToolkit.Routing.Entities;

namespace MFToolkit.Routing;

/// <summary>
/// 路由栈管理器，负责管理所有顶级路由的栈
/// </summary>
public class RouteStackManager
{
    /// <summary>
    /// 按顶级路由ID存储的栈字典
    /// </summary>
    private readonly Dictionary<Guid, RouteStack> _stacks = new();

    /// <summary>
    /// 默认顶级路由ID
    /// </summary>
    private readonly Guid _defaultTopRouteId;

    /// <summary>
    /// 当前活跃的顶级路由ID
    /// </summary>
    private Guid _currentTopRouteId;

    /// <summary>
    /// 已注册的顶级路由实体
    /// </summary>
    private readonly List<RouteEntity> _registeredTopRoutes = new();

    /// <summary>
    /// 当前顶级路由ID
    /// </summary>
    public Guid CurrentTopRouteId => _currentTopRouteId;

    /// <summary>
    /// 当前栈
    /// </summary>
    public RouteStack CurrentStack => GetOrCreateStack(_currentTopRouteId);

    /// <summary>
    /// 用户注册的顶级路由（不包含默认路由）
    /// </summary>
    public IReadOnlyList<RouteEntity> RegisteredTopRoutes => _registeredTopRoutes.AsReadOnly();

    /// <summary>
    /// 是否使用默认顶级路由
    /// </summary>
    public bool IsUsingDefaultTopRoute => _currentTopRouteId == _defaultTopRouteId;

    /// <inheritdoc/>
    public RouteStackManager()
    {
        // 创建默认顶级路由ID（只是一个 Guid，用户无感知）
        _defaultTopRouteId = Guid.NewGuid();

        // 自动激活默认顶级路由
        _currentTopRouteId = _defaultTopRouteId;
        _stacks[_currentTopRouteId] = new RouteStack(_currentTopRouteId);
    }

    /// <summary>
    /// 获取或创建指定顶级路由的栈
    /// </summary>
    public RouteStack GetOrCreateStack(Guid topRouteId)
    {
        if (!_stacks.TryGetValue(topRouteId, out var stack))
        {
            stack = new RouteStack(topRouteId);
            _stacks[topRouteId] = stack;
        }
        return stack;
    }

    /// <summary>
    /// 切换顶级路由
    /// </summary>
    public void SwitchTopRoute(Guid topRouteId)
    {
        _currentTopRouteId = topRouteId;
        GetOrCreateStack(topRouteId);
    }

    /// <summary>
    /// 注册顶级路由实体
    /// </summary>
    public void RegisterTopRoute(RouteEntity entity)
    {
        if (!entity.IsTop)
            entity.IsTop = true;

        _registeredTopRoutes.Add(entity);

        // 如果是第一个注册的顶级路由，自动切换到它
        if (_registeredTopRoutes.Count == 1)
        {
            SwitchTopRoute(entity.Id);
        }
    }

    /// <summary>
    /// 获取指定顶级路由ID的注册信息
    /// </summary>
    public RouteEntity? GetTopRoute(Guid topRouteId)
    {
        // 先检查注册的顶级路由
        var registered = _registeredTopRoutes.FirstOrDefault(r => r.Id == topRouteId);
        if (registered != null)
            return registered;

        // 检查是否是默认顶级路由
        if (topRouteId == _defaultTopRouteId)
            return null; // 默认路由没有实体

        return null;
    }

    /// <summary>
    /// 获取所有顶级路由ID（包括默认路由）
    /// </summary>
    public IEnumerable<Guid> GetAllTopRouteIds()
    {
        yield return _defaultTopRouteId;
        foreach (var route in _registeredTopRoutes)
        {
            yield return route.Id;
        }
    }
}
