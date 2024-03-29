using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace MFToolkit.Utils.HttpExtensions;
/// <summary>
/// 提供类型化HTTPClient服务
/// </summary>
public class HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger) : HttpClient, IDisposable
{
    //private readonly HttpClient _httpClient = httpClient;
    //private readonly ILogger<HttpClientService> _logger = logger;
    public HttpClient GetHttpClient() => httpClient;

    //#region Public Send

    //#region Simple Get Overloads

    //public Task<string> GetStringAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri) =>
    //    httpClient.GetStringAsync(requestUri);

    //public Task<string> GetStringAsync(Uri? requestUri) =>
    //    httpClient.GetStringAsync(requestUri, CancellationToken.None);

    //public Task<string> GetStringAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken) =>
    //    httpClient.GetStringAsync(requestUri, cancellationToken);

    //public Task<string> GetStringAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.GetStringAsync(requestUri, cancellationToken);
    //public Task<byte[]> GetByteArrayAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri) => httpClient.GetByteArrayAsync(requestUri);

    //public Task<byte[]> GetByteArrayAsync(Uri? requestUri) =>
    //   httpClient.GetByteArrayAsync(requestUri, CancellationToken.None);

    //public Task<byte[]> GetByteArrayAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken) =>
    //    httpClient.GetByteArrayAsync(requestUri, cancellationToken);

    //public Task<byte[]> GetByteArrayAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.GetByteArrayAsync(requestUri, cancellationToken);

    //public Task<Stream> GetStreamAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri) =>
    //    httpClient.GetStreamAsync(requestUri);

    //public Task<Stream> GetStreamAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken) =>
    //  httpClient.GetStreamAsync(requestUri, cancellationToken);

    //public Task<Stream> GetStreamAsync(Uri? requestUri) =>
    //    httpClient.GetStreamAsync(requestUri, CancellationToken.None);

    //public Task<Stream> GetStreamAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.GetStreamAsync(requestUri, cancellationToken);

    //#endregion Simple Get Overloads

    //#region REST Send Overloads

    //public Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri) =>
    //   httpClient.GetAsync(requestUri);

    //public Task<HttpResponseMessage> GetAsync(Uri? requestUri) => httpClient.GetAsync(requestUri);

    //public Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpCompletionOption completionOption) =>
    //    httpClient.GetAsync(requestUri, completionOption);

    //public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption) =>
    //   httpClient.GetAsync(requestUri, completionOption, CancellationToken.None);

    //public Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken) =>
    //   httpClient.GetAsync(requestUri, cancellationToken);

    //public Task<HttpResponseMessage> GetAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.GetAsync(requestUri, cancellationToken);

    //public Task<HttpResponseMessage> GetAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) =>
    //   httpClient.GetAsync(requestUri, completionOption, cancellationToken);

    //public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken) => httpClient.GetAsync(requestUri, completionOption, cancellationToken);

    //public Task<HttpResponseMessage> PostAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content) =>
    //    httpClient.PostAsync(requestUri, content);

    //public Task<HttpResponseMessage> PostAsync(Uri? requestUri, HttpContent? content) =>
    //   httpClient.PostAsync(requestUri, content, CancellationToken.None);

    //public Task<HttpResponseMessage> PostAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content, CancellationToken cancellationToken) =>
    //   httpClient.PostAsync(requestUri, content, cancellationToken);

    //public Task<HttpResponseMessage> PostAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PostAsync(requestUri, content, cancellationToken);

    //public Task<HttpResponseMessage> PutAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content) =>
    //    httpClient.PutAsync(requestUri, content);

    //public Task<HttpResponseMessage> PutAsync(Uri? requestUri, HttpContent? content) =>
    //   httpClient.PutAsync(requestUri, content, CancellationToken.None);

    //public Task<HttpResponseMessage> PutAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content, CancellationToken cancellationToken) =>
    //   httpClient.PutAsync(requestUri, content, cancellationToken);

    //public Task<HttpResponseMessage> PutAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PutAsync(requestUri, content, cancellationToken);

    //public Task<HttpResponseMessage> PatchAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content) =>
    //    httpClient.PatchAsync(requestUri, content);

    //public Task<HttpResponseMessage> PatchAsync(Uri? requestUri, HttpContent? content) =>
    //    httpClient.PatchAsync(requestUri, content, CancellationToken.None);

    //public Task<HttpResponseMessage> PatchAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, HttpContent? content, CancellationToken cancellationToken) =>
    //    httpClient.PatchAsync(requestUri, content, cancellationToken);

    //public Task<HttpResponseMessage> PatchAsync(Uri? requestUri, HttpContent? content, CancellationToken cancellationToken) => httpClient.PatchAsync(requestUri, content, cancellationToken);

    //public Task<HttpResponseMessage> DeleteAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri) =>
    //   httpClient.DeleteAsync(requestUri);

    //public Task<HttpResponseMessage> DeleteAsync(Uri? requestUri) =>
    //    httpClient.DeleteAsync(requestUri, CancellationToken.None);

    //public Task<HttpResponseMessage> DeleteAsync([StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, CancellationToken cancellationToken) =>
    //  httpClient.DeleteAsync(requestUri, cancellationToken);

    //public Task<HttpResponseMessage> DeleteAsync(Uri? requestUri, CancellationToken cancellationToken) => httpClient.DeleteAsync(requestUri, cancellationToken);

    //#endregion REST Send Overloads

    //#endregion Publish Send

    public new void Dispose()
    {
        httpClient?.Dispose();
        base.Dispose();
    }
}