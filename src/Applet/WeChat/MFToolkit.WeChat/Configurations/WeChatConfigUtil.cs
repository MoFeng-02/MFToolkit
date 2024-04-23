using MFToolkit.WeChat.Configurations.BasicConfiguration;

namespace MFToolkit.WeChat.Configurations;
/// <summary>
/// 微信开发配置工具
/// </summary>
public class WeChatConfigUtil
{
    private static WeChatConfig? basicConfig;
    private static Dictionary<string, WeChatConfig>? basicConfigs;

    internal static Dictionary<string, WeChatConfig>? GetBasicConfigs() => basicConfigs;
    #region 基本配置
    /// <summary>
    /// 设置基本的参数
    /// <para>如果有多个则使用<see cref="SetBasicConfigurations"/></para>
    /// </summary>
    /// <param name="appid"></param>
    /// <param name="appsecret"></param>
    public static void SetBasicConfiguration(WeChatConfig config)
    {
        if (basicConfig != null) return;
        basicConfig = config;
    }
    /// <summary>
    /// 设置多个基本参数
    /// </summary>
    /// <param name="kvs">给定识别键然后赋值</param>
    public static void SetBasicConfigurations(Dictionary<string, WeChatConfig> kvs)
    {
        if (basicConfigs != null) return;
        basicConfigs = kvs;
    }
    /// <summary>
    /// 获取基本配置
    /// </summary>
    /// <param name="key">如果给key则代表是在多基本参数里面查找配置</param>
    /// <returns></returns>
    public static WeChatConfig GetBasicConfiguration(string? key = null)
    {
        try
        {
            return (key == null ? basicConfig : basicConfigs == null ? null! : basicConfigs[key]) ?? null!;
        }
        catch (Exception)
        {
            return null!;
        }
    }
    #endregion
}
