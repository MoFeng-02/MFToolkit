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
    public static IHttpClientBuilder AddHttpClientService(this IServiceCollection services, Action<HttpClient>? configureClient = null)
    {
        configureClient ??= (config) =>
        {
        };
        var httpClientBuilder = services.AddHttpClient<HttpClientService>(configureClient);
        return httpClientBuilder;
    }
    /// <summary>
    /// 注册强类型化HttpClientService
    /// </summary>
    /// <typeparam name="TClient">强类型HttpClientService</typeparam>
    /// <param name="services"></param>
    /// <param name="configureClient"></param>
    /// <returns></returns>
    public static IHttpClientBuilder AddHttpClientService<TClient>(this IServiceCollection services, Action<HttpClient>? configureClient = null) where TClient : HttpClientService
    {
        configureClient ??= (config) =>
        {
        };
        var httpClientBuilder = services.AddHttpClient<TClient>(configureClient);
        return httpClientBuilder;
    }
}
