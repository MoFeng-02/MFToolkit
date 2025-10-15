namespace MFToolkit.Abstractions.DependencyInjection;

/// <summary>
/// 作用域服务自动Try注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TryScopedAttribute : AutoTryInjectAttribute
{
    /// <summary>
    /// 作用域服务自动Try注入（默认生命周期：Scoped）
    /// </summary>
    public TryScopedAttribute() : base(Lifetime.Scoped) { }

    /// <summary>
    /// 作用域服务自动Try注入（指定服务类型）
    /// </summary>
    public TryScopedAttribute(Type serviceType) : base(serviceType, Lifetime.Scoped) { }

    /// <summary>
    /// 带Key的作用域服务自动Try注入
    /// </summary>
    public TryScopedAttribute(object? key) : base(key, Lifetime.Scoped) { }

    /// <summary>
    /// 带类型和Key的作用域服务自动Try注入
    /// </summary>
    public TryScopedAttribute(Type serviceType, object? key) : base(serviceType, key, Lifetime.Scoped) { }
}

/// <summary>
/// 泛型作用域服务自动Try注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TryScopedAttribute<TService> : AutoTryInjectAttribute<TService>
{
    /// <summary>
    /// 泛型作用域服务自动Try注入
    /// </summary>
    public TryScopedAttribute() : base(Lifetime.Scoped) { }

    /// <summary>
    /// 带Key的泛型作用域服务自动Try注入
    /// </summary>
    public TryScopedAttribute(object? key) : base(key, Lifetime.Scoped) { }
}
