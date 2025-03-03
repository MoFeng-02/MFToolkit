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
public class Demoa : IService
{
}
