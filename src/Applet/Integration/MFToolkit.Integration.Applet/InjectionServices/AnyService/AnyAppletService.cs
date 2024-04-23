using MFToolkit.Integration.Applet.InjectionServices.WeChat;
using MFToolkit.WeChat.Configurations.BasicConfiguration;
using MFToolkit.WeChat.Inject;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Integration.Applet.InjectionServices.AnyService;
public static class AnyAppletService
{
    /// <summary>
    /// 注入所有小程序
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddAnyApplet(this IServiceCollection service, WeChatConfig? weChatConfig = null, Dictionary<string, WeChatConfig>? weChatConfigs = null)
    {
        if (weChatConfig != null) service.AddWeChatApplet(weChatConfig);
        if (weChatConfigs != null) service.AddWeChatApplet(weChatConfigs);
        return service;
    }
    /// <summary>
    /// 注册所有小程序的相关请求服务
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddAnyAppletRequestService(this IServiceCollection service)
    {
        service.AddWeChatRequestService();
        return service;
    }
}
