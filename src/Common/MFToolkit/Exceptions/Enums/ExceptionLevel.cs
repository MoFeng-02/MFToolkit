namespace MFToolkit.Exceptions.Enums;

/// <summary>
/// 定义了异常的严重级别
/// </summary>
public enum ExceptionLevel
{
    /// <summary>
    /// 信息性 - 不是真正的错误，只是用于记录一些信息
    /// </summary>
    Info = 0,

    /// <summary>
    /// 警告 - 表示可能存在的问题，但应用程序仍可继续运行
    /// </summary>
    Warning = 1,

    /// <summary>
    /// 用户错误 - 由用户操作不当引起的错误，例如输入错误等
    /// </summary>
    UserError = 2,

    /// <summary>
    /// 应用程序错误 - 指应用程序内部出现的问题，通常需要开发者介入解决
    /// </summary>
    ApplicationError = 3,

    /// <summary>
    /// 系统错误 - 严重的错误，可能涉及到操作系统或外部服务，可能导致应用部分功能不可用
    /// </summary>
    SystemError = 4,

    /// <summary>
    /// 致命错误 - 最严重的错误类型，可能导致整个应用程序崩溃或无法继续运行
    /// </summary>
    FatalError = 5
}