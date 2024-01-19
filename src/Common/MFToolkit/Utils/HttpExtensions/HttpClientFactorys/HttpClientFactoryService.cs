namespace MFToolkit.Utils.HttpExtensions.HttpClientFactorys;

/// <summary>
/// 记得一定要这样
/// <code>
/// builder.Services.AddHttpClient();
/// builder.Services.AddSingleton<HttpClientFactoryService>(); || builder.Services.AddScoped<HttpClientFactoryService>();
/// </code>
/// </summary>
public class HttpClientFactoryService
{
    private readonly IHttpClientFactory _httpClientFactory;

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