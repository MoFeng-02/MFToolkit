using System.Text.Json.Serialization;
using MFToolkit.Minecraft.Converters;
using MFToolkit.Minecraft.Entities.Account;
using MFToolkit.Minecraft.Entities.Account.Http;
using MFToolkit.Minecraft.Entities.Cape;
using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Entities.Profile;
using MFToolkit.Minecraft.Entities.Skin;
using MFToolkit.Minecraft.Entities.Versions;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Options;

namespace MFToolkit.Minecraft.JsonExtensions;

/// <summary>
/// JSON序列化上下文，用于AOT编译支持
/// </summary>
[JsonSourceGenerationOptions(Converters = [
    typeof(SafeStringEnumConverter<VersionType>),
    typeof(SafeStringEnumConverter<StorageMode>),
    typeof(SafeStringEnumConverter<RuleAction>),
    typeof(ArgumentListConverter),
    typeof(ArgumentValueConverter),
    ],
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(VersionManifest))]
[JsonSerializable(typeof(LatestVersionInfo))]
[JsonSerializable(typeof(VersionInfo))]
[JsonSerializable(typeof(VersionType))]
[JsonSerializable(typeof(List<VersionInfo>))]
[JsonSerializable(typeof(IReadOnlyList<VersionInfo>))]
[JsonSerializable(typeof(GameVersionDetail))]
[JsonSerializable(typeof(VersionInfoDetail))]
[JsonSerializable(typeof(Downloads))]
[JsonSerializable(typeof(DownloadItem))]
[JsonSerializable(typeof(Library))]
[JsonSerializable(typeof(LibraryDownloads))]
[JsonSerializable(typeof(ExtractRules))]
[JsonSerializable(typeof(AssetIndex))]
[JsonSerializable(typeof(JavaVersionInfo))]
[JsonSerializable(typeof(Arguments))]
[JsonSerializable(typeof(RuleBasedArgument))]
[JsonSerializable(typeof(List<RuleBasedArgument>))]
[JsonSerializable(typeof(Rule))]
[JsonSerializable(typeof(Os))]
[JsonSerializable(typeof(RuleAction))]
[JsonSerializable(typeof(LoggingConfig))]
[JsonSerializable(typeof(LoggingEntry))]
[JsonSerializable(typeof(LoggingFile))]
[JsonSerializable(typeof(AssetIndexContent))] // 用于资源索引JSON
[JsonSerializable(typeof(Dictionary<string, AssetObject>))] // 资源索引中的objects对象

// Auth Account相关类型

[JsonSerializable(typeof(MinecraftAccount))]
[JsonSerializable(typeof(MicrosoftAuthInfo))]
[JsonSerializable(typeof(XboxAuthInfo))]
[JsonSerializable(typeof(MojangAuthInfo))]
[JsonSerializable(typeof(Session))]
[JsonSerializable(typeof(ProfileProperty))]
[JsonSerializable(typeof(SkinInfo))]
[JsonSerializable(typeof(SkinUploadRequest))]
[JsonSerializable(typeof(CapeInfo))]
[JsonSerializable(typeof(PlayerProfile))]
[JsonSerializable(typeof(XboxDisplayClaims))]
[JsonSerializable(typeof(XboxUserInfo))]
// Auth Request / Response相关类型
[JsonSerializable(typeof(MicrosoftTokenResponse))]
[JsonSerializable(typeof(MicrosoftDeviceCodeResponse))]
[JsonSerializable(typeof(MicrosoftTokenErrorResponse))]
[JsonSerializable(typeof(MojangAuthRequest))]
[JsonSerializable(typeof(MojangAuthRefreshRequest))]
[JsonSerializable(typeof(MojangTokenResponse))]
[JsonSerializable(typeof(MojangLogingOut))]
[JsonSerializable(typeof(XboxLiveTokenResponse))]
[JsonSerializable(typeof(XboxServiceTokenResponse))]
[JsonSerializable(typeof(XboxDisplayClaimsResponse))]
[JsonSerializable(typeof(XboxUserInfoResponse))]
[JsonSerializable(typeof(XboxAuthToken))]
[JsonSerializable(typeof(XboxUserClaim))]
[JsonSerializable(typeof(XboxAuthRequest))]
[JsonSerializable(typeof(XboxXstsAuthRequest))]
[JsonSerializable(typeof(XboxAuthProperties))]
[JsonSerializable(typeof(XboxXstsAuthProperties))]

// 基础类型支持
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]

#region Options
[JsonSerializable(typeof(StorageOptions))]
[JsonSerializable(typeof(StorageMode))]
[JsonSerializable(typeof(LauncherOptions))]
[JsonSerializable(typeof(DownloadOptions))]
#endregion
public partial class MinecraftJsonSerializerContext : JsonSerializerContext
{ }
