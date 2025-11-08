using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Options;
using MFToolkit.Minecraft.Services.Auth;
using MFToolkit.Minecraft.Services.Auth.Interfaces;
using MFToolkit.Minecraft.Services.Cape;
using MFToolkit.Minecraft.Services.Downloads;
using MFToolkit.Minecraft.Services.Downloads.Interfaces;
using MFToolkit.Minecraft.Services.Profile;
using MFToolkit.Minecraft.Services.Skin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MFToolkit.Minecraft.Extensions.DependencyInjection;

/// <summary>
/// 服务集合扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加Minecraft服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMinecraftServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // 添加配置选项
        services.AddOptions<OfficialAuthOptions>();
        //.BindConfiguration(OfficialAuthOptions.OfficialAuth)
        //.ValidateOnStart();

        services.AddOptions<OfflineAuthOptions>();
        //.BindConfiguration(OfflineAuthOptions.OfflineAuth)
        //.ValidateOnStart();

        services.AddOptions<SkinOptions>();
        //.BindConfiguration(SkinOptions.Skin)
        //.ValidateOnStart();

        // 添加HTTP客户端
        services.AddHttpClient<IOfficialAuthService, OfficialAuthService>();
        services.AddHttpClient<IOfflineAuthService, OfflineAuthService>();
        services.AddHttpClient<IThirdPartyAuthService, ThirdPartyAuthService>();
        services.AddHttpClient<IProfileService, ProfileService>();
        services.AddHttpClient<ISkinService, SkinService>();
        services.AddHttpClient<ICapeService, CapeService>();
        services.AddHttpClient<IMinecraftVersionService, MinecraftVersionService>();
        services.AddHttpClient<IMinecraftDownloadService, MinecraftDownloadService>();

        // 添加认证服务
        services.TryAddScoped<IOfficialAuthService, OfficialAuthService>();
        services.TryAddScoped<IOfflineAuthService, OfflineAuthService>();
        services.TryAddScoped<IThirdPartyAuthService, ThirdPartyAuthService>();
        // 添加其他服务
        services.TryAddScoped<IProfileService, ProfileService>();
        services.TryAddScoped<ISkinService, SkinService>();
        services.TryAddScoped<ICapeService, CapeService>();

        // 添加通用认证服务（根据账号类型自动选择合适的认证服务）
        services.TryAddScoped<Func<AccountType, IAuthService>>(serviceProvider => accountType =>
        {
            return accountType switch
            {
                AccountType.Microsoft or AccountType.Xbox or AccountType.Mojang => serviceProvider.GetRequiredService<IOfficialAuthService>(),
                AccountType.Offline => serviceProvider.GetRequiredService<IOfflineAuthService>(),
                AccountType.ThirdParty => serviceProvider.GetRequiredService<IThirdPartyAuthService>(),
                _ => throw new NotSupportedException($"Account type '{accountType}' is not supported.")
            };
        });

        return services;
    }

    /// <summary>
    /// 添加Minecraft服务并配置官方认证选项
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOfficialAuth">官方认证配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMinecraftServices(this IServiceCollection services, Action<OfficialAuthOptions> configureOfficialAuth)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureOfficialAuth == null)
            throw new ArgumentNullException(nameof(configureOfficialAuth));

        services.AddMinecraftServices();
        services.Configure(configureOfficialAuth);

        return services;
    }

    /// <summary>
    /// 添加Minecraft服务并配置所有选项
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOfficialAuth">官方认证配置</param>
    /// <param name="configureOfflineAuth">离线认证配置</param>
    /// <param name="configureSkin">皮肤配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddMinecraftServices(
        this IServiceCollection services,
        Action<OfficialAuthOptions> configureOfficialAuth,
        Action<OfflineAuthOptions> configureOfflineAuth,
        Action<SkinOptions> configureSkin)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureOfficialAuth == null)
            throw new ArgumentNullException(nameof(configureOfficialAuth));

        if (configureOfflineAuth == null)
            throw new ArgumentNullException(nameof(configureOfflineAuth));

        if (configureSkin == null)
            throw new ArgumentNullException(nameof(configureSkin));

        services.AddMinecraftServices();
        services.Configure(configureOfficialAuth);
        services.Configure(configureOfflineAuth);
        services.Configure(configureSkin);

        return services;
    }
}
