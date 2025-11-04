using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// 存储配置选项
/// </summary>
public class StorageOptions() : BaseOptions("storage_options.json")
{
    /// <summary>
    /// 基础路径
    /// </summary>
    public string BasePath { get; set; } = ".minecraft";

    /// <summary>
    /// 存储模式
    /// </summary>
    public StorageMode StorageMode { get; set; } = StorageMode.Global;

    /// <summary>
    /// 自定义版本路径（仅在完全自定义模式下有效）
    /// </summary>
    public string? CustomVersionsPath { get; set; }

    /// <summary>
    /// 自定义资源路径（仅在完全自定义模式下有效）
    /// </summary>
    public string? CustomAssetsPath { get; set; }

    /// <summary>
    /// 自定义库路径（仅在完全自定义模式下有效）
    /// </summary>
    public string? CustomLibrariesPath { get; set; }

    /// <summary>
    /// 自定义配置路径（仅在完全自定义模式下有效）
    /// </summary>
    public string? CustomConfigPath { get; set; }

    /// <summary>
    /// 自定义日志路径（仅在完全自定义模式下有效）
    /// </summary>
    public string? CustomLogsPath { get; set; }

    /// <summary>
    /// 获取版本存储路径
    /// </summary>
    /// <param name="versionId">版本ID（可选）</param>
    /// <returns>版本存储路径</returns>
    public string GetVersionsPath(string? versionId = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "versions"),
            StorageMode.Isolated when versionId != null => Path.Combine(BasePath, "versions", versionId),
            StorageMode.Isolated => throw new InvalidOperationException("版本ID不能为空"),
            StorageMode.Hybrid => Path.Combine(BasePath, "versions"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomVersionsPath) => CustomVersionsPath,
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义版本路径"),
            _ => Path.Combine(BasePath, "versions")
        };
    }

    /// <summary>
    /// 获取资源存储路径
    /// </summary>
    /// <param name="versionId">版本ID（可选）</param>
    /// <returns>资源存储路径</returns>
    public string GetAssetsPath(string? versionId = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "assets"),
            StorageMode.Isolated when versionId != null => Path.Combine(BasePath, "versions", versionId, "assets"),
            StorageMode.Isolated => throw new InvalidOperationException("版本ID不能为空"),
            StorageMode.Hybrid => Path.Combine(BasePath, "assets"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomAssetsPath) => CustomAssetsPath,
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义资源路径"),
            _ => Path.Combine(BasePath, "assets")
        };
    }

    /// <summary>
    /// 获取库存储路径
    /// </summary>
    /// <param name="versionId">版本ID（可选）</param>
    /// <returns>库存储路径</returns>
    public string GetLibrariesPath(string? versionId = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "libraries"),
            StorageMode.Isolated when versionId != null => Path.Combine(BasePath, "versions", versionId, "libraries"),
            StorageMode.Isolated => throw new InvalidOperationException("版本ID不能为空"),
            StorageMode.Hybrid => Path.Combine(BasePath, "libraries"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomLibrariesPath) => CustomLibrariesPath,
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义库路径"),
            _ => Path.Combine(BasePath, "libraries")
        };
    }

    /// <summary>
    /// 获取配置存储路径
    /// </summary>
    /// <param name="versionId">版本ID（可选）</param>
    /// <returns>配置存储路径</returns>
    public string GetConfigPath(string? versionId = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "config"),
            StorageMode.Isolated when versionId != null => Path.Combine(BasePath, "versions", versionId, "config"),
            StorageMode.Isolated => throw new InvalidOperationException("版本ID不能为空"),
            StorageMode.Hybrid => Path.Combine(BasePath, "config"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomConfigPath) => CustomConfigPath,
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义配置路径"),
            _ => Path.Combine(BasePath, "config")
        };
    }

    /// <summary>
    /// 获取日志存储路径
    /// </summary>
    /// <param name="versionId">版本ID（可选）</param>
    /// <returns>日志存储路径</returns>
    public string GetLogsPath(string? versionId = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "logs"),
            StorageMode.Isolated when versionId != null => Path.Combine(BasePath, "versions", versionId, "logs"),
            StorageMode.Isolated => throw new InvalidOperationException("版本ID不能为空"),
            StorageMode.Hybrid => Path.Combine(BasePath, "logs"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomLogsPath) => CustomLogsPath,
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义日志路径"),
            _ => Path.Combine(BasePath, "logs")
        };
    }

    /// <summary>
    /// 获取特定版本的完整路径
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>版本完整路径</returns>
    public string GetVersionPath(string versionId)
    {
        if (string.IsNullOrEmpty(versionId))
            throw new ArgumentNullException(nameof(versionId));

        return Path.Combine(GetVersionsPath(versionId), versionId);
    }

    /// <summary>
    /// 获取特定版本的JAR文件路径
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>JAR文件路径</returns>
    public string GetVersionJarPath(string versionId)
    {
        if (string.IsNullOrEmpty(versionId))
            throw new ArgumentNullException(nameof(versionId));

        return Path.Combine(GetVersionPath(versionId), $"{versionId}.jar");
    }

    /// <summary>
    /// 获取特定版本的JSON文件路径
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>JSON文件路径</returns>
    public string GetVersionJsonPath(string versionId)
    {
        if (string.IsNullOrEmpty(versionId))
            throw new ArgumentNullException(nameof(versionId));

        return Path.Combine(GetVersionPath(versionId), $"{versionId}.json");
    }

    /// <summary>
    /// 获取资源索引文件路径
    /// </summary>
    /// <param name="assetsVersion">资源版本</param>
    /// <returns>资源索引文件路径</returns>
    public string GetAssetIndexPath(string assetsVersion)
    {
        if (string.IsNullOrEmpty(assetsVersion))
            throw new ArgumentNullException(nameof(assetsVersion));

        return Path.Combine(GetAssetsPath(), "indexes", $"{assetsVersion}.json");
    }

    /// <summary>
    /// 获取获取资源对象路径
    /// </summary>
    /// <param name="hash">资源哈希值</param>
    /// <returns>资源对象路径</returns>
    public string GetAssetObjectPath(string hash)
    {
        if (string.IsNullOrEmpty(hash))
            throw new ArgumentNullException(nameof(hash));

        if (hash.Length < 2)
            throw new ArgumentException("哈希值长度必须至少为2", nameof(hash));

        string prefix = hash[..2];
        return Path.Combine(GetAssetsPath(), "objects", prefix, hash);
    }

    /// <summary>
    /// 获取库文件路径
    /// </summary>
    /// <param name="libraryPath">库路径（如：com/mojang/netty/1.6/netty-1.6.jar）</param>
    /// <returns>库文件路径</returns>
    public string GetLibraryPath(string libraryPath)
    {
        if (string.IsNullOrEmpty(libraryPath))
            throw new ArgumentNullException(nameof(libraryPath));

        return Path.Combine(GetLibrariesPath(), libraryPath);
    }

    /// <summary>
    /// 获取资源索引目录路径
    /// </summary>
    /// <param name="versionId"></param>
    /// <returns></returns>
    public string GetIndexsPath(string? versionId = null)
    {
        return Path.Combine(GetAssetsPath(versionId), "indexes");
    }

    /// <summary>
    /// 获取资源对象目录路径
    /// </summary>
    /// <param name="versionId"></param>
    /// <returns></returns>
    public string GetObjectsPath(string? versionId = null)
    {
        return Path.Combine(GetAssetsPath(versionId), "objects");
    }

    /// <summary>
    /// 获取原生库存储路径（用于存放平台相关的本地库文件）
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>原生库存储路径</returns>
    public string GetNativesPath(string versionId)
    {
        if (string.IsNullOrEmpty(versionId))
            throw new ArgumentNullException(nameof(versionId));

        // 根据官方Minecraft目录结构，natives目录通常位于版本目录下的特定子目录
        // 格式：versions/{version}/{version}-natives 或 versions/{version}/natives
        // 我们使用更常见的 {version}-natives 格式
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "versions", versionId, $"{versionId}-natives"),
            StorageMode.Isolated => Path.Combine(BasePath, "versions", versionId, $"{versionId}-natives"),
            StorageMode.Hybrid => Path.Combine(BasePath, "versions", versionId, $"{versionId}-natives"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomVersionsPath) =>
                Path.Combine(CustomVersionsPath, versionId, $"{versionId}-natives"),
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义版本路径"),
            _ => Path.Combine(BasePath, "versions", versionId, $"{versionId}-natives")
        };
    }

    /// <summary>
    /// 获取截图存储路径
    /// </summary>
    /// <param name="versionId">版本ID（可选）</param>
    /// <returns>截图存储路径</returns>
    public string GetScreenshotsPath(string? versionId = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "screenshots"),
            StorageMode.Isolated when versionId != null => Path.Combine(BasePath, "versions", versionId, "screenshots"),
            StorageMode.Isolated => throw new InvalidOperationException("版本ID不能为空"),
            StorageMode.Hybrid => Path.Combine(BasePath, "screenshots"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomAssetsPath) =>
                Path.Combine(CustomAssetsPath, "screenshots"),
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义资源路径"),
            _ => Path.Combine(BasePath, "screenshots")
        };
    }

    /// <summary>
    /// 获取保存的世界存储路径
    /// </summary>
    /// <param name="versionId">版本ID（可选）</param>
    /// <returns>世界存储路径</returns>
    public string GetSavesPath(string? versionId = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "saves"),
            StorageMode.Isolated when versionId != null => Path.Combine(BasePath, "versions", versionId, "saves"),
            StorageMode.Isolated => throw new InvalidOperationException("版本ID不能为空"),
            StorageMode.Hybrid => Path.Combine(BasePath, "saves"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomAssetsPath) =>
                Path.Combine(CustomAssetsPath, "saves"),
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义资源路径"),
            _ => Path.Combine(BasePath, "saves")
        };
    }

    /// <summary>
    /// 确保所有必要的目录都已创建
    /// </summary>
    /// <param name="versionId">版本ID（可选）</param>
    public void EnsureDirectories(string? versionId = null)
    {
        // 创建基础目录
        Directory.CreateDirectory(BasePath);

        // 创建版本目录
        Directory.CreateDirectory(GetVersionsPath(versionId));

        // 如果提供了版本ID，创建特定版本的目录
        if (!string.IsNullOrEmpty(versionId))
        {
            Directory.CreateDirectory(GetVersionPath(versionId));
        }

        // 创建资源目录
        Directory.CreateDirectory(GetAssetsPath(versionId));
        Directory.CreateDirectory(GetIndexsPath(versionId));
        Directory.CreateDirectory(GetObjectsPath(versionId));

        // 创建库目录
        Directory.CreateDirectory(GetLibrariesPath(versionId));

        // 创建配置目录
        Directory.CreateDirectory(GetConfigPath(versionId));

        // 创建日志目录
        Directory.CreateDirectory(GetLogsPath(versionId));
    }
}
