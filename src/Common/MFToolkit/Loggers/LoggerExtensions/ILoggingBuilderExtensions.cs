using MFToolkit.Loggers.LoggerExtensions.Configurations;
using MFToolkit.Loggers.LoggerExtensions.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace MFToolkit.Loggers.LoggerExtensions;
public static class ILoggingBuilderExtensions
{
    /// <summary>
    /// 拓展官方Logging，实现本地存储
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddLocalFileLogger(this ILoggingBuilder builder, Action<LoggerConfiguration> config)
    {
        builder.AddConfiguration();
        builder.Services.Configure(config);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LocalFileLoggerProvider>());
        //builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, LogClearTask>());
        LoggerProviderOptions.RegisterProviderOptions<LoggerConfiguration, LocalFileLoggerProvider>(builder.Services);
        return builder;
    }
}
