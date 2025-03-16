using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using MFToolkit.App;
using MFToolkit.Exceptions;
using MFToolkit.Http.HttpClientFactorys;
using MFToolkit.Http.Models;
using MFToolkit.Json.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Http;

/// <summary>
/// Http 工具集
/// </summary>
public sealed class HttpUtil
{
    private static Uri? _baseUri;

    /// <summary>
    /// 初始化快捷请求信息HttpRequest
    /// </summary>
    /// <param name="uri">请求基路由</param>
    public static void InitRequest(Uri? uri)
    {
        _baseUri = uri;
        //HttpRequestExtension.HttpRequestInit(baseUri);
    }

    /// <summary>
    /// 获取强类型HTTPClient
    /// </summary>
    /// <returns></returns>
    public static HttpClientService GetHttpClientService()
    {
        return MFApp.GetService<HttpClientService>() ?? throw new(
            "未注册HttpClientService，参考链接：https://learn.microsoft.com/zh-cn/dotnet/core/extensions/httpclient-factory#typed-clients");
    }

    /// <summary>
    /// 创建HttpClient实例
    /// </summary>
    /// <param name="name">Http Client 的逻辑名称</param>
    /// <param name="isAddBaseUri">是否添加基础请求路由</param>
    /// <returns></returns>
    public static HttpClient CreateHttpClient(string name = "", bool isAddBaseUri = true)
    {
        var httpClient = MFApp.GetService<HttpClientFactoryService>()?.CreateHttpClient(name) ?? throw MFAppException.UnRealizedException;
        if (isAddBaseUri) httpClient.BaseAddress = _baseUri ?? default;
        return httpClient;
    }

    /// <summary>
    /// 设置用户的登录凭证
    /// </summary>
    /// <param name="httpName">创建的HttpClient的逻辑名称</param>
    /// <param name="scheme"></param>
    /// <param name="parameter">传递过去的参数</param>
    /// <param name="isAddBaseUri">是否添加基础请求路由</param>
    /// <returns></returns>
    public static HttpClient SetAuthorization(string httpName = "fast", string scheme = "Bearer",
        string? parameter = null, bool isAddBaseUri = true)
    {
        var httpClient = CreateHttpClient(httpName, isAddBaseUri);
        httpClient.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(parameter) ? new(scheme) : new(scheme, parameter);
        return httpClient;
    }


    /// <summary>
    /// 设置JSON Body
    /// </summary>
    /// <param name="body"></param>
    /// <param name="context">如果是AOT的情况下手动提供Json序列化上下文</param>
    /// <returns></returns>
    public static HttpContent SetBodyAsJson<T>(T body, JsonSerializerContext? context = null) where T : notnull
    {
        var httpContent = new StringContent(body.ValueToJson(context: context)!,
            new MediaTypeHeaderValue("application/json"));
        return httpContent;
    }

    /// <summary>
    /// 设置上传文件
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static HttpContent SetBodyAsMultipart(Stream stream, string? fileName = null)
    {
        var httpContent = new MultipartFormDataContent();
        string boundary = string.Format("--{0}", DateTime.Now.Ticks.ToString("x"));
        httpContent.Headers.Add("ContentType", $"multipart/form-data, boundary={boundary}");
        if (fileName == null) httpContent.Add(new StreamContent(stream, (int)stream.Length), "file");
        else httpContent.Add(new StreamContent(stream, (int)stream.Length), "file", fileName);
        return httpContent;
    }

    /// <summary>
    /// 设置多个上传文件
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static HttpContent SetBodyAsMultipart(List<FileInfo> files)
    {
        var httpContent = new MultipartFormDataContent();
        string boundary = string.Format("--{0}", DateTime.Now.Ticks.ToString("x"));
        httpContent.Headers.Add("ContentType", $"multipart/form-data, boundary={boundary}");
        int index = -1;
        foreach (FileInfo file in files)
        {
            index++;
            using var stream = file.OpenRead();
            httpContent.Add(new StreamContent(stream, (int)stream.Length), $"file_{index}", file.Name);
        }

        return httpContent;
    }
}

/// <summary>
/// 额外HttpUtil的拓展
/// </summary>
public static partial class HttpClientExtensionTwo
{
    /// <summary>
    /// 创建初始化快捷请求信息HttpRequest
    /// </summary>
    /// <param name="services"></param>
    /// <param name="baseUri">基础请求路由</param>
    /// <returns></returns>
    public static IServiceCollection AddHttpClientRequestInit(this IServiceCollection services, Uri? baseUri)
    {
        HttpUtil.InitRequest(baseUri);
        return services;
    }

    /// <summary>
    /// 创建初始化快捷请求信息HttpRequest
    /// </summary>
    /// <param name="services"></param>
    /// <param name="baseUri">基础请求路由</param>
    /// <returns></returns>
    public static IServiceCollection AddHttpClientRequestInit(this IServiceCollection services, string baseUri)
    {
        HttpUtil.InitRequest(baseUri == null ? null : new(baseUri));
        return services;
    }

    /// <summary>
    /// 返回格式化类型的值
    /// </summary>
    /// <param name="response">请求响应体</param>
    /// <returns></returns>
    public static async Task<ApiResult?> ReadAsFormattingAsync(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) return null;
        var str = await response.Content.ReadAsStringAsync();
        var result = str.JsonToDeserialize<ApiResult>();
        return result;
    }

    /// <summary>
    /// 返回格式化类型的值
    /// </summary>
    /// <typeparam name="T">返回规范值Data的类型</typeparam>
    /// <param name="response">请求响应体</param>
    /// <param name="context">如果是AOT的情况下手动提供Json序列化上下文</param>
    /// <returns></returns>
    public static async Task<ApiResult<T>?> ReadAsFormattingAsync<T>(this HttpResponseMessage response,
        JsonSerializerContext? context = null) where T :
        class
    {
        if (!response.IsSuccessStatusCode) return null;
        var str = await response.Content.ReadAsStringAsync();
        var result = str.JsonToDeserialize<ApiResult<T>>(context: context);
        return result;
    }

    /// <summary>
    /// 设置用户的登录凭证
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="scheme"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static HttpClient SetAuthorization(this HttpClient httpClient, string scheme = "Bearer",
        string? parameter = null)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(parameter) ? new(scheme) : new(scheme, parameter);
        return httpClient;
    }

    /// <summary>
    /// 设置用户的登录凭证
    /// </summary>
    /// <param name="httpClientService"></param>
    /// <param name="scheme"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public static HttpClientService SetAuthorization(this HttpClientService httpClientService, string scheme = "Bearer",
        string? parameter = null)
    {
        httpClientService.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(parameter) ? new(scheme) : new(scheme, parameter);
        return httpClientService;
    }
}