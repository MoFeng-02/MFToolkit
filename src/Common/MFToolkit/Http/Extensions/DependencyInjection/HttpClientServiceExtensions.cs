using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Http.Extensions.DependencyInjection;
/// <summary>
/// 注册HttpClientService的依赖注入
/// </summary>
public static class HttpClientServiceExtensions
{
    /// <summary>
    /// 注册强类型化HttpClientService
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureClient"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddHttpClientService(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        var httpClientBuilder = services.AddHttpClient<HttpClientService>(configureClient);
        return httpClientBuilder;
    }
}
