using Garnet;
using MFToolkit.Garnet.Interfaces;
using MFToolkit.Garnet.Models;
using MFToolkit.Garnet.Services;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MFToolkit.Garnet.Extensions;
public static class GarnetServiceExtensions
{

    /// <summary>
    /// 注册Garnet Server
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static void AddGarnetServer(this IServiceCollection services, string[] args)
    {
        _ = Task.Run(() =>
        {
            try
            {
                Console.WriteLine("启动内置Garnet服务");
                using var server = new GarnetServer(args);
                server.Start();
                Console.WriteLine("启动成功，将休眠此线程");
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to initialize server due to exception: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// 注册全局唯一GarnetService实例
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns>返回当前实例</returns>
    public static GarnetService AddGarnetService(this IServiceCollection services, string config)
    {
        GarnetService garnetService = new(config);
        services.AddSingleton<IGarnetService>(garnetService);
        return garnetService;
    }

    /// <summary>
    /// 注册全局唯一GarnetService实例
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns>返回当前实例</returns>
    public static GarnetService AddGarnetService(this IServiceCollection services, ConnectConfiguration config)
    {
        GarnetService garnetService = new(config);
        services.AddSingleton<IGarnetService>(garnetService);
        return garnetService;
    }

    /// <summary>
    /// 注册全局唯一GarnetService实例
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns>返回当前实例</returns>
    public static GarnetService AddGarnetService(this IServiceCollection services, ConfigurationOptions config)
    {
        GarnetService garnetService = new(config);
        services.AddSingleton<IGarnetService>(garnetService);
        return garnetService;
    }
}
