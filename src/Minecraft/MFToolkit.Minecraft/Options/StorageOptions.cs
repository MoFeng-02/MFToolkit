using System;
using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Options;

/// <summary>
/// 存储配置选项
/// </summary>
public class StorageOptions : BaseOptions
{
    public static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config", "storage_config.json");
    private string _basePath = ".minecraft";

    /// <summary>
    /// 基础路径（不用官方路径，原因是可能当前目录就是在非系统盘，默认以当前软件存储位置下进行存放，而且默认的系统盘下，存放太多可能导致系统盘容积减少太多，对用户不友好）
    /// </summary>
    public string BasePath
    {
        get =>
            _basePath == ".minecraft"
                ? Path.Combine(AppContext.BaseDirectory, ".minecraft")
                : _basePath;
        set => _basePath = value;
    }

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
    /// 获取版本清单目录路径
    /// </summary>
    /// <returns></returns>
    public virtual string GetVersionManifestDirectory()
    {
        return Path.Combine(AppContext.BaseDirectory, "VersionManifest");
    }

    /// <summary>
    /// 获取版本清单路径
    /// </summary>
    /// <returns></returns>
    public virtual string GetVersionManifestPath()
    {
        return Path.Combine(GetVersionManifestDirectory(), "version_manifest.json");
    }

    /// <summary>
    /// 获取版本存储路径
    /// </summary>
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>版本存储路径</returns>
    public virtual string GetVersionsPath(string? folderName = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "versions"),
            StorageMode.Isolated when folderName != null => Path.Combine(BasePath, "versions", folderName),
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
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>资源存储路径</returns>
    public virtual string GetAssetsPath(string? folderName = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "assets"),
            StorageMode.Isolated when folderName != null => Path.Combine(BasePath, "versions", folderName, "assets"),
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
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>库存储路径</returns>
    public virtual string GetLibrariesPath(string? folderName = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "libraries"),
            StorageMode.Isolated when folderName != null => Path.Combine(BasePath, "versions", folderName, "libraries"),
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
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>配置存储路径</returns>
    public virtual string GetConfigPath(string? folderName = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "config"),
            StorageMode.Isolated when folderName != null => Path.Combine(BasePath, "versions", folderName, "config"),
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
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>日志存储路径</returns>
    public virtual string GetLogsPath(string? folderName = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "logs"),
            StorageMode.Isolated when folderName != null => Path.Combine(BasePath, "versions", folderName, "logs"),
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
    /// <param name="versionName">存放文件夹的名称/文件名称，通常是版本ID</param>
    /// <returns>版本完整路径</returns>
    public virtual string GetVersionPath(string versionName)
    {
        if (string.IsNullOrEmpty(versionName))
            throw new ArgumentNullException(nameof(versionName));

        return Path.Combine(GetVersionsPath(versionName), versionName);
    }

    /// <summary>
    /// 获取特定版本的JAR文件路径
    /// </summary>
    /// <param name="versionName">存放文件夹的名称/文件名称，通常是版本ID</param>
    /// <returns>JAR文件路径</returns>
    public virtual string GetVersionJarPath(string versionName)
    {
        if (string.IsNullOrEmpty(versionName))
            throw new ArgumentNullException(nameof(versionName));

        return Path.Combine(GetVersionPath(versionName), $"{versionName}.jar");
    }

    /// <summary>
    /// 获取特定版本的JSON文件路径
    /// </summary>
    /// <param name="versionName">存放文件夹的名称/文件名称，通常是版本ID</param>
    /// <returns>JSON文件路径</returns>
    public virtual string GetVersionJsonPath(string versionName)
    {
        if (string.IsNullOrEmpty(versionName))
            throw new ArgumentNullException(nameof(versionName));

        return Path.Combine(GetVersionPath(versionName), $"{versionName}.json");
    }

    /// <summary>
    /// 获取资源索引文件夹路径
    /// </summary>
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    public virtual string GetAssetIndexsPath(string? folderName = null)
    {
        return Path.Combine(GetAssetsPath(folderName), "indexes");
    }

    /// <summary>
    /// 获取资源索引文件路径
    /// </summary>
    /// <param name="assetsVersion">资源版本</param>
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>资源索引文件路径</returns>
    public virtual string GetAssetIndexPath(string assetsVersion, string? folderName = null)
    {
        if (string.IsNullOrEmpty(assetsVersion))
            throw new ArgumentNullException(nameof(assetsVersion));

        return Path.Combine(GetAssetIndexsPath(folderName), $"{assetsVersion}.json");
    }

    /// <summary>
    /// 获取获取资源对象路径
    /// </summary>
    /// <param name="hash">资源哈希值</param>
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>资源对象路径</returns>
    public virtual string GetAssetObjectPath(string hash, string? folderName = null)
    {
        if (string.IsNullOrEmpty(hash))
            throw new ArgumentNullException(nameof(hash));

        if (hash.Length < 2)
            throw new ArgumentException("哈希值长度必须至少为2", nameof(hash));

        string prefix = hash[..2];
        return Path.Combine(GetAssetsPath(folderName), "objects", prefix, hash);
    }

    /// <summary>
    /// 获取库文件路径
    /// </summary>
    /// <param name="libraryPath">库路径（如：com/mojang/netty/1.6/netty-1.6.jar）</param>
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>库文件路径</returns>
    public virtual string GetLibraryPath(string libraryPath, string? folderName = null)
    {
        if (string.IsNullOrEmpty(libraryPath))
            throw new ArgumentNullException(nameof(libraryPath));

        return Path.Combine(GetLibrariesPath(folderName), libraryPath);
    }

    /// <summary>
    /// 获取皮肤存储路径
    /// </summary>
    /// <returns></returns>
    public virtual string GetSkinsPath()
    {
        return Path.Combine(GetAssetsPath(), "skins");
    }

    /// <summary>
    /// 获取资源索引目录路径
    /// </summary>
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns></returns>
    public virtual string GetIndexsPath(string? folderName = null)
    {
        return Path.Combine(GetAssetsPath(folderName), "indexes");
    }

    /// <summary>
    /// 获取资源对象目录路径
    /// </summary>
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns></returns>
    public virtual string GetObjectsPath(string? folderName = null)
    {
        return Path.Combine(GetAssetsPath(folderName), "objects");
    }

    /// <summary>
    /// 获取原生库存储路径（用于存放平台相关的本地库文件）
    /// </summary>
    /// <param name="versionName">存放文件夹的名称/文件名称，通常是版本ID</param>
    /// <returns>原生库存储路径</returns>
    public virtual string GetNativesPath(string versionName)
    {
        if (string.IsNullOrEmpty(versionName))
            throw new ArgumentNullException(nameof(versionName));

        // 根据官方Minecraft目录结构，natives目录通常位于版本目录下的特定子目录
        // 格式：versions/{version}/{version}-natives 或 versions/{version}/natives
        // 我们使用更常见的 {version}-natives 格式
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "versions", versionName, $"{versionName}-natives"),
            StorageMode.Isolated => Path.Combine(BasePath, "versions", versionName, $"{versionName}-natives"),
            StorageMode.Hybrid => Path.Combine(BasePath, "versions", versionName, $"{versionName}-natives"),
            StorageMode.Custom when !string.IsNullOrEmpty(CustomVersionsPath) =>
                Path.Combine(CustomVersionsPath, versionName, $"{versionName}-natives"),
            StorageMode.Custom => throw new InvalidOperationException("未设置自定义版本路径"),
            _ => Path.Combine(BasePath, "versions", versionName, $"{versionName}-natives")
        };
    }

    /// <summary>
    /// 获取截图存储路径
    /// </summary>
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>截图存储路径</returns>
    public virtual string GetScreenshotsPath(string? folderName = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "screenshots"),
            StorageMode.Isolated when folderName != null => Path.Combine(BasePath, "versions", folderName,
                "screenshots"),
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
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    /// <returns>世界存储路径</returns>
    public virtual string GetSavesPath(string? folderName = null)
    {
        return StorageMode switch
        {
            StorageMode.Global => Path.Combine(BasePath, "saves"),
            StorageMode.Isolated when folderName != null => Path.Combine(BasePath, "versions", folderName, "saves"),
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
    /// <param name="folderName">存放文件夹的名称/文件名称，通常是版本ID（可选）</param>
    public virtual void EnsureDirectories(string? folderName = null)
    {
        // 创建基础目录
        Directory.CreateDirectory(BasePath);

        // 创建版本目录
        Directory.CreateDirectory(GetVersionsPath(folderName));

        // 如果提供了版本ID，创建特定版本的目录
        if (!string.IsNullOrEmpty(folderName))
        {
            Directory.CreateDirectory(GetVersionPath(folderName));
        }

        // 创建资源目录
        Directory.CreateDirectory(GetAssetsPath(folderName));
        Directory.CreateDirectory(GetIndexsPath(folderName));
        Directory.CreateDirectory(GetObjectsPath(folderName));

        // 创建库目录
        Directory.CreateDirectory(GetLibrariesPath(folderName));

        // 创建配置目录
        Directory.CreateDirectory(GetConfigPath(folderName));

        // 创建日志目录
        Directory.CreateDirectory(GetLogsPath(folderName));
    }

    /// <summary>
    /// 保存当前选项类
    /// </summary>
    /// <returns></returns>
    public Task SaveAsync()
    {
        return base.SaveAsync(ConfigPath);
    }
}