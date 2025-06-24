using MFToolkit.AutoAttribute.DependencyInjection;

namespace GeneratorDemo.DI;

public interface IService
{
}
public interface IService2
{
}
[AutoInjectServiceName("AddGeneratorDemoServicess")]
[AutoInject]
[AutoTryInject<IService>]
[AutoTryInject(typeof(IService),"0")]
[AutoTryInject<IService2>("1", lifetime: Lifetime.Scoped)]
[AutoTryInject(Lifetime.Singleton)]
[AutoTryInject("2",Lifetime.Scoped)]
public class Demoa : IService, IService2
{
}
