using MFToolkit.WeChat.Services;
using MFToolkit.WeChat.Utils;
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
    /// <summary>
    /// 注册WeChat配置持久化，不提供删除，删除需要自行去处理
    /// </summary>
    /// <param name="service"></param>
    /// <param name="readPath">读取路径，只能是文件夹</param>
    /// <param name="encryptionKey">内容加密Key，如果是有加密的话</param>
    /// <returns></returns>
    public static IServiceCollection AddWeChatConfigurationLasting(this IServiceCollection service, string? readPath = null, string? encryptionKey = null)
    {
        Task.Run(async () => await WeChatUtil.WeChatConfigurationLasting(readPath, encryptionKey));
        return service;
    }
}
