//using Microsoft.Extensions.Logging;
//using NLog;
//using NLog.Extensions.Logging;

//namespace MFToolkit.Loggers.NLogs;

//public partial class NLogHelperUtils
//{
//    public readonly static Logger logger = LogManager.GetCurrentClassLogger();
//    public Logger InitLogger()
//    {
//        var log = LogManager.GetCurrentClassLogger();
//        return log;
//    }
//}
//public static partial class NLogHelper
//{
//    /// <summary>
//    /// 处理NLog配置
//    /// </summary>
//    /// <param name="logging"></param>
//    /// <returns></returns>
//    public static ILoggingBuilder AddNLogConfig(this ILoggingBuilder logging)
//    {
//        return logging.AddNLog("Loggers/NLogs/NLog.config");
//    }
//}