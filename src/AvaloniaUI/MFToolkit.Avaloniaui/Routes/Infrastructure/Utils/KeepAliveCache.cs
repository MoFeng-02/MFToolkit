using System.Collections.Concurrent;

namespace MFToolkit.Avaloniaui.Routes.Infrastructure.Utils;

/// <summary>
/// 保活缓存工具，用于管理需要长期保留的页面和视图模型实例
/// </summary>
public class KeepAliveCache
{
    private readonly ConcurrentDictionary<string, object> _pages = new ConcurrentDictionary<string, object>();
    private readonly ConcurrentDictionary<string, object> _viewModels = new ConcurrentDictionary<string, object>();
    private readonly Queue<string> _cacheOrder = new Queue<string>();
    private readonly int _maxCacheSize;

    /// <summary>
    /// 当前缓存的页面数量
    /// </summary>
    public int PageCount => _pages.Count;

    /// <summary>
    /// 当前缓存的视图模型数量
    /// </summary>
    public int ViewModelCount => _viewModels.Count;

    /// <summary>
    /// 初始化保活缓存
    /// </summary>
    /// <param name="maxCacheSize">最大缓存数量（默认20，超过则移除最早缓存的实例）</param>
    public KeepAliveCache(int maxCacheSize = 20)
    {
        if (maxCacheSize < 1)
            throw new ArgumentException("最大缓存数量必须大于0", nameof(maxCacheSize));

        _maxCacheSize = maxCacheSize;
    }

    /// <summary>
    /// 新增获取修改
    /// </summary>
    /// <param name="route"></param>
    /// <param name="page"></param>
    /// <param name="viewModel"></param>
    public void AddOrUpdate(string route, object page, object? viewModel)
    {
        CachePage(route, page);
        CacheViewModel(route, viewModel);
    }

    /// <summary>
    /// 缓存页面实例
    /// </summary>
    /// <param name="route">路由路径（作为缓存键）</param>
    /// <param name="page">页面实例（不可为null）</param>
    /// <exception cref="ArgumentNullException">当route或page为null时抛出</exception>
    public void CachePage(string route, object page)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentNullException(nameof(route), "路由路径不能为空");
        if (page == null)
            throw new ArgumentNullException(nameof(page), "页面实例不能为空");

        // 检查缓存大小，超过则移除最早的
        EnsureCacheSize();

        _pages[route] = page;
        _cacheOrder.Enqueue(route);
    }

    /// <summary>
    /// 缓存视图模型实例
    /// </summary>
    /// <param name="route">路由路径（作为缓存键）</param>
    /// <param name="viewModel">视图模型实例（不可为null）</param>
    /// <exception cref="ArgumentNullException">当route或viewModel为null时抛出</exception>
    public void CacheViewModel(string route, object? viewModel)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentNullException(nameof(route), "路由路径不能为空");

        _viewModels[route] = viewModel ?? throw new ArgumentNullException(nameof(viewModel), "视图模型实例不能为空");
    }

    /// <summary>
    /// 尝试获取页面和模型实体
    /// </summary>
    /// <param name="route"></param>
    /// <param name="page"></param>
    /// <param name="viewModel"></param>
    /// <returns></returns>
    public bool TryGet(string route, out object? page, out object? viewModel)
    {
        TryGetPage(route, out page);
        TryGetViewModel(route, out viewModel);
        return page != null || viewModel != null;
    }

    /// <summary>
    /// 尝试获取缓存的页面实例
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <param name="page">输出参数：页面实例</param>
    /// <returns>是否获取成功</returns>
    public bool TryGetPage(string route, out object? page)
    {
        return _pages.TryGetValue(route, out page);
    }

    /// <summary>
    /// 尝试获取缓存的视图模型实例
    /// </summary>
    /// <param name="route">路由路径</param>
    /// <param name="viewModel">输出参数：视图模型实例</param>
    /// <returns>是否获取成功</returns>
    public bool TryGetViewModel(string route, out object? viewModel)
    {
        return _viewModels.TryGetValue(route, out viewModel);
    }

    /// <summary>
    /// 移除指定路由的缓存
    /// </summary>
    /// <param name="route">路由路径</param>
    public void RemoveCache(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return;

        _pages.TryRemove(route, out _);
        _viewModels.TryRemove(route, out _);
    }

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public void Clear()
    {
        _pages.Clear();
        _viewModels.Clear();
        _cacheOrder.Clear();
    }

    /// <summary>
    /// 确保缓存不超过最大限制
    /// </summary>
    private void EnsureCacheSize()
    {
        while (_cacheOrder.Count >= _maxCacheSize)
        {
            var oldestRoute = _cacheOrder.Dequeue();
            RemoveCache(oldestRoute);
        }
    }
}