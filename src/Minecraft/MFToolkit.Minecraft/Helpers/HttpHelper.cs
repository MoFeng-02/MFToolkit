using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MFToolkit.Minecraft.Helpers;

/// <summary>
/// HTTP工具类
/// </summary>
public static class HttpHelper
{
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <typeparam name="T">响应类型</typeparam>
    /// <param name="client">HTTP客户端</param>
    /// <param name="url">请求URL</param>
    /// <param name="token">授权令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    public static async Task<T> GetAsync<T>(HttpClient client, string url, string? token = null, CancellationToken cancellationToken = default)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken) ??
               throw new InvalidOperationException("Failed to deserialize response.");
    }
    
    /// <summary>
    /// 发送POST请求
    /// </summary>
    /// <typeparam name="TRequest">请求类型</typeparam>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="client">HTTP客户端</param>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="token">授权令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    public static async Task<TResponse> PostAsync<TRequest, TResponse>(HttpClient client, string url, TRequest data, string? token = null, CancellationToken cancellationToken = default)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(data)
        };
        
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken) ??
               throw new InvalidOperationException("Failed to deserialize response.");
    }
    
    /// <summary>
    /// 发送PUT请求
    /// </summary>
    /// <typeparam name="TRequest">请求类型</typeparam>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="client">HTTP客户端</param>
    /// <param name="url">请求URL</param>
    /// <param name="data">请求数据</param>
    /// <param name="token">授权令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    public static async Task<TResponse> PutAsync<TRequest, TResponse>(HttpClient client, string url, TRequest data, string? token = null, CancellationToken cancellationToken = default)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        
        using var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(data)
        };
        
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken) ??
               throw new InvalidOperationException("Failed to deserialize response.");
    }
    
    /// <summary>
    /// 发送DELETE请求
    /// </summary>
    /// <param name="client">HTTP客户端</param>
    /// <param name="url">请求URL</param>
    /// <param name="token">授权令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public static async Task<bool> DeleteAsync(HttpClient client, string url, string? token = null, CancellationToken cancellationToken = default)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        using var response = await client.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
    
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="client">HTTP客户端</param>
    /// <param name="url">文件URL</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件流</returns>
    public static async Task<Stream> DownloadFileAsync(HttpClient client, string url, CancellationToken cancellationToken = default)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        
        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
    
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <typeparam name="TResponse">响应类型</typeparam>
    /// <param name="client">HTTP客户端</param>
    /// <param name="url">上传URL</param>
    /// <param name="stream">文件流</param>
    /// <param name="fileName">文件名</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="token">授权令牌</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应结果</returns>
    public static async Task<TResponse> UploadFileAsync<TResponse>(HttpClient client, string url, Stream stream, string fileName, string contentType, string? token = null, CancellationToken cancellationToken = default)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        
        if (string.IsNullOrEmpty(contentType))
            throw new ArgumentException("Content type cannot be null or empty.", nameof(contentType));
        
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", fileName);
        
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };
        
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken) ??
               throw new InvalidOperationException("Failed to deserialize response.");
    }
}
