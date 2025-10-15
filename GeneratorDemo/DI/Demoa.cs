using MFToolkit.Abstractions.DependencyInjection;

namespace GeneratorDemo.DI;

public interface IService
{
}
public interface IService2
{
}
[AutoInjectServiceName("AddGeneratorDemoServicess")]
[AutoInject<IService>("AutoIServiceSingleton", lifetime: Lifetime.Singleton)]
[AutoInject(typeof(string))]
[AutoInject(typeof(string[][]))]
[AutoTryInject<IService>("AutoIServiceTryInject")]
[AutoTryInject(typeof(IService), "0IServiceAutoTryInject")]
[AutoTryInject<IService2>("1IService2Scoped", lifetime: Lifetime.Scoped)]
[AutoTryInject(Lifetime.Singleton)]
[AutoTryInject("2", Lifetime.Scoped)]
[Singleton("Singleton")]
[Singleton<IService>("Singleton<>")]
[Scoped]
[Scoped<IService>]
[Transient(typeof(int))]
[Transient<IService2>("Transient")]
[TryScoped(12f)]
[TryScoped(123)]
[TryScoped<IService>(typeof(IService2))]
[TryScoped<IService2>("TryScoped")]
[TryScoped<IService2>(true)]
public class Demoa : IService, IService2
{
}
