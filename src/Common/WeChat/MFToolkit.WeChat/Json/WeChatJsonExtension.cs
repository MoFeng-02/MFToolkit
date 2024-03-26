using MFToolkit.JsonExtensions;
using MFToolkit.WeChat.Json.AOT;
using MFToolkit.WeChat.Model;

namespace MFToolkit.WeChat.Json;
public static class WeChatJsonExtension
{
    /// <summary>
    /// 将请求的WeChat响应内容读取成该实例
    /// </summary>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static async Task<AccessData?> WeChatResponseJsonToAccessDataAsync(this HttpResponseMessage responseMessage)
    {
        var rstr = await responseMessage.Content.ReadAsStringAsync();
        var accessData = rstr.JsonToDeserialize<AccessData>(context: WeChatConfigJsonContext.Default);
        return accessData;
    }
    /// <summary>
    /// 将请求的WeChat Json 字符串读取成该实例
    /// </summary>
    /// <param name="responseMessage"></param>
    /// <returns></returns>
    public static AccessData? WeChatJsonToAccessData(this string rstr)
    {
        var accessData = rstr.JsonToDeserialize<AccessData>(context: WeChatConfigJsonContext.Default);
        return accessData;
    }
}
