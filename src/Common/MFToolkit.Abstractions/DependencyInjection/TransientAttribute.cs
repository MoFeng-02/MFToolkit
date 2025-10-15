namespace MFToolkit.Abstractions.DependencyInjection;

/// <summary>
/// 瞬态服务自动注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TransientAttribute : AutoInjectAttribute
{
    /// <summary>
    /// 瞬态服务自动注入（默认生命周期：Transient）
    /// </summary>
    public TransientAttribute() : base(Lifetime.Transient) { }

    /// <summary>
    /// 瞬态服务自动注入（指定服务类型）
    /// </summary>
    public TransientAttribute(Type serviceType) : base(serviceType, Lifetime.Transient) { }

    /// <summary>
    /// 带Key的瞬态服务自动注入
    /// </summary>
    public TransientAttribute(object? key) : base(key, Lifetime.Transient) { }

    /// <summary>
    /// 带类型和Key的瞬态服务自动注入
    /// </summary>
    public TransientAttribute(Type serviceType, object? key) : base(serviceType, key, Lifetime.Transient) { }
}

/// <summary>
/// 泛型瞬态服务自动注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TransientAttribute<TService> : AutoInjectAttribute<TService>
{
    /// <summary>
    /// 泛型瞬态服务自动注入
    /// </summary>
    public TransientAttribute() : base(Lifetime.Transient) { }

    /// <summary>
    /// 带Key的泛型瞬态服务自动注入
    /// </summary>
    public TransientAttribute(object? key) : base(key, Lifetime.Transient) { }
}
