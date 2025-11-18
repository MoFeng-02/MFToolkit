using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Enums;
using MFToolkit.Minecraft.Options;

namespace MFToolkit.Minecraft.Services.Downloads.Internal;

public class BaseHandle
{
    public ModLoaderType ModLoaderType { get; set; }

    /// <summary>
    /// 获取存档信息
    /// </summary>
    /// <param name="gameVersionDetail">版本</param>
    /// <param name="storageOptions">版本存放配置</param>
    /// <param name="customName">自定义的存放名称</param>
    /// <returns></returns>
    public VersionInfoDetail GetVersionInfoDetail(GameVersionDetail gameVersionDetail,
        StorageOptions storageOptions, string? customName = null)
    {
        var name = customName ?? gameVersionDetail.Id;
        VersionInfoDetail versionInfoDetail = new()
        {
            VersionId = gameVersionDetail.Id,
            Name = name,
            DisplayName = name,
            AssetsFolder = storageOptions.GetAssetsPath(name),
            BasePath = storageOptions.BasePath,
            LibraryFolder = storageOptions.GetLibrariesPath(name),
            NativesFolder = storageOptions.GetNativesPath(name),
            VersionFolder = storageOptions.GetVersionPath(name),
            VersionType = gameVersionDetail.Type,
            ModLoaderType = ModLoaderType,
            StorageOptions = storageOptions,
            InheritsFrom = gameVersionDetail.InheritsFrom,
            ReleaseTime = gameVersionDetail.ReleaseTime,
            JavaVersion = gameVersionDetail.JavaVersion,
            MainClass = gameVersionDetail.MainClass,
        };

        return versionInfoDetail;
    }
}