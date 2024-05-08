namespace MFToolkit.Avaloniaui.Platform.Storage;
public static partial class FileSystemUtils
{
    /// <summary>
    /// 为当前平台规范化给定的文件路径。
    /// </summary>
    /// <param name="filename">要规范化的文件路径。</param>
    /// <returns>
    /// 中提供的文件路径的规范化版本 <paramref name="filename"/>.
    /// 向前和向后斜杠将被 <see cref="Path.DirectorySeparatorChar"/>
    /// 使其对当前平台是正确的。
    /// </returns>
    public static string NormalizePath(string filename) =>
        filename
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
}
