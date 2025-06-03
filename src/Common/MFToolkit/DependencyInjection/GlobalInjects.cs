using MFToolkit.App;
using MFToolkit.App.Extensions.DependencyInjection;
using MFToolkit.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.DependencyInjection;

/// <summary>
/// 注入
/// </summary>
public static class GlobalInjects
{
    /// <summary>
    /// 是否已经注入过了
    /// </summary>
    private static bool _isInjectExist;

    /// <summary>
    /// 注入Service，建议在没用类似于下面代码的时候调用，例如
    /// <para>
    /// 不存在这样调用的时候
    /// <code>
    /// var builder = MauiApp.CreateBuilder();
    /// builder.Services.InjectServices();
    /// </code>
    /// 应该直接这样调用
    /// <code>GlobalInjects.InjectServices();</code>
    /// </para>
    /// </summary>
    /// <param name="httpRequestConfiguration">HttpClient 请求基本地址</param>
    /// <param name="serviceOptions">额外自己要注入的配置</param>
    /// <returns></returns>
    public static IServiceCollection InjectServices(HttpRequestConfiguration? httpRequestConfiguration = null,
        Action<IServiceCollection> serviceOptions = null!)
    {
        return new ServiceCollection().AddInjectServices(httpRequestConfiguration,
            serviceOptions);
    }

    /// <summary>
    /// 注入Service，建议在builder.Services的时候调用，例如
    /// <para>
    /// <code>
    /// var builder = MauiApp.CreateBuilder();
    /// builder.Services.InjectServices();
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="httpRequestConfiguration">HttpClient 请求基本地址</param>
    /// <param name="serviceOptions">额外自己要注入的配置</param>
    /// <returns></returns>
    public static IServiceCollection AddInjectServices(this IServiceCollection services,
        HttpRequestConfiguration? httpRequestConfiguration = null,
        Action<IServiceCollection> serviceOptions = null!)
    {
        if (_isInjectExist) return services;
        // 如果配置的不为Null，则返回本身
        if (MFApp.ServiceCollection != null)
        {
            _isInjectExist = true;
            return services;
        }

        _isInjectExist = true;
        services ??= new ServiceCollection();

        //#region HTTP Clent

        //services.AddHttpClientService(options =>
        //{
        //    options.BaseAddress = httpRequestConfiguration == null || !Validator.IsValidUrl(httpRequestConfiguration.BaseRequestUri)
        //        ? null
        //        : new(httpRequestConfiguration.BaseRequestUri!);
        //    if (httpRequestConfiguration?.RequestTokenFunc != null)
        //        options.DefaultRequestHeaders.Authorization =
        //            new AuthenticationHeaderValue("Bearer", httpRequestConfiguration.RequestTokenFunc());
        //})
        //    // 暂时先注释
        //    // #if DEBUG
        //    .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler()
        //    {
        //        ServerCertificateCustomValidationCallback = delegate { return true; }
        //    })
        //    // #endif
        //    ;
        ////services.AddSingleton<HttpClientFactoryService>();

        //#endregion

        #region GRPC 配置

        //builder.Services.AddGrpcClient<ChatHub.ChatHubClient>("Chat", o =>
        //{
        //    o.Address = new Uri(toHttp);
        //}).AddCallCredentials((context, metadata) =>
        //{
        //    var user = UserUtil.GetUserTokenInfo();
        //    if (user != null && !string.IsNullOrEmpty(user.AccessToken))
        //    {
        //        metadata.Add("Authorization", $"Bearer {user.AccessToken}");
        //    }
        //    return Task.CompletedTask;
        //});

        #endregion

        #region 注入配置 额外配置

        if (!string.IsNullOrEmpty(httpRequestConfiguration?.BaseRequestUri))
            services.AddHttpClientRequestInit(httpRequestConfiguration.BaseRequestUri);

        #endregion

        serviceOptions?.Invoke(services);
        // 取消内部注册，由用户自行注册
        //services.AddDownloadService().AddDownloadPauseInfoHandler();
        //services.AddDownloadService<DownloadHandler>();

        services.AddInjectMFAppService();
        return services;
    }

    /// <summary>
    /// 注入MFApp的ServiceProvider，建议在所有服务注册完成后调用
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceProvider MFAppServiceProvider(this IServiceProvider services)
    {
        MFApp.ServiceProvider = services;
        return services;
    }
}

/// <summary>
/// HttpClient 请求配置
/// </summary>
public class HttpRequestConfiguration
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public HttpRequestConfiguration()
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="requestUri"></param>
    public HttpRequestConfiguration(string requestUri)
    {
        BaseRequestUri = requestUri;
    }

    /// <summary>
    /// 基础请求地址
    /// </summary>
    public string? BaseRequestUri { get; set; }

    /// <summary>
    /// 请求Token委托
    /// </summary>
    public Func<string>? RequestTokenFunc { get; set; }
}
