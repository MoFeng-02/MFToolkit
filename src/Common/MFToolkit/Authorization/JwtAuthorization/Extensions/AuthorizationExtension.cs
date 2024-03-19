using MFToolkit.Authorization.JwtAuthorization.Configuration;
using MFToolkit.Authorization.JwtAuthorization.Handler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Authorization.JwtAuthorization.Extensions;
public static class AuthorizationExtension
{
    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization(this IServiceCollection service, JsonWebTokenConfig config)
    {
        JsonWebTokenConfig.SetJsonWebTokenConfig(config);
        service.AddAuthorization(options =>
        {
            options.AddPolicy("jwt", policy =>
            {
                policy.Requirements.Add(new JwtAuthorizationHandler());
            });
        });
        return service;
    }

    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization<THandler>(this IServiceCollection service, JsonWebTokenConfig config) where THandler : class, new()
    {

        THandler handler = new();
        if (handler is not IAuthorizationRequirement r) return service;
        JsonWebTokenConfig.SetJsonWebTokenConfig(config);
        service.AddAuthorization(options =>
        {
            options.AddPolicy("jwt", policy =>
            {
                policy.Requirements.Add(r);
            });
        });
        return service;
    }

    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization(this IServiceCollection service, Dictionary<string, JsonWebTokenConfig> configs)
    {
        JsonWebTokenConfig.SetJsonWebTokenConfig(configs);
        service.AddAuthorization(options =>
        {
            options.AddPolicy("jwt", policy =>
            {
                policy.Requirements.Add(new JwtAuthorizationHandler());
            });
        });
        return service;
    }

    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization<THandler>(this IServiceCollection service, Dictionary<string, JsonWebTokenConfig> configs) where THandler : class, new()
    {
        THandler handler = new();
        if (handler is not IAuthorizationRequirement r) return service;
        JsonWebTokenConfig.SetJsonWebTokenConfig(configs);
        service.AddAuthorization(options =>
        {
            options.AddPolicy("jwt", policy =>
            {
                THandler handler = new THandler();
                policy.Requirements.Add(r);
            });
        });
        return service;
    }

}
