﻿using MFToolkit.Authentication.JwtAuthentication.Configuration;
using MFToolkit.Authentication.JwtAuthentication.Handler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Authentication.JwtAuthentication.Extensions;
public static class AuthenticationExtension
{
    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization(this IServiceCollection service, Action<JsonWebTokenConfig> configAction)
    {
        JsonWebTokenConfig config = new();
        configAction.Invoke(config);
        JsonWebTokenConfig.SetJsonWebTokenConfig(config);
        service.AddAuthentication().AddJwtBearer();
        service.AddAuthorization(options =>
        {
            options.AddPolicy("jwt", policy =>
            {
                policy.Requirements.Add(new JwtAuthenticationHandler());
            });
        });
        return service;
    }

    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization<THandler>(this IServiceCollection service, Action<JsonWebTokenConfig> configAction) where THandler : JwtAuthenticationHandler, new()
    {

        THandler handler = new THandler();
        if (handler is not IAuthorizationRequirement) return service;
        service.AddSingleton<IAuthorizationHandler>(handler);
        JsonWebTokenConfig config = new();
        configAction.Invoke(config);
        JsonWebTokenConfig.SetJsonWebTokenConfig(config);
        service.AddAuthentication().AddJwtBearer();
        service.AddAuthorization(options =>
        {
            options.AddPolicy("jwt", policy =>
            {
                policy.Requirements.Add(handler);
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
        service.AddAuthentication().AddJwtBearer();
        service.AddAuthorization(options =>
        {
            options.AddPolicy("jwt", policy =>
            {
                policy.Requirements.Add(new JwtAuthenticationHandler());
            });
        });
        return service;
    }

    /// <summary>
    /// 注册JWT配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthorization<THandler>(this IServiceCollection service, Dictionary<string, JsonWebTokenConfig> configs) where THandler : JwtAuthenticationHandler, new()
    {
        THandler handler = new();
        if (handler is not IAuthorizationRequirement r) return service;
        JsonWebTokenConfig.SetJsonWebTokenConfig(configs);
        service.AddAuthentication().AddJwtBearer();
        service.AddAuthorization(options =>
        {
            options.AddPolicy("jwt", policy =>
            {
                policy.Requirements.Add(handler);
            });
        });
        return service;
    }

}
