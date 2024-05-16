namespace MFToolkit.Http;
/// <summary>
/// 提供类型化HTTPClient服务
/// </summary>
public class HttpClientService(HttpClient httpClient) : HttpClient, IDisposable
{
    public HttpClient GetHttpClient() => httpClient;
    public new void Dispose()
    {
        httpClient?.Dispose();
        base.Dispose();
    }
}