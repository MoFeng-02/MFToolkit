namespace MFToolkit.Abstractions.DependencyInjection;


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
    public object? Key { get; }
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
    public AutoInjectAttribute(object? key, Lifetime lifetime = Lifetime.Transient)
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
    public AutoInjectAttribute(Type serviceType, object? key, Lifetime lifetime = Lifetime.Transient)
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
public class AutoInjectAttribute<TService> : AutoInjectAttribute
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
    public AutoInjectAttribute(object? key, Lifetime lifetime = Lifetime.Transient) : base(typeof(TService), key, lifetime)
    {
    }
}


/// <summary>
/// 自定义服务名称，让AutoInjectAttribute引用类库支持自定义服务名称，默认是AddAutoInjectServices，全不提供则默认
/// <para>
/// 只需要在任意类上添加AutoInjectNamespaceAttribute特性，即可注册服务名称。
/// </para>
/// </summary>
/// <param name="autoServiceName">默认名称是AddAutoInjectServices</param>
/// <remarks>
/// 示例：
/// [AutoInjectNamespace("AddDemo01Services")]
/// [AutoInject{IService}]
/// public class Service : IService
/// {
/// 
/// }
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AutoInjectServiceNameAttribute(string autoServiceName = "AddAutoInjectServices") : Attribute
{

    /// <summary>
    /// 服务名称
    /// </summary>
    public string AutoServiceName { get; } = autoServiceName;
}