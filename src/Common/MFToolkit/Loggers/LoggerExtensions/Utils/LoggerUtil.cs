using MFToolkit.App;
using Microsoft.Extensions.Logging;

namespace MFToolkit.Loggers.LoggerExtensions.Utils;

public class LoggerUtil
{
    public static ILogger Logger { get; private set; } = AppUtil.GetService<ILogger<LoggerUtil>>();
    public static void LogInformation(string? message) => Logger.LogInformation(message);

    public static void LogInformation<T>(string? message) => AppUtil.GetService<ILogger<T>>().LogInformation
        (message);

    public static void LogError(string? message) => Logger.LogError(message);
    public static void LogError<T>(string? message) => AppUtil.GetService<ILogger<T>>().LogError(message);

}