namespace MFToolkit.AutoAttribute.DependencyInjection;
/// <summary>
/// 自动注入Try服务特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AutoTryInjectAttribute : AutoInjectAttribute
{
    /// <summary>
    /// 默认自动注入
    /// </summary>
    /// <param name="lifetime">生命周期</param>
    public AutoTryInjectAttribute(Lifetime lifetime = Lifetime.Transient) : base(lifetime)
    {
    }
    /// <summary>
    /// 默认自动注入
    /// </summary>
    /// <param name="serviceType">服务类型</param>
    /// <param name="lifetime">生命周期</param>
    public AutoTryInjectAttribute(Type serviceType, Lifetime lifetime = Lifetime.Transient) : base(serviceType, lifetime)
    {
    }
    /// <summary>
    /// Key自动注入
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="lifetime">生命周期</param>
    public AutoTryInjectAttribute(string key, Lifetime lifetime = Lifetime.Transient) : base(key, lifetime)
    {
    }
    /// <summary>
    /// 类型自动注入
    /// </summary>
    /// <param name="serviceType">服务类型</param>
    /// <param name="key">Key</param>
    /// <param name="lifetime">生命周期</param>
    public AutoTryInjectAttribute(Type serviceType, string? key, Lifetime lifetime = Lifetime.Transient) : base(serviceType, key, lifetime)
    {
    }
}

/// <summary>
/// 自动注入Try泛型服务特性
/// </summary>
/// <typeparam name="TService"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AutoTryInjectAttribute<TService> : AutoInjectAttribute<TService>
{
    /// <summary>
    /// 泛型默认自动注入
    /// </summary>
    /// <param name="lifetime">生命周期</param>
    public AutoTryInjectAttribute(Lifetime lifetime = Lifetime.Transient) : base(lifetime)
    {
    }
    /// <summary>
    /// 泛型默认自动注入
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="lifetime">生命周期</param>
    public AutoTryInjectAttribute(string key, Lifetime lifetime = Lifetime.Transient) : base(key, lifetime)
    {
    }
}