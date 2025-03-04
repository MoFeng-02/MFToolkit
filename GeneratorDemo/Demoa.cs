using MFToolkit.AutoAttribute;

namespace GeneratorDemo;

public interface IService
{
}
public interface IService2
{
}
[AutoInject]
[AutoInject<IService>(lifetime: Lifetime.Scoped)]
[AutoInject<IService2>("1", lifetime: Lifetime.Scoped)]
[AutoInject("12")]
public class Demoa : IService, IService2
{
}
