using MFToolkit.Download.DownloadServices;
using Microsoft.Extensions.DependencyInjection;

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
        return services;
    }
}
