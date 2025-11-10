using MFToolkit.Minecraft.Entities.GameVersion;

namespace MFToolkit.Minecraft.Extensions.VersionExtensions;

/// <summary>
/// 游戏版本详情的扩展方法
/// </summary>
public static class GameVersionInfoExtensions
{
    /// <summary>
    /// 获取适用于指定环境的所有库
    /// </summary>
    public static IEnumerable<Library> GetApplicableLibraries(this GameVersionDetail version, string os, Dictionary<string, bool>? features = null)
    {
        return version.Libraries.Where(lib => lib.IsApplicable(os, features));
    }

    /// <summary>
    /// 获取普通库（非原生库）
    /// </summary>
    public static IEnumerable<Library> GetNormalLibraries(this GameVersionDetail version, string os, Dictionary<string, bool>? features = null)
    {
        return version.GetApplicableLibraries(os, features).Where(lib => !lib.HasNatives);
    }

    /// <summary>
    /// 获取原生库
    /// </summary>
    public static IEnumerable<Library> GetNativeLibraries(this GameVersionDetail version, string os, Dictionary<string, bool>? features = null)
    {
        return version.GetApplicableLibraries(os, features).Where(lib => lib.HasNatives);
    }

    /// <summary>
    /// 获取所有库的下载项
    /// </summary>
    public static IEnumerable<DownloadItem> GetAllLibraryDownloads(this GameVersionDetail version, string os, Dictionary<string, bool>? features = null)
    {
        foreach (var library in version.GetApplicableLibraries(os, features))
        {
            foreach (var download in library.GetDownloads(os))
            {
                yield return download;
            }
        }
    }

    /// <summary>
    /// 获取原生库的下载项（包含提取规则）
    /// </summary>
    public static IEnumerable<(DownloadItem Download, ExtractRules? ExtractRules)> GetNativeLibraryDownloads(this GameVersionDetail version, string os, Dictionary<string, bool>? features = null)
    {
        foreach (var library in version.GetNativeLibraries(os, features))
        {
            var nativeClassifier = library.GetNativeClassifier(os);
            if (!string.IsNullOrEmpty(nativeClassifier) &&
                library.Downloads?.Classifiers != null &&
                library.Downloads.Classifiers.TryGetValue(nativeClassifier, out var download))
            {
                yield return (download, library.Extract);
            }
        }
    }
}