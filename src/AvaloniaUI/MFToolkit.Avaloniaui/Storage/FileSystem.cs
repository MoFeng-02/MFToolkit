namespace MFToolkit.Avaloniaui.Storage;

/// <summary>
/// 文件系统-App文件系统
/// </summary>
public class FileSystem : IFileSystem
{
    private static IFileSystem? _current;
    public static IFileSystem Current => _current ??= new FileSystem();

    protected FileSystem()
    {
        _current ??= this;
    }

    public string AppDataDirectory => AppDataDirectoryMethod();
    public string CacheDirectory => CacheDirectoryMethod();

    /// <summary>
    /// 获取自身应用主要数据存储位置
    /// </summary>
    /// <returns></returns>
    protected virtual string AppDataDirectoryMethod()
    {
        var thisBaseCacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData");
        if (!Directory.Exists(thisBaseCacheDirectory)) Directory.CreateDirectory(thisBaseCacheDirectory);
        return thisBaseCacheDirectory;
    }

    /// <summary>
    /// 获取Cache文件夹
    /// </summary>
    /// <returns></returns>
    protected virtual string CacheDirectoryMethod()
    {
        var thisBaseCacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
        if (!Directory.Exists(thisBaseCacheDirectory)) Directory.CreateDirectory(thisBaseCacheDirectory);
        return thisBaseCacheDirectory;
    }
}