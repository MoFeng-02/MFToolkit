using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Configuration;
using MFToolkit.AspNetCore.Authentication.JwtAuthorization.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.AspNetCore.Authentication.JwtAuthorization.Extensions;

/// <summary>
/// 授权拓展依赖注入
/// </summary>
public static class AuthenticationExtension
{
    private const string DefaultPolicyName = "jwt";
    private static string? _useCorsPolicyName;
    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <param name="configAction"></param>
    /// <param name="policyName">策略名称，默认jwt</param>
    /// <param name="corsOptions">跨域配置</param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization(this IServiceCollection service, Action<JsonWebTokenConfig> configAction, string policyName = DefaultPolicyName, Action<CorsOptions>? corsOptions = null)
    {
        JsonWebTokenConfig config = new();
        configAction.Invoke(config);
        JsonWebTokenConfig.SetJsonWebTokenConfig(config);
        service.AddAuthentication().AddJwtBearer();
        service.AddAuthorizationBuilder()
            .AddPolicy(policyName, policy =>
            {
                policy.Requirements.Add(new JwtAuthorizationHandler());
            });
        if (corsOptions != null)
        {
            service.AddCors(corsOptions);
            _useCorsPolicyName = policyName;
        }
        return service;
    }

    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <param name="configAction"></param>
    /// <param name="policyName">策略名称，默认jwt</param>
    /// <param name="corsOptions">跨域配置</param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization<THandler>(this IServiceCollection service, Action<JsonWebTokenConfig> configAction, string policyName = DefaultPolicyName, Action<CorsOptions>? corsOptions = null) where THandler : JwtAuthorizationHandler, new()
    {

        THandler handler = new THandler();
        if (handler is not IAuthorizationRequirement) return service;
        service.AddAuthentication().AddJwtBearer();
        //service.AddSingleton<IAuthorizationHandler>(handler);
        JsonWebTokenConfig config = new();
        configAction.Invoke(config);
        JsonWebTokenConfig.SetJsonWebTokenConfig(config);
        service.AddAuthorizationBuilder()
            .AddPolicy(policyName, policy =>
            {
                policy.Requirements.Add(handler);
            });

        if (corsOptions != null)
        {
            service.AddCors(corsOptions);

            _useCorsPolicyName = policyName;
        }
        return service;
    }

    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <param name="configs">JWT密钥字典集</param>
    /// <param name="policyName">策略名称，默认jwt</param>
    /// <param name="corsOptions">跨域配置</param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization(this IServiceCollection service, Dictionary<string, JsonWebTokenConfig> configs, string policyName = DefaultPolicyName, Action<CorsOptions>? corsOptions = null)
    {
        JsonWebTokenConfig.SetJsonWebTokenConfig(configs);
        service.AddAuthentication().AddJwtBearer();
        service.AddAuthorizationBuilder()
            .AddPolicy(policyName, policy =>
            {
                policy.Requirements.Add(new JwtAuthorizationHandler());
            });
        if (corsOptions != null)
        {
            service.AddCors(corsOptions);
            _useCorsPolicyName = policyName;
        }
        return service;
    }

    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <param name="configs">JWT密钥字典集</param>
    /// <param name="policyName">策略名称，默认jwt</param>
    /// <param name="corsOptions">跨域配置</param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization<THandler>(this IServiceCollection service, Dictionary<string, JsonWebTokenConfig> configs, string policyName = DefaultPolicyName, Action<CorsOptions>? corsOptions = null) where THandler : JwtAuthorizationHandler, new()
    {
        THandler handler = new();
        if (handler is not IAuthorizationRequirement r) return service;
        JsonWebTokenConfig.SetJsonWebTokenConfig(configs);
        service.AddAuthentication().AddJwtBearer();
        service.AddAuthorizationBuilder()
            .AddPolicy(policyName, policy =>
            {
                policy.Requirements.Add(handler);
            });

        if (corsOptions != null)
        {
            service.AddCors(corsOptions);
            _useCorsPolicyName = policyName;
        }
        return service;
    }
    /// <summary>
    /// 注入Jwt的跨域Cors
    /// <para>参考：https://learn.microsoft.com/zh-cn/aspnet/core/security/cors?view=aspnetcore-8.0#cors-with-named-policy-and-middleware</para>
    /// <para>如果是最小API，则在Map前面注入，如果是其他则参考上面链接</para>
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseJwtAuthorizationCors(this WebApplication app)
    {
        if (_useCorsPolicyName == null) return app;
        app.UseCors(_useCorsPolicyName);
        return app;
    }

    /// <summary>
    /// 注入Jwt的跨域Cors
    /// <para>参考：https://learn.microsoft.com/zh-cn/aspnet/core/security/cors?view=aspnetcore-8.0#cors-with-named-policy-and-middleware</para>
    /// <para>如果是最小API，则在Map前面注入，如果是其他则参考上面链接</para>
    /// </summary>
    /// <param name="app"></param>
    /// <param name="configurePolicy"></param>
    /// <returns></returns>
    public static WebApplication UseJwtAuthorizationCorss(this WebApplication app, Action<CorsPolicyBuilder> configurePolicy)
    {
        ArgumentNullException.ThrowIfNull(configurePolicy);
        app.UseCors(configurePolicy);
        return app;
    }

}
