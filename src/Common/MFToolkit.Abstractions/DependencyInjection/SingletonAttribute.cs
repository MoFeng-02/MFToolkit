namespace MFToolkit.Abstractions.DependencyInjection;

/// <summary>
/// 单例服务自动注入特性
/// </summary>
/// <remarks>
/// 服务类型说明：
/// <para>注意：如果指定的类型不是当前类的基类或实现的接口，该参数将被视为服务Key</para>
/// <para>示例1（正确用法）：[Singleton(typeof(IMyService))] - 当当前类实现了 IMyService 时</para>
/// <para>示例2（作为Key）：[Singleton(typeof(string))] - 当当前类没有实现 string 时，typeof(string) 将被当作Key</para>
/// <para>提示：此规则适用于所有生命周期特性（Singleton、Scoped、Transient、TrySingleton 等，自动化AutoInject等也同样如此，泛型注入除外）</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SingletonAttribute : AutoInjectAttribute
{
    /// <summary>
    /// 单例服务自动注入（默认生命周期：Singleton）
    /// </summary>
    public SingletonAttribute() : base(Lifetime.Singleton) { }

    /// <summary>
    /// 单例服务自动注入（指定服务类型）
    /// <param name="serviceType">
    /// 服务类型
    /// <para>注意：如果指定的类型不是当前类的基类或实现的接口，该参数将被视为服务Key</para>
    /// <para>示例1（正确用法）：[特性生命周期(typeof(IMyService))] - 当当前类实现了 IMyService 时</para>
    /// <para>示例2（作为Key）：[特性生命周期(typeof(string))] - 当当前类没有实现 string 时，typeof(string) 将被当作Key</para>
    /// </param>
    /// </summary>
    public SingletonAttribute(Type serviceType) : base(serviceType, Lifetime.Singleton) { }

    /// <summary>
    /// 带Key的单例服务自动注入
    /// </summary>
    public SingletonAttribute(object? key) : base(key, Lifetime.Singleton) { }

    /// <summary>
    /// 带类型和Key的单例服务自动注入
    /// </summary>
    public SingletonAttribute(Type serviceType, object? key) : base(serviceType, key, Lifetime.Singleton) { }
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
    public SingletonAttribute(object? key) : base(key, Lifetime.Singleton) { }
}
