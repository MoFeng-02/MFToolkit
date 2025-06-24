namespace MFToolkit.AutoAttribute.DependencyInjection;

/// <summary>
/// 服务生命周期
/// </summary>
public enum Lifetime
{
    /// <summary>
    /// 瞬态
    /// <para>特点：每次请求都创建新实例</para>
    /// <para>适用场景：无状态、计算密集服务</para>
    /// <para>不适用常见：需要缓存数据的服务</para>
    /// </summary>
    Transient,
    /// <summary>
    /// 作用域
    /// <para>特点：每个请求内保持单实例</para>
    /// <para>适用场景：有状态服务、DbContext依赖</para>
    /// <para>不适用常见：跨请求状态共享服务</para>
    /// </summary>
    Scoped,
    /// <summary>
    /// 单例
    /// <para>特点：整个应用使用单实例</para>
    /// <para>适用场景：配置服务、缓存服务</para>
    /// <para>不适用常见：线程不安全服务</para>
    /// </summary>
    Singleton
}