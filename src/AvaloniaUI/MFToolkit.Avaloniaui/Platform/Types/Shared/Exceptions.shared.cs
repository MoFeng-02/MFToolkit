namespace MFToolkit.Avaloniaui.Platform.Types.Shared;
public static class ExceptionUtils
{
#if (NETSTANDARD || !PLATFORM) || NET6_0_OR_GREATER
    internal static NotImplementedInReferenceAssemblyException NotSupportedOrImplementedException =>
        new NotImplementedInReferenceAssemblyException();
#else
		internal static FeatureNotSupportedException NotSupportedOrImplementedException =>
			new FeatureNotSupportedException($"This API is not supported on {DeviceInfo.Platform}");
#endif

}

/// <summary>
/// 从引用程序集执行时发生的异常。这通常意味着NuGet没有安装到应用项目中。
/// </summary>
class NotImplementedInReferenceAssemblyException : NotImplementedException
{
    /// <summary>
    /// 初始化类的新实例 <see cref="NotImplementedInReferenceAssemblyException"/>
    /// </summary>
    public NotImplementedInReferenceAssemblyException()
        : base("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.")
    {
    }
}

/// <summary>
/// 调用需要特定权限的API时发生的异常。
/// </summary>
public class PermissionException : UnauthorizedAccessException
{
    /// <summary>
    /// 使用指定消息初始化类的新实例。 <see cref="PermissionException"/> 
    /// </summary>
    /// <param name="message">更详细地描述此异常的消息。</param>
    public PermissionException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// 当尝试在不支持该特性的平台上使用该特性时发生的异常。
/// </summary>
public class FeatureNotSupportedException : NotSupportedException
{
    /// <summary>
    /// 初始化类的新实例 <see cref="FeatureNotSupportedException"/>
    /// </summary>
    public FeatureNotSupportedException()
    {
    }

    /// <summary>
    /// 使用指定消息初始化类的新实例。 <see cref="FeatureNotSupportedException"/> 
    /// </summary>
    /// <param name="message">更详细地描述此异常的消息。</param>
    public FeatureNotSupportedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 使用指定的消息和内部异常初始化类的新实例。 <see cref="FeatureNotSupportedException"/> 
    /// </summary>
    /// <param name="message">更详细地描述此异常的消息。</param>
    /// <param name="innerException">与此异常相关的内部异常。</param>
    public FeatureNotSupportedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// 当尝试在未启用该功能的平台上使用该功能时发生的异常。
/// </summary>
public class FeatureNotEnabledException : InvalidOperationException
{
    /// <summary>
    /// 使用指定消息初始化类的新实例。 <see cref="FeatureNotEnabledException"/> class.
    /// </summary>
    public FeatureNotEnabledException()
    {
    }

    /// <summary>
    /// 使用指定消息初始化类的新实例。 <see cref="FeatureNotEnabledException"/> 
    /// </summary>
    /// <param name="message">更详细地描述此异常的消息。</param>
    public FeatureNotEnabledException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// 使用指定消息初始化类的新实例。 <see cref="FeatureNotEnabledException"/>
    /// </summary>
    /// <param name="message">更详细地描述此异常的消息。</param>
    /// <param name="innerException">与此异常相关的内部异常。</param>
    public FeatureNotEnabledException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
