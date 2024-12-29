using System;
using MFToolkit.DI.DynamicInjection.Enumerate;

namespace MFToolkit.DI.DynamicInjection.Attributes;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class MFInjectableAttribute : Attribute
{
    /// <summary>
    /// 生命周期
    /// </summary>
    public Dependencies Dependencies { get; set; }
    /// <summary>
    /// 实现类型
    /// </summary>
    public Type? ImplementationType { get; set; }
    // 支持非泛型和泛型两种情况
    public MFInjectableAttribute() { }

    public MFInjectableAttribute(Type implementationType, Dependencies dependencies = Dependencies.Transient)
    {
        ImplementationType = implementationType;
        Dependencies = dependencies;
    }
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class MFInjectableAttribute<TService> : MFInjectableAttribute
{
    // 泛型构造函数
    public MFInjectableAttribute(Dependencies dependencies = Dependencies.Transient) : base(typeof(TService), dependencies)
    {
    }
}