using System.Collections.Concurrent;

namespace MFToolkit.Avaloniaui.Routes;

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
    /// 页面停用或切换时触发
    /// </summary>
    /// <returns></returns>
    Task OnDeactivatedAsync();
}

/// <summary>
/// 保活页面缓存管理器
/// <para>功能说明：</para>
/// <para>1. 管理所有标记为 IsKeepAlive 的页面实例缓存</para>
/// <para>2. 自动清理超过指定时间的过期缓存（默认30分钟）</para>
/// <para>3. 线程安全的缓存操作（基于 ConcurrentDictionary）</para>
/// </summary>
public class KeepAliveCache : IDisposable
{
    // 线程安全的缓存存储结构，Key为（路由路径，参数哈希）元组
    private readonly ConcurrentDictionary<(string Route, string ParamsHash), RouteCurrentInfo> _cachedPages = new();

    // 定时清理过期缓存的计时器
    private readonly Timer _cleanupTimer;

    // 缓存最大保留时间（默认30分钟）
    private readonly TimeSpan _maxCacheDuration = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 构造函数（初始化自动清理计时器）
    /// </summary>
    public KeepAliveCache()
    {
        _cleanupTimer = new Timer(_ => RemoveExpiredEntries(), null, _maxCacheDuration, _maxCacheDuration);
    }

    /// <summary>
    /// 生成参数的哈希标识（用于区分不同参数组合）
    /// </summary>
    /// <param name="parameters">路由参数字典（允许为null或空）</param>
    /// <returns>参数字符串的哈希值（空字符串表示无参数）</returns>
    private static string GetParamsHash(Dictionary<string, object?>? parameters)
    {
        if (parameters == null || parameters.Count == 0) return string.Empty;

        // 按Key排序后生成哈希字符串（示例："key1=hash1&key2=hash2"）
        var entries = parameters
            .OrderBy(p => p.Key)
            .Select(p => $"{p.Key}={p.Value?.GetHashCode() ?? 0}");
        return string.Join("&", entries).GetHashCode().ToString();
    }

    /// <summary>
    /// 生成缓存键（路由路径 + 参数哈希）
    /// </summary>
    /// <param name="route">路由路径（如 "/user/profile"）</param>
    /// <param name="parameters">路由参数（允许为null）</param>
    /// <returns>(Route, ParamsHash) 元组</returns>
    private static (string Route, string ParamsHash) GenerateKey(string route, Dictionary<string, object?>? parameters)
        => (route, GetParamsHash(parameters));

    /// <summary>
    /// 尝试从缓存中获取页面实例
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <param name="parameters">路由参数（允许为null）</param>
    /// <param name="info">输出参数：找到的缓存实例（可能为null）</param>
    /// <returns>true - 找到缓存；false - 未找到</returns>
    public bool TryGetPage(
        string route,
        Dictionary<string, object?>? parameters,
        out RouteCurrentInfo? info
    )
    {
        info = null;
        if (string.IsNullOrEmpty(route)) return false;

        var key = GenerateKey(route, parameters);
        if (!_cachedPages.TryGetValue(key, out info)) return false;
        // 更新最后访问时间
        info.LastAccessedTime = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// 缓存页面实例（仅当页面标记为 IsKeepAlive 时生效）
    /// </summary>
    /// <param name="info">要缓存的路由信息</param>
    public void CachePage(RouteCurrentInfo info)
    {
        if (!info.IsKeepAlive || info.Route == null) return; // 过滤非保活页

        var key = GenerateKey(info.Route, info.Parameters);
        // 线程安全添加或更新缓存
        _cachedPages.AddOrUpdate(
            key,
            _ => info,
            (_, existing) => existing // 保留现有实例（避免重复激活）
        );
    }

    /// <summary>
    /// 清理过期缓存条目（定时触发）
    /// </summary>
    private void RemoveExpiredEntries()
    {
        var now = DateTime.UtcNow;
        foreach (var (key, info) in _cachedPages)
        {
            // 跳过仍在导航堆栈中的页面
            if (info.IsInNavigationStack) continue;

            // 检查最后访问时间是否超时
            if (now - info.LastAccessedTime > _maxCacheDuration)
            {
                _cachedPages.TryRemove(key, out _);
            }
        }
    }

    /// <summary>
    /// 释放资源（停止定时器）
    /// </summary>
    public void Dispose() => _cleanupTimer.Dispose();
}