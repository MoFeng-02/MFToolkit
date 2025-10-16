using MFToolkit.Avaloniaui.Routes.Core.Entities;

namespace MFToolkit.Avaloniaui.Routes.Core.Interfaces;

/// <summary>
/// 导航服务接口，定义导航操作的上层功能
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// 导航事件（值是页面）
    /// </summary>
    Action<object?, RouteCurrentInfo?>? NavigationCompleted { get; set; }

    /// <summary>
    /// 推送新页面到导航栈
    /// </summary>
    /// <param name="route">目标路由</param>
    /// <param name="parameters">路由参数</param>
    /// <returns>页面实例</returns>
    Task<object?> GoToAsync(string route, Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// 推送新页面到导航栈
    /// </summary>
    /// <param name="parameters">路由参数</param>
    /// <typeparam name="TPage">页面类型</typeparam>
    /// <returns></returns>
    Task<object?> GoToAsync<TPage>(Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// 推送新页面到导航栈
    /// </summary>
    /// <param name="pageType">页面类型</param>
    /// <param name="parameters">路由参数</param>
    /// <returns></returns>
    Task<object?> GoToAsync(Type pageType, Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// 替换当前页面
    /// </summary>
    /// <param name="route">目标路由</param>
    /// <param name="parameters">路由参数</param>
    /// <returns>页面实例</returns>
    Task<object?> ReplaceAsync(string route, Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// 返回上一页
    /// </summary>
    /// <returns>页面实例</returns>
    Task<object?> GoBackAsync();

    /// <summary>
    /// 返回根页面
    /// </summary>
    /// <returns>页面实例</returns>
    Task<object?> GoToRootAsync();

    /// <summary>
    /// 获取当前页面
    /// </summary>
    /// <returns>页面实例</returns>
    object? GetCurrentPage();

    /// <summary>
    /// 获取当前视图模型
    /// </summary>
    /// <returns>视图模型实例</returns>
    object? GetCurrentViewModel();
}