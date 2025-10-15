using MFToolkit.Abstractions.DependencyInjection;

namespace GeneratorDemo.DI;

public interface IService
{
}
public interface IService2
{
}
[AutoInjectServiceName("AddGeneratorDemoServicess")]
//[AutoInject]
//[AutoTryInject<IService>]
//[AutoTryInject(typeof(IService),"0")]
//[AutoTryInject<IService2>("1", lifetime: Lifetime.Scoped)]
//[AutoTryInject(Lifetime.Singleton)]
//[AutoTryInject("2",Lifetime.Scoped)]
[Singleton]
[Singleton<IService>]
[Scoped]
[Scoped<IService>]
[Transient]
[Transient<IService2>("12")]
[TryScoped]
[TryScoped<IService>]
[TryScoped<IService2>("123")]
public class Demoa : IService, IService2
{
}
