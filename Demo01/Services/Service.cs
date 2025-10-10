using MFToolkit.Abstractions.DependencyInjection;

namespace Demo01.Services
{
    [AutoInject]
    [AutoInject<IService>]
    public class Service : IService
    {
    }
}
