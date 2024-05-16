using MFToolkit.App;
using MFToolkit.Exceptions;
using Microsoft.Extensions.Logging;

namespace MFToolkit.Loggers.LoggerExtensions.Utils;

public class LoggerUtil
{
    public static ILogger Logger { get; private set; } = MFApp.GetService<ILogger<LoggerUtil>>() ?? throw MFAppException.UnRealizedException;
    public static void LogInformation(string? message) => Logger.LogInformation(message);

    public static void LogInformation<T>(string? message) => MFApp.GetService<ILogger<T>>()?.LogInformation
        (message);

    public static void LogError(string? message) => Logger.LogError(message);
    public static void LogError<T>(string? message) => MFApp.GetService<ILogger<T>>()?.LogError(message);

}