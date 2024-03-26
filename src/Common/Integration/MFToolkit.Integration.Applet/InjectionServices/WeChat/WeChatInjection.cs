using MFToolkit.WeChat.Configurations;
using MFToolkit.WeChat.Configurations.BasicConfiguration;
using MFToolkit.WeChat.Inject;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Integration.Applet.InjectionServices.WeChat;
/// <summary>
/// 微信开发注入
/// </summary>
public static class WeChatInjection
{
    /// <summary>
    /// 注册微信相关服务
    /// </summary>
    /// <param name="service">程序服务集合</param>
    /// <param name="weChatConfig">微信服务开发配置</param>
    /// <returns></returns>
    public static IServiceCollection AddWeChatApplet(this IServiceCollection service, WeChatConfig weChatConfig)
    {
        WeChatConfigUtil.SetBasicConfiguration(weChatConfig);
        service.AddWeChatService();
        return service;
    }
    /// <summary>
    /// 注册微信相关服务
    /// </summary>
    /// <param name="service">程序服务集合</param>
    /// <param name="weChatConfig">微信服务开发配置，根据key和value模式来提供多种配置</param>
    /// <returns></returns>
    public static IServiceCollection AddWeChatApplet(this IServiceCollection service, Dictionary<string, WeChatConfig> weChatConfig)
    {
        WeChatConfigUtil.SetBasicConfigurations(weChatConfig);
        service.AddWeChatService();
        return service;
    }
}
