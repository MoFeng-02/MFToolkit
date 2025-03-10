using MFToolkit.CommonTypes.Enumerates;

namespace MFToolkit.Exceptions;
/// <summary>
/// 通用异常类，提供静态方法来创建和抛出不同级别的异常
/// </summary>
public static class OhException
{
    /// <summary>
    /// 创建一个带有指定消息和级别的异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="exceptionLevel">异常级别</param>
    /// <param name="code">自定义状态码</param>
    /// <returns>MFCommonException 对象</returns>
    public static MFCommonException Create(string message, ExceptionLevel exceptionLevel, int code = 500)
    {
        return new MFCommonException(message, exceptionLevel, code);
    }

    /// <summary>
    /// 创建一个带有指定消息、内部异常和级别的异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常-引发当前异常的异常，或者null引用</param>
    /// <param name="exceptionLevel">异常级别</param>
    /// <param name="code">自定义状态码</param>
    /// <returns>MFCommonException 对象</returns>
    public static MFCommonException Create(string message, Exception innerException, ExceptionLevel exceptionLevel, int code = 500)
    {
        return new MFCommonException(message, innerException, exceptionLevel, code);
    }

    /// <summary>
    /// 抛出一个带有指定消息和级别的异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="exceptionLevel">异常级别</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException Throw(string message, ExceptionLevel exceptionLevel, int code = 500)
    {
        throw Create(message, exceptionLevel, code);
    }

    /// <summary>
    /// 抛出一个带有指定消息、内部异常和级别的异常
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    /// <param name="exceptionLevel">异常级别</param>
    /// <param name="code">自定义状态码</param>
    public static void Throw(string message, Exception innerException, ExceptionLevel exceptionLevel, int code = 500)
    {
        throw Create(message, innerException, exceptionLevel, code);
    }

    /// <summary>
    /// 抛出一个带有指定级别的异常（无消息）
    /// </summary>
    /// <param name="exceptionLevel">异常级别</param>
    /// <param name="code">自定义状态码</param>
    public static void Throw(ExceptionLevel exceptionLevel, int code = 500)
    {
        throw Create(string.Empty, exceptionLevel, code);
    }

    /// <summary>
    /// 抛出一个带有指定内部异常和级别的异常（无消息）
    /// </summary>
    /// <param name="innerException">内部异常</param>
    /// <param name="exceptionLevel">异常级别</param>
    /// <param name="code">自定义状态码</param>
    public static void Throw(Exception innerException, ExceptionLevel exceptionLevel, int code = 500)
    {
        throw Create(string.Empty, innerException, exceptionLevel, code);
    }
    /// <summary>
    /// 信息性 - 不是真正的错误，只是用于记录一些信息
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException Info(string message, int code = 500)
    {
        return Create(message, ExceptionLevel.Info, code);
    }

    /// <summary>
    /// 警告 - 表示可能存在的问题，但应用程序仍可继续运行
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException Warning(string message, int code = 500)
    {
        return Create(message, ExceptionLevel.Warning, code);
    }

    /// <summary>
    /// 用户错误 - 由用户操作不当引起的错误，例如输入错误等
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException UserError(string message, int code = 500)
    {
        return Create(message, ExceptionLevel.UserError, code);
    }

    /// <summary>
    /// 参数错误 - 通常是属于前端传输过来的参数出现错误，例如：不属于这个用户权限的查询参数，其他参数异常等等
    /// </summary>
    /// <param name="message"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static MFCommonException ParamError(string message, int code = 500)
    {
        return Create(message, ExceptionLevel.ParamError, code);
    }

    /// <summary>
    /// 应用程序错误 - 指应用程序内部出现的问题，通常需要开发者介入解决
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException ApplicationError(string message, int code = 500)
    {
        throw Create(message, ExceptionLevel.ApplicationError, code);
    }

    /// <summary>
    /// 系统错误 - 严重的错误，可能涉及到操作系统或外部服务，可能导致应用部分功能不可用
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException SystemError(string message, int code = 500)
    {
        throw Create(message, ExceptionLevel.SystemError, code);
    }

    /// <summary>
    /// 致命错误 - 最严重的错误类型，可能导致整个应用程序崩溃或无法继续运行
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException FatalError(string message, int code = 500)
    {
        throw Create(message, ExceptionLevel.FatalError, code);
    }

    /// <summary>
    /// 信息性 - 不是真正的错误，只是用于记录一些信息
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">引起当前异常的异常，或者null引用</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException Info(string message, Exception innerException, int code = 500)
    {
        return Create(message, innerException, ExceptionLevel.Info, code);
    }

    /// <summary>
    /// 警告 - 表示可能存在的问题，但应用程序仍可继续运行
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">引起当前异常的异常，或者null引用</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException Warning(string message, Exception innerException, int code = 500)
    {
        return Create(message, innerException, ExceptionLevel.Warning, code);
    }

    /// <summary>
    /// 用户错误 - 由用户操作不当引起的错误，例如输入错误等
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">引起当前异常的异常，或者null引用</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException UserError(string message, Exception innerException, int code = 500)
    {
        return Create(message, innerException, ExceptionLevel.UserError, code);
    }

    /// <summary>
    /// 参数错误 - 通常是属于前端传输过来的参数出现错误，例如：不属于这个用户权限的查询参数，其他参数异常等等
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException">引起当前异常的异常，或者null引用</param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static MFCommonException ParamError(string message, Exception innerException, int code = 500)
    {
        return Create(message, innerException, ExceptionLevel.ParamError, code);
    }

    /// <summary>
    /// 应用程序错误 - 指应用程序内部出现的问题，通常需要开发者介入解决
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">引起当前异常的异常，或者null引用</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException ApplicationError(string message, Exception innerException, int code = 500)
    {
        throw Create(message, innerException, ExceptionLevel.ApplicationError, code);
    }

    /// <summary>
    /// 系统错误 - 严重的错误，可能涉及到操作系统或外部服务，可能导致应用部分功能不可用
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">引起当前异常的异常，或者null引用</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException SystemError(string message, Exception innerException, int code = 500)
    {
        throw Create(message, innerException, ExceptionLevel.SystemError, code);
    }

    /// <summary>
    /// 致命错误 - 最严重的错误类型，可能导致整个应用程序崩溃或无法继续运行
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">引起当前异常的异常，或者null引用</param>
    /// <param name="code">自定义状态码</param>
    public static MFCommonException FatalError(string message, Exception innerException, int code = 500)
    {
        throw Create(message, innerException, ExceptionLevel.FatalError, code);
    }
}