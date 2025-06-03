using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.App.Extensions.DependencyInjection;
/// <inheritdoc/>
public static class MFAppServicesExtension
{
    /// <summary>
    /// 注入给MFApp获取配置
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddInjectMFAppService(this IServiceCollection services)
    {
        #region 配置获取
        MFApp.ServiceCollection = services;
        #endregion
        return services;
    }
    /// <summary>
    /// 重新加载MFApp的ServiceProvider
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ReloadMFAppServiceProvider(this IServiceCollection services)
    {
        MFApp.ServiceProvider = services.BuildServiceProvider();
        return services;
    }
}
