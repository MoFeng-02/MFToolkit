using System.Reflection;
using MFToolkit.DependencyInjection.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.DependencyInjection.Extensions;
public static class DependencyInjectionServiceCollectionExtensions
{

    /// <summary>
    /// 注册所有实现了IPrivateDependency的服务
    /// <para>
    /// 如何使用，示例：
    /// <code>builder.Services.AddDependencyInjection(Assembly.Load("Zero.Api.Services"));</code>
    /// </para>
    /// </summary>
    /// <param name="services">服务容器</param>
    /// <param name="assembly">程序集</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i => typeof(IPrivateDependency).IsAssignableFrom(i)));
        foreach (var type in types)
        {
            // 获取继承的注册类型
            var @interface = type.GetInterfaces().Where(i => typeof(IPrivateDependency).IsAssignableFrom(i)).FirstOrDefault(i => i != typeof(IPrivateDependency));
            // 获取实现类型
            var interfaceType = type.GetInterfaces()
                .FirstOrDefault(i => i != typeof(IPrivateDependency));
            if (interfaceType != null)
            {
                if (@interface == typeof(IScoped))
                {
                    services.AddScoped(interfaceType, type);
                }
                else if (@interface == typeof(ISingleton))
                {
                    services.AddSingleton(interfaceType, type);
                }
                else if (@interface == typeof(ITransient))
                {
                    services.AddTransient(interfaceType, type);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid interface type: {interfaceType.Name}");
                }
            }
        }
        return services;
    }
}

