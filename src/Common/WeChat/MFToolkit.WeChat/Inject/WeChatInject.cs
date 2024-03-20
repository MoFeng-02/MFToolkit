using MFToolkit.WeChat.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.WeChat.Inject;
public static class WeChatInject
{
    /// <summary>
    /// 注入WeChat服务
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public static IServiceCollection AddWeChatService(this IServiceCollection service)
    {
        service.AddTransient<IWeChatService, WeChatService>();
        return service;
    }
}
