using System.Net.Http.Headers;
using MFToolkit.App;
using MFToolkit.Download.Inject;
using MFToolkit.Http;
using MFToolkit.Http.HttpClientFactorys;
using MFToolkit.Json.Extensions;
using MFToolkit.Loggers.LoggerExtensions;
using MFToolkit.Loggers.LoggerExtensions.Configurations;
using MFToolkit.Loggers.LoggerExtensions.Tasks;
using MFToolkit.Socket.SignalR.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MFToolkit.Injects;

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
    /// <param name="signalRConfiguration">SignalR请求基本信息</param>
    /// <param name="loggerOptions">日志配置</param>
    /// <param name="serviceOptions">额外自己要注入的配置</param>
    /// <returns></returns>
    public static IServiceCollection InjectServices(HttpRequestConfiguration? httpRequestConfiguration = null,
        SignalRConfiguration? signalRConfiguration = null,
        Action<LoggerConfiguration>? loggerOptions = null,
        Action<IServiceCollection> serviceOptions = null!)
    {
        return new ServiceCollection().AddInjectServices(httpRequestConfiguration, signalRConfiguration, loggerOptions,
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
    /// <param name="signalRConfiguration">SignalR请求基本信息</param>
    /// <param name="loggerOptions">日志配置</param>
    /// <param name="serviceOptions">额外自己要注入的配置</param>
    /// <returns></returns>
    public static IServiceCollection AddInjectServices(this IServiceCollection services,
        HttpRequestConfiguration? httpRequestConfiguration = null,
        SignalRConfiguration? signalRConfiguration = null,
        Action<LoggerConfiguration>? loggerOptions = null,
        Action<IServiceCollection> serviceOptions = null!)
    {
        if (_isInjectExist) return services;
        // 如果配置的不为Null，则返回本身
        if (AppUtil.ServiceCollection != null)
        {
            _isInjectExist = true;
            return services;
        }

        _isInjectExist = true;
        services ??= new ServiceCollection();
        // 设置默认的Json转义配置
        services.SetDefaultJsonSerializerOptions(new()
        {
            // 设置Json转义的时候忽略大小写
            PropertyNameCaseInsensitive = true,
        });
        services.AddLogging(logging =>
        {
            logging.AddLocalFileLogger(loggerOptions ??= (options) =>
            {
                options.AddStartLogLevel(LogLevel.Information);
                options.AddStartLogLevel(LogLevel.Error);
                options.AddStartLogLevel(LogLevel.Warning);

                options.BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                options.OpenGroupLevel = true;
                options.SaveTimeType = SaveTimeType.Hour;
                Task.Run(() => LogClear.ClearLogs(options));
            });
        });

        #region HTTP Clent

        services.AddHttpClient<HttpClientService>(options =>
        {
            options.BaseAddress = httpRequestConfiguration == null
                ? null
                : new(httpRequestConfiguration.BaseRequestUri);
            options.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", httpRequestConfiguration?.RequestTokenFunc());
        })
            // 暂时先注释
            // #if DEBUG
            .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; }
            })
            // #endif
            ;
        services.AddSingleton<HttpClientFactoryService>();

        #endregion

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

        #region SignalR

        if (signalRConfiguration != null)
        {
            ChatHubUtil.ConnectionBuild(signalRConfiguration.BaseRequestUri, signalRConfiguration.RequestTokenFunc);
        }

        #endregion

        serviceOptions?.Invoke(services);

        services.AddDownloadService().AddDownloadPauseInfoHandler();
        //services.AddDownloadService<DownloadHandler>();

        services.AddAppUtilService();
        return services;
    }
}

public class HttpRequestConfiguration
{
    public HttpRequestConfiguration()
    {
    }

    public HttpRequestConfiguration(string requestUri)
    {
        BaseRequestUri = requestUri;
    }

    public string BaseRequestUri { get; set; }
    public Func<string> RequestTokenFunc { get; set; }
}

public class SignalRConfiguration
{
    /// <summary>
    /// 请求地址
    /// </summary>
    public string BaseRequestUri { get; set; } = null!;

    /// <summary>
    /// 获取token的方法，如果需要token的话就传这个
    /// </summary>
    public Func<string?> RequestTokenFunc { get; set; } = null!;
}