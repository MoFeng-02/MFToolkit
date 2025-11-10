using MFToolkit.Minecraft.Entities.GameVersion;
using MFToolkit.Minecraft.Entities.Versions;

namespace MFToolkit.Minecraft.Services.Downloads.Interfaces;

/// <summary>
/// 清单服务接口
/// <para>包括：原版清单，加载器清单</para>
/// </summary>
public interface IMinecraftVersionService
{
    /// <summary>
    /// 获取服务清单
    /// </summary>
    /// <param name="version">使用哪个版本的清单，默认v2，也建议V2</param>
    /// <param name="isRefresh">是否强制重新加载版本清单</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<VersionManifest> GetVersionManifestAsync(string version = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json", bool isRefresh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查找所选版本ID的，如果有本地路径优先本地路径获取，否则在VersionManifest中获取
    /// </summary>
    /// <param name="versionId">游戏版本ID</param>
    /// <param name="localPath">本地路径（直接包括了文件名）</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GameVersionDetail> GetGameVersionInfoAsync(string versionId, string? localPath = null, CancellationToken cancellationToken = default);
}