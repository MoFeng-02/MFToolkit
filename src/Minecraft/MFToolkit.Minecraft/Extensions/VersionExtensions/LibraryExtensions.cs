// Library.Extensions.cs

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
        if (string.IsNullOrEmpty(library.Name))
            throw new ArgumentException("Library name is required");

        var parts = library.Name.Split(':');
        if (parts.Length < 3)
            throw new ArgumentException($"Invalid library name format: {library.Name}");

        var groupId = parts[0].Replace('.', '/');
        var artifactId = parts[1];
        var version = parts[2];

        string? classifier = null;
        string extension = "jar";

        // 处理分类器和扩展名
        if (parts.Length >= 4)
        {
            classifier = ParseClassifierAndExtension(parts[3], ref extension);
        }

        if (parts.Length >= 5)
        {
            extension = parts[4];
        }

        var fileName = BuildFileName(artifactId, version, classifier, extension);
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
}