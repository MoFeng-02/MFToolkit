using Garnet;
using MFToolkit.Garnet.Interfaces;
using MFToolkit.Garnet.Models;
using MFToolkit.Garnet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public static IServiceCollection AddGarnetService(this IServiceCollection services, string config)
    {
        // 1. 注册ConnectionMultiplexer（容器管理生命周期）
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            try
            {
                return ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                // 添加日志记录（需先注册ILogger）
                var logger = sp.GetService<ILogger<GarnetService>>();
                logger?.LogCritical(ex, "Redis连接失败");
                throw;
            }
        });

        // 2. 按需获取数据库和订阅者
        services.AddSingleton(sp =>
            sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        services.AddSingleton(sp =>
            sp.GetRequiredService<IConnectionMultiplexer>().GetSubscriber());

        services.AddSingleton<IGarnetService, GarnetService>();
        return services;
    }

    /// <summary>
    /// 注册全局唯一GarnetService实例
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns>返回当前实例</returns>
    public static IServiceCollection AddGarnetService(this IServiceCollection services, ConnectConfiguration config)
    {
        // 1. 注册ConnectionMultiplexer（容器管理生命周期）
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            try
            {
                return ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                // 添加日志记录（需先注册ILogger）
                var logger = sp.GetService<ILogger<GarnetService>>();
                logger?.LogCritical(ex, "Redis连接失败");
                throw;
            }
        });

        // 2. 按需获取数据库和订阅者
        services.AddSingleton(sp =>
            sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        services.AddSingleton(sp =>
            sp.GetRequiredService<IConnectionMultiplexer>().GetSubscriber());

        services.AddSingleton<IGarnetService, GarnetService>();
        return services;
    }

    /// <summary>
    /// 注册全局唯一GarnetService实例
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns>返回当前实例</returns>
    public static IServiceCollection AddGarnetService(this IServiceCollection services, ConfigurationOptions config)
    {
        // 1. 注册ConnectionMultiplexer（容器管理生命周期）
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            try
            {
                return ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                // 添加日志记录（需先注册ILogger）
                var logger = sp.GetService<ILogger<GarnetService>>();
                logger?.LogCritical(ex, "Redis连接失败");
                throw;
            }
        });

        // 2. 按需获取数据库和订阅者
        services.AddSingleton(sp =>
            sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        services.AddSingleton(sp =>
            sp.GetRequiredService<IConnectionMultiplexer>().GetSubscriber());

        services.AddSingleton<IGarnetService, GarnetService>();
        return services;
    }
}
