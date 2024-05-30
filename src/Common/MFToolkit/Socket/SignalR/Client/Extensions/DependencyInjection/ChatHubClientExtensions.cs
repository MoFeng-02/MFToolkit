using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.Socket.SignalR.Client.Extensions.DependencyInjection;
public static class ChatHubClientExtensions
{
    /// <summary>
    /// 注册SignalR ChatHub聊天客户端
    /// </summary>
    /// <param name="services"></param>
    /// <param name="url"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static IServiceCollection AddMFChatHubClientBuilder(this IServiceCollection services, string url, Func<string> token)
    {
        ChatHubClient.ConnectionBuild(url, token);
        return services;
    }
}
