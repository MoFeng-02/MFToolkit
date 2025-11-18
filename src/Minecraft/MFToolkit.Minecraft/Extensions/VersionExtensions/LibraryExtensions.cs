// Library.Extensions.cs

using System.Runtime.InteropServices;
using MFToolkit.Minecraft.Entities.GameVersion;

namespace MFToolkit.Minecraft.Extensions.VersionExtensions;

/// <summary>
/// 库相关的扩展方法
/// </summary>
public static class LibraryExtensions
{
    /// <summary>
    /// 获取库的文件路径（相对于libraries目录）
    /// </summary>
    public static string GetLibraryPath(this Library library)
    {
        // 如果有下载信息中的路径，优先使用
        if (!string.IsNullOrEmpty(library.Downloads?.Artifact?.Path))
            return library.Downloads.Artifact.Path;

        // 否则从库名称解析
        var parts = library.Name.Split(':');
        if (parts.Length < 3)
            return $"{library.Name}.jar";

        var groupId = parts[0].Replace('.', '/');
        var artifactId = parts[1];
        var version = parts[2];

        // 处理分类器（第四部分）
        string? classifier = null;
        string extension = "jar";

        if (parts.Length >= 4 && !string.IsNullOrEmpty(parts[3]))
        {
            // 检查是否包含扩展名
            if (parts[3].Contains('@'))
            {
                var classifierParts = parts[3].Split('@');
                classifier = classifierParts[0];
                extension = classifierParts.Length > 1 ? classifierParts[1] : extension;
            }
            else
            {
                classifier = parts[3];
            }
        }

        // 处理显式扩展名（第五部分）
        if (parts.Length >= 5 && !string.IsNullOrEmpty(parts[4]))
        {
            extension = parts[4];
        }

        // 构建标准 Maven 路径
        var fileName = $"{artifactId}-{version}";
        if (!string.IsNullOrEmpty(classifier))
        {
            fileName += $"-{classifier}";
        }

        fileName += $".{extension}";

        return $"{groupId}/{artifactId}/{version}/{fileName}";
    }

    /// <summary>
    /// 获取库的文件名
    /// </summary>
    public static string GetLibraryFileName(this Library library)
    {
        var path = library.GetLibraryPath();
        return Path.GetFileName(path);
    }

    /// <summary>
    /// 提取库的所有适用构件路径
    /// </summary>
    /// <param name="library">库信息</param>
    /// <param name="os">目标操作系统</param>
    /// <param name="librariesRoot">库根目录</param>
    /// <returns>所有适用的库文件路径</returns>
    public static List<string> ExtractLibraryPaths(this Library library, string os, string librariesRoot)
    {
        var paths = new List<string>();

        if (!library.IsApplicable(os))
            return paths;

        // 提取主要构件（所有平台通用）
        if (library.Downloads?.Artifact != null)
        {
            var mainPath = ExtractPathFromDownloadItem(library.Downloads.Artifact, library.Name, librariesRoot);
            paths.Add(Path.Combine(librariesRoot, mainPath));
        }

        // 提取原生构件（特定平台）
        var nativeClassifier = library.GetNativeClassifier(os);
        if (!string.IsNullOrEmpty(nativeClassifier) &&
            library.Downloads?.Classifiers != null &&
            library.Downloads.Classifiers.TryGetValue(nativeClassifier, out var nativeDownload))
        {
            var nativePath = ExtractPathFromDownloadItem(nativeDownload, library.Name, librariesRoot, nativeClassifier);
            paths.Add(Path.Combine(librariesRoot, nativePath));
        }

        return paths;
    }

    /// <summary>
    /// 批量提取多个库的路径
    /// </summary>
    public static List<string> ExtractAllLibraryPaths(
        this IEnumerable<Library> libraries, string os, string librariesRoot)
    {
        var paths = new List<string>();

        foreach (var library in libraries)
        {
            var libraryPaths = library.ExtractLibraryPaths(os, librariesRoot);
            paths.AddRange(libraryPaths);
        }

        return paths;
    }

    /// <summary>
    /// 从下载项中提取文件路径
    /// </summary>
    private static string ExtractPathFromDownloadItem(DownloadItem download, string libraryName,
        string librariesRoot, string? classifier = null)
    {
        // 优先使用下载项中的path
        if (!string.IsNullOrEmpty(download.Path))
            return download.Path;

        // 回退方案：从Maven坐标构建路径
        return BuildMavenPath(libraryName, classifier);
    }

    /// <summary>
    /// 根据Maven坐标构建库路径
    /// </summary>
    private static string BuildMavenPath(string mavenCoordinate, string? classifier = null)
    {
        var parts = mavenCoordinate.Split(':');
        if (parts.Length < 3)
            return mavenCoordinate;

        var groupId = parts[0].Replace('.', Path.DirectorySeparatorChar);
        var artifactId = parts[1];
        var version = parts[2];

        var fileName = $"{artifactId}-{version}";

        if (!string.IsNullOrEmpty(classifier))
            fileName += $"-{classifier}";

        fileName += ".jar";

        return Path.Combine(groupId, artifactId, version, fileName);
    }

    private static string? ParseClassifierAndExtension(string part, ref string extension)
    {
        if (part.Contains('@'))
        {
            var classifierParts = part.Split('@');
            extension = classifierParts.Length > 1 ? classifierParts[1] : extension;
            return classifierParts[0];
        }

        return part;
    }

    private static string BuildFileName(string artifactId, string version, string? classifier, string extension)
    {
        var fileName = $"{artifactId}-{version}";
        if (!string.IsNullOrEmpty(classifier))
        {
            fileName += $"-{classifier}";
        }

        return $"{fileName}.{extension}";
    }

    /// <summary>
    /// 获取简略信息
    /// </summary>
    /// <param name="libraries"></param>
    /// <param name="os">系统</param>
    /// <param name="librariesRoot">库的根目录路径</param>
    /// <returns></returns>
    public static IEnumerable<LibrarySimple> GetLibrarySimples(this IEnumerable<Library> libraries, string os,
        string librariesRoot)
    {
        var simples = new List<LibrarySimple>();
        foreach (var library in libraries)
        {
            if (!library.IsApplicable(os))
                continue;

            // 添加主要工件
            if (library.Downloads?.Artifact != null)
            {
                simples.Add(new()
                {
                    Name = library.Name,
                    Path = Path.Combine(librariesRoot, library.GetLibraryPath()),
                    Sha1 = library.Downloads.Artifact.Sha1,
                    Size = library.Downloads.Artifact.Size,
                });
            }

            // 添加原生库 - 关键修正：使用正确的路径
            var nativeClassifier = library.GetNativeClassifier(os);
            if (!string.IsNullOrEmpty(nativeClassifier) &&
                library.Downloads?.Classifiers != null &&
                library.Downloads.Classifiers.TryGetValue(nativeClassifier, out var nativeDownload))
            {
                // 方法1：优先使用下载项中的路径
                var nativePath = !string.IsNullOrEmpty(nativeDownload.Path)
                    ? nativeDownload.Path
                    : library.GetLibraryPathWithClassifier(nativeClassifier); // 使用带分类器的路径

                simples.Add(new()
                {
                    Name = library.Name,
                    Path = Path.Combine(librariesRoot, nativePath),
                    Sha1 = nativeDownload.Sha1,
                    Size = nativeDownload.Size,
                });
            }
        }

        return simples.DistinctBy(q => q.Name);
    }

    /// <summary>
    /// 获取带分类器的库路径
    /// </summary>
    public static string GetLibraryPathWithClassifier(this Library library, string classifier)
    {
        // 如果有明确的下载路径，使用它
        if (library.Downloads?.Classifiers != null &&
            library.Downloads.Classifiers.TryGetValue(classifier, out var download) &&
            !string.IsNullOrEmpty(download.Path))
        {
            return download.Path;
        }

        // 否则基于主库路径构建带分类器的路径
        var basePath = library.GetLibraryPath();
        var directory = Path.GetDirectoryName(basePath);
        var fileName = Path.GetFileNameWithoutExtension(basePath);
        var extension = Path.GetExtension(basePath);

        return Path.Combine(directory, $"{fileName}-{classifier}{extension}");
    }

    // /// <summary>
    // /// 获取native库
    // /// </summary>
    // /// <param name="libraries"></param>
    // /// <param name="os">系统</param>
    // /// <param name="librariesRoot">库的根目录路径</param>
    // /// <returns></returns>
    // public static List<LibrarySimple> GetLibraryNativesSimples(this IEnumerable<Library> libraries, string os,
    //     string librariesRoot)
    // {
    //     var allLibraries = libraries.GetLibrarySimples(os, librariesRoot);
    //
    //     // 过滤包含"natives"的路径
    //     return allLibraries.Where(q => q.Path.Contains("natives")).ToList();
    // }

    /// <summary>
    /// 获取原生库（全架构支持）
    /// </summary>
    public static List<LibrarySimple> GetLibraryNativesSimples(this IEnumerable<Library> libraries, string os,
        string librariesRoot)
    {
        var natives = new List<LibrarySimple>();
        var (architecture, bitness) = GetCurrentArchitectureInfo();

        foreach (var library in libraries)
        {
            if (!library.IsApplicable(os) || !library.HasNatives)
                continue;

            var nativeClassifier = FindNativeClassifier(library, os, architecture, bitness);
            if (string.IsNullOrEmpty(nativeClassifier))
                continue;

            if (library.Downloads?.Classifiers != null &&
                library.Downloads.Classifiers.TryGetValue(nativeClassifier, out var nativeDownload))
            {
                var nativePath = !string.IsNullOrEmpty(nativeDownload.Path)
                    ? nativeDownload.Path
                    : library.GetLibraryPathWithClassifier(nativeClassifier);

                natives.Add(new()
                {
                    Name = library.Name,
                    Path = Path.Combine(librariesRoot, nativePath),
                    Sha1 = nativeDownload.Sha1,
                    Size = nativeDownload.Size,
                });
            }
        }

        return natives;
    }

    /// <summary>
    /// 获取当前系统架构信息
    /// </summary>
    private static (string architecture, string bitness) GetCurrentArchitectureInfo()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var is64Bit = Environment.Is64BitOperatingSystem;

        return architecture switch
        {
            Architecture.X86 => ("x86", "32"),
            Architecture.X64 => ("x64", "64"),
            Architecture.Arm => ("arm", "32"),
            Architecture.Arm64 => ("arm64", "64"),
            _ => (is64Bit ? "x64" : "x86", is64Bit ? "64" : "32")
        };
    }

    /// <summary>
    /// 查找最适合的原生分类器（全架构支持）
    /// </summary>
    private static string? FindNativeClassifier(Library library, string os, string architecture, string bitness)
    {
        if (library.Natives == null) return null;

        // 架构别名映射
        var archAliases = new Dictionary<string, string[]>
        {
            ["x86"] = new[] { "x86", "32", "i386", "i686", "x32" },
            ["x64"] = new[] { "", "x64", "64", "x86_64", "amd64", "x86-64" },
            ["arm"] = new[] { "arm", "arm32", "armv7", "armhf", "armel" },
            ["arm64"] = new[] { "arm64", "aarch64", "armv8", "arm64-v8a" }
        };

        // 匹配优先级: 从最精确到最通用
        var searchPatterns = new List<string>
        {
            // 1. 精确架构匹配 (windows-arm64, linux-x64)
            $"{os}-{architecture}",

            // 2. 位数架构匹配 (windows-64, linux-32)
            $"{os}-{bitness}",

            // 3. 通用架构匹配 (windows-x86, linux-arm)
            $"{os}-{GetArchitectureFamily(architecture)}",

            // 4. 通用操作系统匹配 (windows, linux)
            os,
        };

        // 5. 添加架构别名匹配
        if (archAliases.TryGetValue(architecture, out var aliases))
        {
            foreach (var alias in aliases)
            {
                searchPatterns.Add($"{os}-{alias}");
            }
        }

        foreach (var pattern in searchPatterns.Distinct())
        {
            if (library.Natives.TryGetValue(pattern, out var classifier))
            {
                // 替换所有可能的占位符
                return ReplaceArchitecturePlaceholders(classifier, architecture, bitness);
            }
        }

        return null;
    }

    /// <summary>
    /// 获取架构家族
    /// </summary>
    private static string GetArchitectureFamily(string architecture)
    {
        return architecture switch
        {
            "x86" or "x64" => "x86",
            "arm" or "arm64" => "arm",
            _ => architecture
        };
    }

    /// <summary>
    /// 替换架构占位符
    /// </summary>
    private static string ReplaceArchitecturePlaceholders(string classifier, string architecture, string bitness)
    {
        if (string.IsNullOrEmpty(classifier)) return classifier;

        return classifier
            .Replace("${arch}", bitness) // 传统占位符，通常指位数
            .Replace("${architecture}", architecture) // 明确架构
            .Replace("${bitness}", bitness) // 明确位数
            .Replace("${platform_arch}", architecture) // 平台架构
            .Replace("${platform_bits}", bitness); // 平台位数
    }

    /// <summary>
    /// 获取所有可用的原生库架构（调试用）
    /// </summary>
    public static List<string> GetAvailableNativeArchitectures(this IEnumerable<Library> libraries, string os)
    {
        var architectures = new HashSet<string>();

        foreach (var library in libraries)
        {
            if (!library.IsApplicable(os) || library.Natives == null)
                continue;

            foreach (var nativeKey in library.Natives.Keys)
            {
                if (nativeKey.StartsWith(os))
                {
                    architectures.Add(nativeKey);
                }
            }
        }

        return architectures.OrderBy(x => x).ToList();
    }
}