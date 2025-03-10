using Microsoft.AspNetCore.Http;

namespace MFToolkit.AspNetCore.App;
/// <summary>
/// APP 拓展通用工具类
/// </summary>
public class MFApp : MFToolkit.App.MFApp
{
    /// <summary>
    /// 获取当前HttpContext，如果是后台服务则为空
    /// </summary>
    public static HttpContext? HttpContext { get; private set; } = GetService<HttpContext>();
}
