using MFToolkit.Download.DownloadHandlers;
using MFToolkit.Download.DownloadServices;
using Microsoft.Extensions.DependencyInjection;
using NPOI.SS.Formula.Functions;

namespace MFToolkit.Download.Inject;
public static class DownloadInject
{
    /// <summary>
    /// 注册下载服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDownloadService(this IServiceCollection services)
    {
        services.AddTransient<IDownloadService, DownloadService>();
        services.AddSingleton<DownloadHandler>();
        return services;
    }
    /// <summary>
    /// 注册下载服务
    /// </summary>
    /// <typeparam name="T">注册自定义下载处理程序</typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDownloadService<T>(this IServiceCollection services) where T : DownloadHandler
    {
        services.AddTransient<IDownloadService, DownloadService>();
        services.AddSingleton<DownloadHandler, T>();
        return services;
    }
}
