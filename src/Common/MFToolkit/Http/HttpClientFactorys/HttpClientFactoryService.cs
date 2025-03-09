namespace MFToolkit.Http.HttpClientFactorys;

/// <summary>
/// 记得一定要这样
/// <para>
/// <code>
/// builder.Services.AddHttpClient();
/// builder.Services.AddSingleton{HttpClientFactoryService}(); || builder.Services.AddScoped{HttpClientFactoryService}();
/// </code>
/// </para>
/// </summary>
public class HttpClientFactoryService
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// HttpClientFactoryService
    /// </summary>
    /// <param name="httpClientFactory"></param>
    public HttpClientFactoryService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    /// <summary>
    /// 创建相关的HttpClient
    /// </summary>
    /// <param name="name">HttpClient的逻辑名称</param>
    /// <returns></returns>
    public HttpClient CreateHttpClient(string name = "")
    {
        var httpClient = _httpClientFactory.CreateClient(name);
        return httpClient;
    }
}