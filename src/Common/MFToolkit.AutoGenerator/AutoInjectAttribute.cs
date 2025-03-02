namespace MFToolkit.AutoGenerator;

public enum Lifetime
{
    Transient,
    Scoped,
    Singleton
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AutoInjectAttribute : Attribute
{
    public Type? ServiceType { get; }
    public string? Key { get; }
    public Lifetime Lifetime { get; }

    public AutoInjectAttribute(Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = null;
        Key = null;
        Lifetime = lifetime;
    }
#if NET8
    public AutoInjectAttribute(string key, Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = null;
        Key = key;
        Lifetime = lifetime;
    }
    public AutoInjectAttribute(Type serviceType, string? key = null, Lifetime lifetime = Lifetime.Transient)
    {
        ServiceType = serviceType;
        Key = key;
        Lifetime = lifetime;
    }
#endif
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AutoInjectAttribute<TService> : Attribute
{
    public string? Key { get; }
    public Lifetime Lifetime { get; }

    public AutoInjectAttribute(string? key = null, Lifetime lifetime = Lifetime.Transient)
    {
        Key = key;
        Lifetime = lifetime;
    }
}