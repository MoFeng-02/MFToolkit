using MFToolkit.Loggers.MFLogger.Configurations;
using MFToolkit.Loggers.MFLogger.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace MFToolkit.Loggers.MFLogger.Extensions.DependencyInjection;
public static class LoggingBuilderExtensions
{
    private static readonly Action<LoggerConfiguration> defaultConfig = (options) =>
    {
        options.AddStartLogLevel(LogLevel.Information);
        options.AddStartLogLevel(LogLevel.Error);
        options.AddStartLogLevel(LogLevel.Warning);

        options.BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        options.OpenGroupLevel = true;
        options.SaveTimeType = SaveTimeType.Hour;
        Task.Run(() => LogClear.ClearLogs(options));
    };
    /// <summary>
    /// 拓展官方Logging，实现本地存储
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddMFLocalFileLogger(this ILoggingBuilder builder, Action<LoggerConfiguration>? config = null)
    {
        builder.AddConfiguration();
        builder.Services.Configure(config ?? defaultConfig);
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LocalFileLoggerProvider>());
        //builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, LogClearTask>());
        LoggerProviderOptions.RegisterProviderOptions<LoggerConfiguration, LocalFileLoggerProvider>(builder.Services);
        return builder;
    }
}
