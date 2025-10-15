namespace MFToolkit.Abstractions.DependencyInjection;

/// <summary>
/// 作用域服务自动注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ScopedAttribute : AutoInjectAttribute
{
    /// <summary>
    /// 作用域服务自动注入（默认生命周期：Scoped）
    /// </summary>
    public ScopedAttribute() : base(Lifetime.Scoped) { }

    /// <summary>
    /// 作用域服务自动注入（指定服务类型）
    /// </summary>
    public ScopedAttribute(Type serviceType) : base(serviceType, Lifetime.Scoped) { }

    /// <summary>
    /// 带Key的作用域服务自动注入
    /// </summary>
    public ScopedAttribute(string key) : base(key, Lifetime.Scoped) { }

    /// <summary>
    /// 带类型和Key的作用域服务自动注入
    /// </summary>
    public ScopedAttribute(Type serviceType, string? key) : base(serviceType, key, Lifetime.Scoped) { }
}

/// <summary>
/// 泛型作用域服务自动注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ScopedAttribute<TService> : AutoInjectAttribute<TService>
{
    /// <summary>
    /// 泛型作用域服务自动注入
    /// </summary>
    public ScopedAttribute() : base(Lifetime.Scoped) { }

    /// <summary>
    /// 带Key的泛型作用域服务自动注入
    /// </summary>
    public ScopedAttribute(string key) : base(key, Lifetime.Scoped) { }
}
