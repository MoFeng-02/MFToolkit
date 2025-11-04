namespace MFToolkit.Minecraft.Exceptions.Auths;
/// <summary>
/// 授权待处理异常
/// </summary>
public class AuthorizationPendingException : Exception
{
    /// <summary>
    /// 初始化授权待处理异常
    /// </summary>
    /// <param name="message">异常消息</param>
    public AuthorizationPendingException(string message) : base(message) { }
}

/// <summary>
/// 授权被拒绝异常
/// </summary>
public class AuthorizationDeclinedException : Exception
{
    /// <summary>
    /// 初始化授权被拒绝异常
    /// </summary>
    /// <param name="message">异常消息</param>
    public AuthorizationDeclinedException(string message) : base(message) { }
}

/// <summary>
/// 验证代码无效异常
/// </summary>
public class BadVerificationCodeException : Exception
{
    /// <summary>
    /// 初始化验证代码无效异常
    /// </summary>
    /// <param name="message">异常消息</param>
    public BadVerificationCodeException(string message) : base(message) { }
}

/// <summary>
/// 令牌过期异常
/// </summary>
public class ExpiredTokenException : Exception
{
    /// <summary>
    /// 初始化令牌过期异常
    /// </summary>
    /// <param name="message">异常消息</param>
    public ExpiredTokenException(string message) : base(message) { }
}