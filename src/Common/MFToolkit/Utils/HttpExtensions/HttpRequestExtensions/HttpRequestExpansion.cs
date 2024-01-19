namespace MFToolkit.Utils.HttpExtensions.HttpRequestExtensions;
internal static class HttpRequestExtension
{
    /// <summary>
    /// 基础请求路径
    /// </summary>
    private static Uri baseUri;
    internal static void HttpRequestInit(Uri uri)
    {
        baseUri = uri;
    }

    internal static Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage)
    {
        using var httpClient = HttpUtil.CreateHttpClient("SEND");
        requestMessage.RequestUri = new Uri((baseUri + requestMessage.RequestUri?.AbsoluteUri));
        return httpClient.SendAsync(requestMessage);
    }
    internal static Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption httpCompletionOption)
    {
        using var httpClient = HttpUtil.CreateHttpClient("GET");
        return httpClient.GetAsync(baseUri + requestUri, httpCompletionOption);
    }
    internal static Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent httpContent)
    {
        using var httpClient = HttpUtil.CreateHttpClient("POST");
        return httpClient.PostAsync(baseUri + requestUri, httpContent);
    }
    internal static Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent httpContent)
    {
        using var httpClient = HttpUtil.CreateHttpClient("PUT");
        return httpClient.PutAsync(baseUri + requestUri, httpContent);
    }
    internal static Task<HttpResponseMessage> DeleteAsync(string requestUri)
    {
        using var httpClient = HttpUtil.CreateHttpClient("DELETE");
        return httpClient.DeleteAsync(baseUri + requestUri);
    }
}
