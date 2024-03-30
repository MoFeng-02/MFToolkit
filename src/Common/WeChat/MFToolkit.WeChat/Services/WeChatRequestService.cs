using MFToolkit.Http;
using MFToolkit.WeChat.Configurations;
using MFToolkit.WeChat.Configurations.BasicConfiguration;

namespace MFToolkit.WeChat.Services;
public class WeChatRequestService(HttpClientService httpClientService) : IWeChatRequestService
{
    /// <summary>
    /// 获取微信OpenId
    /// </summary>
    /// <param name="code">前端提供过来的临时code</param>
    /// <param name="appid">AppId</param>
    /// <param name="secret">AppSecret</param>
    /// <param name="configKey">配置key</param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> GetWeChatOpenIdAsync(string code, string appid = null, string secret = null, string? configKey = null)
    {
        WeChatConfig config = WeChatConfigUtil.GetBasicConfiguration(configKey);
        appid ??= config.AppId;
        secret ??= config.AppSecret;
        string url_path = $"https://api.weixin.qq.com/sns/jscode2session?appid={appid}&secret={secret}&js_code={code}&grant_type=authorization_code";
        return await httpClientService.GetAsync(url_path);
    }
    /// <summary>
    /// 获取微信OpenId
    /// </summary>
    /// <param name="appid">AppId</param>
    /// <param name="secret">AppSecret</param>
    /// <param name="configKey">配置key</param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> GetWeChatAccessTokenAsync(string appid = null, string secret = null, string? configKey = null)
    {
        WeChatConfig config = WeChatConfigUtil.GetBasicConfiguration(configKey);
        appid ??= config.AppId;
        secret ??= config.AppSecret;
        string url_path = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appid}&secret={secret}";
        return await httpClientService.GetAsync(url_path);
    }
}
