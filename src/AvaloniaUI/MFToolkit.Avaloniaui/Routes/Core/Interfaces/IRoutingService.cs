using MFToolkit.Avaloniaui.Routes.Core.Entities;

namespace MFToolkit.Avaloniaui.Routes.Core.Interfaces;

/// <summary>
/// 路由服务接口，定义路由管理的核心功能
/// </summary>
public interface IRoutingService
{
    /// <summary>
    /// 注册路由模型
    /// </summary>
    /// <param name="model">路由模型</param>
    void RegisterRoute(RoutingModel model);
    /// <summary>
    /// 根据路由注册的页面类型来获取路由
    /// <para>通常用于未设置路由而随机生成路由的类</para>
    /// </summary>
    /// <param name="type">页面类型</param>
    /// <returns>当前页面类型的路由</returns>
    string? PageTypeToRoute(Type type);
    
    /// <summary>
    /// 导航到指定路由
    /// </summary>
    /// <param name="route">目标路由</param>
    /// <param name="parameters">路由参数</param>
    /// <returns>当前路由信息</returns>
    Task<RouteCurrentInfo?> GoToAsync(string route, Dictionary<string, object?>? parameters = null);
    
    /// <summary>
    /// 替换当前路由
    /// </summary>
    /// <param name="route">目标路由</param>
    /// <param name="parameters">路由参数</param>
    /// <returns>当前路由信息</returns>
    Task<RouteCurrentInfo?> ReplaceAsync(string route, Dictionary<string, object?>? parameters = null);
    
    /// <summary>
    /// 返回上一页
    /// </summary>
    /// <returns>当前路由信息</returns>
    Task<RouteCurrentInfo?> GoBackAsync();
    
    /// <summary>
    /// 返回根页面
    /// </summary>
    /// <returns>当前路由信息</returns>
    Task<RouteCurrentInfo?> GoToRootAsync();
    
    /// <summary>
    /// 获取当前路由信息
    /// </summary>
    /// <returns>当前路由信息</returns>
    RouteCurrentInfo? GetCurrentRouteInfo();
    
    /// <summary>
    /// 清除路由历史
    /// </summary>
    void ClearHistory();
}
    