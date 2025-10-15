namespace MFToolkit.Abstractions.DependencyInjection;

/// <summary>
/// 单例服务自动注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SingletonAttribute : AutoInjectAttribute
{
    /// <summary>
    /// 单例服务自动注入（默认生命周期：Singleton）
    /// </summary>
    public SingletonAttribute() : base(Lifetime.Singleton) { }

    /// <summary>
    /// 单例服务自动注入（指定服务类型）
    /// </summary>
    public SingletonAttribute(Type serviceType) : base(serviceType, Lifetime.Singleton) { }

    /// <summary>
    /// 带Key的单例服务自动注入
    /// </summary>
    public SingletonAttribute(string key) : base(key, Lifetime.Singleton) { }

    /// <summary>
    /// 带类型和Key的单例服务自动注入
    /// </summary>
    public SingletonAttribute(Type serviceType, string? key) : base(serviceType, key, Lifetime.Singleton) { }
}

/// <summary>
/// 泛型单例服务自动注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SingletonAttribute<TService> : AutoInjectAttribute<TService>
{
    /// <summary>
    /// 泛型单例服务自动注入
    /// </summary>
    public SingletonAttribute() : base(Lifetime.Singleton) { }

    /// <summary>
    /// 带Key的泛型单例服务自动注入
    /// </summary>
    public SingletonAttribute(string key) : base(key, Lifetime.Singleton) { }
}
