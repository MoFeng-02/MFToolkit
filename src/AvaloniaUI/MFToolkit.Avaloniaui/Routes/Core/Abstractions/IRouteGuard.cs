using MFToolkit.Avaloniaui.Routes.Core.Entities;

namespace MFToolkit.Avaloniaui.Routes.Core.Abstractions;

/// <summary>
/// 路由守卫接口，用于导航前的权限验证等逻辑
/// </summary>
public interface IRouteGuard
{
    /// <summary>
    /// 检查是否允许导航到目标路由
    /// </summary>
    /// <param name="targetRoute">目标路由模型</param>
    /// <param name="parameters">导航参数</param>
    /// <returns>如果允许导航则返回true，否则返回false</returns>
    Task<bool> CanNavigateAsync(RoutingModel targetRoute, Dictionary<string, object?>? parameters);
    
    /// <summary>
    /// 当导航被阻止时调用
    /// </summary>
    /// <param name="targetRoute">目标路由模型</param>
    /// <param name="parameters">导航参数</param>
    Task OnNavigationBlockedAsync(RoutingModel targetRoute, Dictionary<string, object?>? parameters);
}
    