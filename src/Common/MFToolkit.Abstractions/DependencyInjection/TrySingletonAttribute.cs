namespace MFToolkit.Abstractions.DependencyInjection;

/// <summary>
/// 单例服务自动Try注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TrySingletonAttribute : AutoTryInjectAttribute
{
    /// <summary>
    /// 单例服务自动Try注入（默认生命周期：Singleton）
    /// </summary>
    public TrySingletonAttribute() : base(Lifetime.Singleton) { }

    /// <summary>
    /// 单例服务自动Try注入（指定服务类型）
    /// </summary>
    public TrySingletonAttribute(Type serviceType) : base(serviceType, Lifetime.Singleton) { }

    /// <summary>
    /// 带Key的单例服务自动Try注入
    /// </summary>
    public TrySingletonAttribute(string key) : base(key, Lifetime.Singleton) { }

    /// <summary>
    /// 带类型和Key的单例服务自动Try注入
    /// </summary>
    public TrySingletonAttribute(Type serviceType, string? key) : base(serviceType, key, Lifetime.Singleton) { }
}

/// <summary>
/// 泛型单例服务自动Try注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TrySingletonAttribute<TService> : AutoTryInjectAttribute<TService>
{
    /// <summary>
    /// 泛型单例服务自动Try注入
    /// </summary>
    public TrySingletonAttribute() : base(Lifetime.Singleton) { }

    /// <summary>
    /// 带Key的泛型单例服务自动Try注入
    /// </summary>
    public TrySingletonAttribute(string key) : base(key, Lifetime.Singleton) { }
}
