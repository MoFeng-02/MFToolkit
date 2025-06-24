using MFToolkit.AutoAttribute.DependencyInjection;

namespace Demo.Admin.Services2.Services
{
    [AutoInjectServiceName("AddDemoAdminServices2")]
    [AutoInject]
    [AutoInject<IService>]
    public class Service : IService
    {
    }
}
