using MFToolkit.Utils.AppExtensions;
using Microsoft.Extensions.Logging;

namespace MFToolkit.Loggers.LoggerExtensions.Utils;
public class LoggerUtil
{
    public static ILogger Logger { get; private set; } = AppUtil.GetService<ILogger<LoggerUtil>>();
}
