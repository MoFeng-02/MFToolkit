namespace MFToolkit.AutoGenerator;

/// <summary>
/// 生命周期
/// </summary>
public enum Lifetime
{
    /// <summary>
    /// 瞬态
    /// </summary>
    Transient,
    /// <summary>
    /// 作用域
    /// </summary>
    Scoped,
    /// <summary>
    /// 单例
    /// </summary>
    Singleton
}