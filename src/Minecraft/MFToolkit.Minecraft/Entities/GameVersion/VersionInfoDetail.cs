using System.Text.Json;
using System.Text.Json.Serialization;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.JsonExtensions;
using MFToolkit.Minecraft.Options;

namespace MFToolkit.Minecraft.Entities.GameVersion;

/// <summary>
/// 存储的版本选项，用于告知存放位置，它用于存放位置的模型，内部也提供了对应的单独的存储配置，恰到好处的用它，能节省很多时间去处理一些路径问题
/// </summary>
public class VersionInfoDetail
{
    /// <summary>
    /// ID
    /// </summary>
    [JsonPropertyName("当前信息唯一ID")]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 版本ID（唯一标识）
    /// </summary>
    [JsonPropertyName("versionId")]
    public required string VersionId { get; set; }

    /// <summary>
    /// 显示名称（用户友好的名称）
    /// </summary>
    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }

    /// <summary>
    /// 基路径
    /// </summary>
    [JsonPropertyName("basePath")]
    public required string BasePath { get; set; }

    /// <summary>
    /// 存放名称（和版本json名称一样，比如1.21.10.jar和1.21.10.json，但是没用后缀）
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// 版本文件存放文件夹
    /// </summary>
    [JsonPropertyName("versionFolder")]
    public required string VersionFolder { get; set; }

    /// <summary>
    /// 库文件存放文件夹
    /// </summary>
    [JsonPropertyName("libraryFolder")]
    public required string LibraryFolder { get; set; }

    /// <summary>
    /// 资源文件存放文件夹
    /// </summary>
    [JsonPropertyName("assetsFolder")]
    public required string AssetsFolder { get; set; }

    /// <summary>
    /// 原生库文件存放文件夹
    /// </summary>
    [JsonPropertyName("nativesFolder")]
    public required string NativesFolder { get; set; }

    /// <summary>
    /// 继承的版本（Forge/Fabric等）
    /// </summary>
    [JsonPropertyName("inheritsFrom")]
    public string? InheritsFrom { get; set; }

    /// <summary>
    /// 版本类型（release, snapshot, old_beta, old_alpha）
    /// </summary>
    [JsonPropertyName("versionType")]
    public VersionType VersionType { get; set; }

    [JsonPropertyName("modLoaderType")] public ModLoaderType ModLoaderType { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    [JsonPropertyName("releaseTime")]
    public DateTimeOffset ReleaseTime { get; set; }

    /// <summary>
    /// Java版本信息
    /// </summary>
    [JsonPropertyName("javaVersion")]
    public JavaVersionInfo? JavaVersion { get; set; }

    /// <summary>
    /// 主类名
    /// </summary>
    [JsonPropertyName("mainClass")]
    public string MainClass { get; set; } = string.Empty;

    /// <summary>
    /// 当前版本的存储选项
    /// </summary>
    [JsonPropertyName("storageOptions")]
    public StorageOptions StorageOptions { get; set; } = new();

    /// <summary>
    /// 版本是否可用（文件完整性检查）
    /// </summary>
    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    /// <summary>
    /// 最后验证时间
    /// </summary>
    [JsonPropertyName("lastValidated")]
    public DateTimeOffset LastValidated { get; set; }

    
    /// <summary>
    /// 验证版本文件是否存在
    /// </summary>
    public bool ValidateFiles()
    {
        var jarPath = StorageOptions.GetVersionJarPath(Name);
        var jsonPath = StorageOptions.GetVersionJsonPath(Name);

        IsAvailable = File.Exists(jarPath) && File.Exists(jsonPath);
        LastValidated = DateTime.Now;

        return IsAvailable;
    }

    /// <summary>
    /// 获取版本显示名称（优先使用DisplayName，没有则使用Id）
    /// </summary>
    public string GetDisplayName()
    {
        return string.IsNullOrEmpty(DisplayName) ? VersionId : DisplayName;
    }

    /// <summary>
    /// 检查是否是原版版本（没有继承）
    /// </summary>
    public bool IsVanilla()
    {
        return string.IsNullOrEmpty(InheritsFrom);
    }

    /// <summary>
    /// 检查是否是Mod版本
    /// </summary>
    public bool IsModded()
    {
        return ModLoaderType != ModLoaderType.None;
    }

    public async Task<GameVersionDetail?> GetGameVersionDetailAsync()
    {
        // 读取版本详情
        var jsonPath = StorageOptions.GetVersionJsonPath(Name);
        // json序列化
        var txt = await File.ReadAllTextAsync(jsonPath);
        var gameVersionDetail =
            JsonSerializer.Deserialize(txt, MinecraftJsonSerializerContext.Default.GameVersionDetail);
        return gameVersionDetail;
    }

    /// <summary>
    /// 获取存档信息
    /// </summary>
    /// <param name="gameVersionDetail">版本</param>
    /// <param name="storageOptions">版本存放配置</param>
    /// <param name="modLoaderType">存储的加载器类型</param>
    /// <param name="customName">自定义的存放名称</param>
    /// <returns></returns>
    public static VersionInfoDetail GetVersionInfoDetail(GameVersionDetail gameVersionDetail,
        StorageOptions storageOptions, ModLoaderType modLoaderType, string? customName = null)
    {
        var name = customName ?? gameVersionDetail.Id;
        VersionInfoDetail versionInfoDetail = new()
        {
            VersionId = gameVersionDetail.Id,
            Name = name,
            DisplayName = name,
            AssetsFolder = storageOptions.GetAssetsPath(gameVersionDetail.Id),
            BasePath = storageOptions.BasePath,
            LibraryFolder = storageOptions.GetLibrariesPath(gameVersionDetail.Id),
            NativesFolder = storageOptions.GetNativesPath(gameVersionDetail.Id),
            VersionFolder = storageOptions.GetVersionPath(name),
            VersionType = gameVersionDetail.Type,
            ModLoaderType = modLoaderType,
            StorageOptions = storageOptions,
            InheritsFrom = gameVersionDetail.InheritsFrom,
            ReleaseTime = gameVersionDetail.ReleaseTime,
            JavaVersion = gameVersionDetail.JavaVersion,
            MainClass = gameVersionDetail.MainClass,
        };

        return versionInfoDetail;
    }
}