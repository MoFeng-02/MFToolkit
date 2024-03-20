namespace MFToolkit.WeChat.Services;
public interface IWeChatService
{
    /// <summary>
    /// 请求获取微信OpenID等信息
    /// </summary>
    /// <param name="code"></param>
    /// <param name="appid"></param>
    /// <param name="secret"></param>
    /// <param name="configKey">配置key</param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetWeChatOpenIdAsync(string code, string appid = null, string secret = null, string? configKey = null);
    /// <summary>
    /// 获取AccessToken -- token有效期两小时 -- 每天请求：最大 2000 次
    /// </summary>
    /// <param name="appid">测试为：wx195d8fedc547e181</param>
    /// <param name="secret">测试为：e98577ad04d5fa9805e0d07cab4907a1</param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetWeChatAccessTokenAsync(string appid = null, string secret = null, string? configKey = null);
}
