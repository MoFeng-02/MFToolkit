using MFToolkit.AutoAttribute.DependencyInjection;

namespace Demo.Admin.Services1.Services
{
    [AutoInjectServiceName("AddDemoAdminServices1")]
    [AutoInject]
    [AutoInject<IService>]
    public class Service : IService
    {
    }
}
