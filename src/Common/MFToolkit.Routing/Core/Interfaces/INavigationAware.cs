namespace MFToolkit.Routing.Core.Interfaces;

/// <summary>
/// 路由生命周期感知接口，实现此接口的页面/视图模型可以接收路由生命周期事件
/// </summary>
public interface INavigationAware
{
    /// <summary>
    /// 导航完成，页面激活时调用
    /// </summary>
    /// <param name="parameters">导航参数</param>
    void OnNavigated(Dictionary<string, object?>? parameters);

    /// <summary>
    /// 即将离开当前页面时调用
    /// </summary>
    void OnNavigatingFrom();

    /// <summary>
    /// 已经离开当前页面后调用
    /// </summary>
    void OnNavigatedFrom();
}

/// <summary>
/// 路由生命周期感知接口的默认空实现，供基类继承
/// </summary>
public abstract class NavigationAware : INavigationAware
{
    /// <inheritdoc />
    public virtual void OnNavigated(Dictionary<string, object?>? parameters)
    {
    }

    /// <inheritdoc />
    public virtual void OnNavigatingFrom()
    {
    }

    /// <inheritdoc />
    public virtual void OnNavigatedFrom()
    {
    }
}
