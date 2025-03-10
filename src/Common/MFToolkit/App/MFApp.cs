﻿using MFToolkit.DependencyInjection;
using MFToolkit.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MFToolkit.App;
/// <summary>
/// APP 拓展通用工具类
/// </summary>
public partial class MFApp
{
    /// <summary>
    /// 服务集合
    /// </summary>
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
    /// 应用程序配置 <see cref="IConfiguration"/>
    /// </summary>
    public static IConfiguration? Configuration;


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

    /// <summary>
    /// 获取注入的服务,带有异常抛出的
    /// </summary>
    /// <param name="serviceType">查找的服务类型</param>
    /// <param name="message">异常信息</param>
    /// <param name="code">状态码</param>
    /// <returns></returns>
    /// <exception cref="OhException" />
    public static object GetService(Type serviceType, string message, int code = 500)
    {
        return GetService(serviceType) ?? throw OhException.ApplicationError(message, code);
    }
    /// <summary>
    /// 获取注入的服务,带有异常抛出的
    /// </summary>
    /// <typeparam name="T">要解析的类型</typeparam>
    /// <param name="message">异常信息</param>
    /// <param name="code">状态码</param>
    /// <returns></returns>
    /// <exception cref="OhException" />
    public static T GetService<T>(string message, int code = 500) where T : notnull
    {
        return GetService<T>() ?? throw OhException.ApplicationError(message, code);
    }

    /// <summary>
    /// 注入Service，建议在没用类似于下面代码的时候调用，例如
    /// <para>
    /// 不存在这样调用的时候
    /// <code>
    /// var builder = MauiApp.CreateBuilder();
    /// builder.Services.InjectServices();
    /// </code>
    /// 应该直接这样调用
    /// <code>GlobalInjects.InjectServices();</code>
    /// </para>
    /// </summary>
    /// <param name="httpRequestConfiguration">HttpClient 请求基本地址</param>
    /// <param name="serviceOptions">额外自己要注入的配置</param>
    /// <returns></returns>
    public static IServiceCollection InjectServices(HttpRequestConfiguration? httpRequestConfiguration = null,
        Action<IServiceCollection> serviceOptions = null!) =>
        GlobalInjects.InjectServices(httpRequestConfiguration, serviceOptions);

}
