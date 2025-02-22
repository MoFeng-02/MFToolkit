using System.Collections.Concurrent;

namespace MFToolkit.Avaloniaui.Routings;

/// <summary>
/// 页面生命周期接口
/// </summary>
public interface IPageLifecycle
{
    /// <summary>
    /// 页面激活时触发
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task OnActivatedAsync(Dictionary<string, object?> parameters);
    /// <summary>
    /// 页面重新激活时触发
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task OnReactivatedAsync(Dictionary<string, object?> parameters);
    /// <summary>
    /// 页面停用时触发
    /// </summary>
    /// <returns></returns>
    Task OnDeactivatedAsync();
}


// 保活缓存管理类
public class KeepAliveCache
{
    private readonly ConcurrentDictionary<string, RouteCurrentInfo> _cachedPages = new();

    /// <summary>获取缓存键（路由参数序列化）</summary>
    private string GetCacheKey(Type pageType, Dictionary<string, object?> routeParams)
    {
        var paramString = string.Join("&",
            routeParams.OrderBy(p => p.Key)
                .Select(p => $"{p.Key}={p.Value}"));
        return $"{pageType.FullName}?{paramString}";
    }

    /// <summary>尝试获取保活页面</summary>
    public bool TryGetPage(
        Type pageType,
        Dictionary<string, object?> routeParams,
        out RouteCurrentInfo? page)
    {
        var key = GetCacheKey(pageType, routeParams);
        return _cachedPages.TryGetValue(key, out page);
    }

    /// <summary>缓存页面实例</summary>
    public void CachePage(
        Type pageType,
        Dictionary<string, object?> routeParams,
        RouteCurrentInfo page)
    {
        var key = GetCacheKey(pageType, routeParams);
        _cachedPages.AddOrUpdate(key, page, (_, _) => page);
    }

    public string GenerateCacheKey(string route, Dictionary<string, object?> parameters)
    {
        var paramString = string.Join("&",
            parameters.OrderBy(p => p.Key)
                .Select(p => $"{p.Key}={p.Value}"));
        return $"{route}?{paramString}";
    }
    public bool TryGetPage(string route, Dictionary<string, object?> parameters, out RouteCurrentInfo? info)
    {
        return _cachedPages.TryGetValue(GenerateCacheKey(route, parameters), out info);
    }

    public void CachePage(RouteCurrentInfo info)
    {
        if (!info.IsKeepAlive) return;
        var key = GenerateCacheKey(info.Route!, info.Parameters);
        _cachedPages.TryAdd(key, info);
    }
    /// <summary>清除指定类型的缓存</summary>
    public void Clear(Type pageType)
    {
        var prefix = $"{pageType.FullName}?";
        var keysToRemove = _cachedPages.Keys
            .Where(k => k.StartsWith(prefix))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cachedPages.TryRemove(key, out _);
        }
    }
}