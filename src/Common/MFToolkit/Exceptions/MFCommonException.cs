using MFToolkit.CommonTypes.Enumerates;

namespace MFToolkit.Exceptions;
/// <summary>
/// 通用异常处理
/// </summary>
public class MFCommonException : Exception
{
    /// <summary>
    /// 异常的级别
    /// </summary>
    public ExceptionLevel ExceptionLevel { get; }
    /// <summary>
    /// 自定义状态码
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// 初始化一个新的 MFCommonException 实例。
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="exceptionLevel">异常级别</param>
    /// <param name="code">自定义响应码</param>
    public MFCommonException(string message, ExceptionLevel exceptionLevel, int code = 500)
        : base(message)
    {
        ExceptionLevel = exceptionLevel;
        Code = code;
    }

    /// <summary>
    /// 初始化一个新的 MFCommonException 实例。
    /// </summary>
    /// <param name="message">异常消息</param>
    /// <param name="innerException">内部异常</param>
    /// <param name="exceptionLevel">异常级别</param>
    /// <param name="code">自定义响应码</param>
    public MFCommonException(string message, Exception innerException, ExceptionLevel exceptionLevel, int code = 500)
        : base(message, innerException)
    {
        ExceptionLevel = exceptionLevel;
        Code = code;
    }

}
