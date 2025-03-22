using MFToolkit.AutoAttribute.DependencyInjection;

namespace GeneratorDemo.DI;

public interface IService
{
}
public interface IService2
{
}
//[AutoInject]
//[AutoInject<IService>]
//[AutoInject<IService2>("1", lifetime: Lifetime.Scoped)]
//[AutoInject(Lifetime.Singleton)]
public class Demoa : IService, IService2
{
}
