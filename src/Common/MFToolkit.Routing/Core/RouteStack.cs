using MFToolkit.Routing.Entities;

namespace MFToolkit.Routing;

/// <summary>
/// 路由栈，按顶级路由隔离的导航历史
/// </summary>
public class RouteStack
{
    /// <summary>
    /// 所属顶级路由ID
    /// </summary>
    public Guid TopRouteId { get; }

    /// <summary>
    /// 当前栈顶条目
    /// </summary>
    public RouteEntry? Current => _entries.Count > 0 ? _entries[_entries.Count - 1] : null;

    /// <summary>
    /// 栈历史（只读副本）
    /// </summary>
    public IReadOnlyList<RouteEntry> History => _entries.AsReadOnly();

    /// <summary>
    /// 栈深度
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// 是否可以返回（栈中有多于一个条目）
    /// </summary>
    public bool CanGoBack => _entries.Count > 1;

    /// <summary>
    /// 当前条目的父条目（栈中上一个条目）
    /// </summary>
    public RouteEntry? Parent => _entries.Count >= 2 ? _entries[_entries.Count - 2] : null;

    /// <summary>
    /// 获取祖先链（从根到父的顺序）
    /// </summary>
    public IReadOnlyList<RouteEntry> GetAncestors()
    {
        // 返回除最后一个外的所有条目（从根到父）
        return _entries.Count >= 2 ? _entries.Take(_entries.Count - 1).ToList() : new List<RouteEntry>();
    }

    private readonly List<RouteEntry> _entries = new();

    /// <inheritdoc/>
    public RouteStack(Guid topRouteId)
    {
        TopRouteId = topRouteId;
    }

    /// <summary>
    /// 入栈
    /// </summary>
    public void Push(RouteEntry entry)
    {
        // 激活新条目
        entry.IsActivated = true;

        // 激活当前条目
        if (Current is { IsActivated: true })
        {
            Current.IsActivated = false;
        }

        _entries.Add(entry);
    }

    /// <summary>
    /// 出栈，返回被移除的条目
    /// </summary>
    public RouteEntry? Pop()
    {
        if (_entries.Count == 0)
            return null;

        var entry = _entries[^1];
        _entries.RemoveAt(_entries.Count - 1);

        // 激活新的栈顶
        if (Current is { IsActivated: false })
        {
            Current.IsActivated = true;
        }

        return entry;
    }

    /// <summary>
    /// 清空栈，保留顶级路由
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
    }

    /// <summary>
    /// 获取指定索引处的条目
    /// </summary>
    public RouteEntry? GetAt(int index)
    {
        if (index >= 0 && index < _entries.Count)
            return _entries[index];
        return null;
    }

    /// <summary>
    /// 获取指定 RouteEntity 的条目
    /// </summary>
    public RouteEntry? FindByEntity(RouteEntity entity)
    {
        return _entries.FirstOrDefault(e => e.Entity.Id == entity.Id);
    }

    /// <summary>
    /// 获取指定 RouteEntity 的条目索引
    /// </summary>
    public int IndexOf(RouteEntity entity)
    {
        return _entries.FindIndex(e => e.Entity.Id == entity.Id);
    }
}
