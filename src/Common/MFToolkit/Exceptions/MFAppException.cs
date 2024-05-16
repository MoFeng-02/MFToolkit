namespace MFToolkit.Exceptions;
public static class MFAppException
{
    /// <summary>
    /// 未实现MFApp异常
    /// </summary>
    public static MFUnRealizedException UnRealizedException = new();
}

/// <summary>
/// 未实现MFApp异常
/// </summary>
public class MFUnRealizedException : SystemException
{
    public MFUnRealizedException() : base("未实现/注册 MFApp ，请在程序入口Services注册后注入 services.AddMFAppService() 以绑定")
    {
    }
}
