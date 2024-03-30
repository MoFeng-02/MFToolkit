using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MFToolkit.App;
/// <summary>
/// APP 拓展通用工具类
/// </summary>
public static class AppUtil
{
    public static IServiceCollection? ServiceCollection;
    /// <summary>
    /// 可以用这个获取创建范围作用域实例
    /// </summary>
    public static IServiceProvider? ServiceProvider;
    /// <summary>
    /// 可以获取比如：开发，生产模式等等信息
    /// </summary>
    public static IHostEnvironment? HostEnvironment;

    /// <summary>
    /// 注入给AppUtil获取配置
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAppUtilService(this IServiceCollection services)
    {
        #region 配置获取
        ServiceCollection = services;
        ServiceProvider = services.BuildServiceProvider();
        #endregion
        return services;
    }
    /// <summary>
    /// 重新加载ServiceProvider
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ReloadAppUtilService(this IServiceCollection services)
    {
        ServiceProvider = services.BuildServiceProvider();
        return services;
    }

    /// <summary>
    /// 获取注入的服务
    /// </summary>
    /// <param name="serviceType">查找的服务类型</param>
    /// <returns>如果不存在这个服务，就返回null</returns>
    public static object? GetService(Type serviceType)
    {
        if (ServiceProvider == null)
        {
            return null;
        }
        try
        {
            var typeService = ServiceProvider.GetRequiredService(serviceType);
            return typeService;
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// 获取注入的服务
    /// </summary>
    /// <typeparam name="T">要解析的类型</typeparam>
    /// <returns>如果不存在这个服务，就返回null</returns>
    public static T? GetService<T>() where T : notnull
    {
        if (ServiceProvider == null)
        {
            return default;
        }
        try
        {
            var typeService = ServiceProvider.GetRequiredService<T>();
            return typeService;
        }
        catch
        {
            return default;
        }
    }


}
