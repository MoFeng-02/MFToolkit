namespace MFToolkit.AutoAttribute;


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
    /// <param name="lifetime"></param>
    public AutoInjectAttribute(Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = null;
        Key = null;
        Lifetime = lifetime;
    }
    /// <summary>
    /// Key自动注入
    /// </summary>
    /// <param name="key"></param>
    /// <param name="lifetime"></param>
    public AutoInjectAttribute(string key, Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = null;
        Key = key;
        Lifetime = lifetime;
    }
    /// <summary>
    /// 类型自动注入
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="key"></param>
    /// <param name="lifetime"></param>
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
/// <typeparam name="TService"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AutoInjectAttribute<TService> : AutoInjectAttribute
{
    /// <summary>
    /// 泛型默认自动注入
    /// </summary>
    /// <param name="key"></param>
    /// <param name="lifetime"></param>
    public AutoInjectAttribute(string? key = null, Lifetime lifetime = Lifetime.Transient) : base(typeof(TService), key, lifetime)
    {
    }
}