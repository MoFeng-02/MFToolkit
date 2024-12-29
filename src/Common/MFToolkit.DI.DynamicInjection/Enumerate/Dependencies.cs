namespace MFToolkit.DI.DynamicInjection.Enumerate;

public enum Dependencies
{
    /// <summary>
    /// 瞬态
    /// </summary>
    Transient,
    /// <summary>
    /// 范围，领域
    /// </summary>
    Scoped,
    /// <summary>
    /// 单例
    /// </summary>
    Singleton
}