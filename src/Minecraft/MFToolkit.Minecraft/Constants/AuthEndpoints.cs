using System;

namespace MFToolkit.Minecraft.Constants;

/// <summary>
/// Minecraft认证相关的API端点常量
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Microsoft OAuth 2.0授权端点
    /// </summary>
    public const string MicrosoftAuthorization = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";

    /// <summary>
    /// Microsoft OAuth 2.0令牌端点
    /// </summary>
    public const string MicrosoftToken = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";

    /// <summary>
    /// Microsoft OAuth 2.0设备代码端点
    /// </summary>
    public const string MicrosoftDeviceCode = "https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode";

    /// <summary>
    /// Xbox Live身份验证端点
    /// </summary>
    public const string XboxLiveAuthorize = "https://user.auth.xboxlive.com/user/authenticate";

    /// <summary>
    /// Xbox Live服务授权端点
    /// </summary>
    public const string XboxAuthorize = "https://xsts.auth.xboxlive.com/xsts/authorize";

    /// <summary>
    /// Minecraft服务登录端点
    /// </summary>
    public const string MinecraftLogin = "https://api.minecraftservices.com/authentication/login_with_xbox";

    /// <summary>
    /// 获取是否拥有正版游戏
    /// </summary>
    public const string MinecraftHaveGame = "https://api.minecraftservices.com/entitlements/mcstore";

    /// <summary>
    /// Minecraft服务刷新令牌端点
    /// </summary>
    public const string MinecraftRefresh = "https://api.minecraftservices.com/authentication/refresh";

    /// <summary>
    /// Minecraft服务验证令牌端点
    /// </summary>
    public const string MinecraftValidate = "https://api.minecraftservices.com/authentication/validate";

    /// <summary>
    /// Minecraft服务注销端点
    /// </summary>
    public const string MinecraftLogout = "https://api.minecraftservices.com/authentication/logout";

    /// <summary>
    /// Minecraft玩家档案端点
    /// </summary>
    public const string MinecraftProfile = "https://api.minecraftservices.com/minecraft/profile";

    /// <summary>
    /// Minecraft皮肤上传端点
    /// </summary>
    public const string MinecraftSkinUpload = "https://api.minecraftservices.com/minecraft/profile/skins";

    /// <summary>
    /// Minecraft披风上传端点
    /// </summary>
    public const string MinecraftCapeUpload = "https://api.minecraftservices.com/minecraft/profile/capes";

    /// <summary>
    /// Minecraft名称可用性检查端点
    /// </summary>
    public const string MinecraftNameAvailability = "https://api.minecraftservices.com/minecraft/profile/name/{0}/available";

    /// <summary>
    /// Minecraft名称更改端点
    /// </summary>
    public const string MinecraftNameChange = "https://api.minecraftservices.com/minecraft/profile/name/{0}";
}
