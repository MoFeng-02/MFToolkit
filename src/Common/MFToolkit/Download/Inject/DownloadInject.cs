using MFToolkit.Download.Handler;
using MFToolkit.Download.Services;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace MFToolkit.Download.Inject;
public static class DownloadInject
{
    /// <summary>
    /// 注册下载服务
    /// <para>建议紧接着在后面跟随AddDownloadPauseInfoHandler等</para>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDownloadService(this IServiceCollection services)
    {
        services.AddTransient<IDownloadService, DownloadService>();
        return services;
    }
    /// <summary>
    /// 注册下载服务
    /// <para>建议紧接着在后面跟随AddDownloadPauseInfoHandler等</para>
    /// </summary>
    /// <typeparam name="T">自定义服务类</typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDownloadService<T>(this IServiceCollection services) where T : class, IDownloadService
    {
        services.AddTransient<IDownloadService, T>();
        return services;
    }
    /// <summary>
    /// 提供下载暂停处理程序内部类
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="isStartEncryption"></param>
    /// <param name="encryptionKey"></param>
    /// <returns></returns>
    public static IServiceCollection AddDownloadPauseInfoHandler(this IServiceCollection services, Func<ConnectionConfig>? config = null, bool isStartEncryption = false, string? encryptionKey = null)
    {
        DownloadPauseInfoHandler.DownloadInitPauseInfoHandler(config, isStartEncryption, encryptionKey);
        services.AddSingleton<DownloadPauseInfoHandler>();
        return services;
    }
    /// <summary>
    /// 提供下载暂停处理程序自定义类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDownloadPauseInfoHandler<T>(this IServiceCollection services) where T : DownloadPauseInfoHandler
    {
        services.AddSingleton<DownloadPauseInfoHandler, T>();
        return services;
    }
}
