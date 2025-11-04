namespace MFToolkit.Minecraft.Constants;

/// <summary>
/// OAuth权限范围常量
/// </summary>
public static class OAuthScopes
{
    /// <summary>
    /// Microsoft Xbox Live基本范围
    /// </summary>
    public const string XboxLiveSignIn = "XboxLive.signin";
    
    /// <summary>
    /// Microsoft Xbox Live离线访问范围
    /// </summary>
    public const string XboxLiveOfflineAccess = "XboxLive.offline_access";
    
    /// <summary>
    /// Minecraft服务范围
    /// </summary>
    public const string Minecraft = "https://api.minecraftservices.com/auth/minecraft";
    
    /// <summary>
    /// 用于Microsoft OAuth的完整范围列表
    /// </summary>
    public static readonly string[] MicrosoftScopes = { XboxLiveSignIn, XboxLiveOfflineAccess, Minecraft };
}
