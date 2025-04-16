namespace MFToolkit.AutoAttribute.DependencyInjection;


/// <summary>
/// 自动注入特性
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AutoInjectAttribute : Attribute
{
    /// <summary>
    /// 服务类型
    /// </summary>
    public Type? ServiceType { get; }
    /// <summary>
    /// 服务Key
    /// </summary>
    public string? Key { get; }
    /// <summary>
    /// 服务生命周期
    /// </summary>
    public Lifetime Lifetime { get; }

    /// <summary>
    /// 默认自动注入
    /// </summary>
    /// <param name="lifetime">生命周期</param>
    public AutoInjectAttribute(Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = null;
        Key = null;
        Lifetime = lifetime;
    }
    /// <summary>
    /// 默认自动注入
    /// </summary>
    /// <param name="serviceType">服务类型</param>
    /// <param name="lifetime">生命周期</param>
    public AutoInjectAttribute(Type serviceType, Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = serviceType;
        Key = null;
        Lifetime = lifetime;
    }
    /// <summary>
    /// Key自动注入
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="lifetime">生命周期</param>
    public AutoInjectAttribute(string key, Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = null;
        Key = key;
        Lifetime = lifetime;
    }
    /// <summary>
    /// 类型自动注入
    /// </summary>
    /// <param name="serviceType">服务类型</param>
    /// <param name="key">Key</param>
    /// <param name="lifetime">生命周期</param>
    public AutoInjectAttribute(Type serviceType, string? key = null, Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = serviceType;
        Key = key;
        Lifetime = lifetime;
    }
}
/// <summary>
/// 泛型注入特性
/// </summary>
/// <typeparam name="TService">服务类型</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AutoInjectAttribute<TService> : AutoInjectAttribute
{
    /// <summary>
    /// 泛型默认自动注入
    /// </summary>
    /// <param name="lifetime">生命周期</param>
    public AutoInjectAttribute(Lifetime lifetime = Lifetime.Transient) : base(typeof(TService), lifetime)
    {
    }
    /// <summary>
    /// 泛型默认自动注入
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="lifetime">生命周期</param>
    public AutoInjectAttribute(string key, Lifetime lifetime = Lifetime.Transient) : base(typeof(TService), key, lifetime)
    {
    }
}

/// <summary>
/// 命名空间自动注入特性，让AutoInjectAttribute引用类库支持自定义命名空间，以及Service的{ServiceName}名称，默认是AddAutoInjectServices，全不提供则默认
/// <para>
/// 只需要在任意类上添加AutoInjectNamespaceAttribute特性，即可注册当前类库全局命名空间和服务名称，解决默认命名空间以及算法命名空间冲突问题。
/// </para>
/// </summary>
/// <param name="nameSpace"></param>
/// <param name="authServiceName">默认名称是AddAutoInjectServices</param>
/// <remarks>
/// 示例：
/// [AutoInjectNamespace("Demo01.Services", "AddDemo01Services")]
/// [AutoInject{IService}]
/// public class Service : IService
/// {
/// 
/// }
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AutoInjectNamespaceAttribute(string nameSpace, string? authServiceName = "AddAutoInjectServices") : Attribute
{
    /// <summary>
    /// 命名空间
    /// </summary>
    public string? NameSpace { get; } = nameSpace;

    /// <summary>
    /// 服务名称
    /// </summary>
    public string? AuthServiceName { get; } = authServiceName;
}