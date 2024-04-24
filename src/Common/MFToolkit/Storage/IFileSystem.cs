namespace MFToolkit.Storage;

/// <summary>
/// FileSystem 主要抽象类
/// </summary>
public interface IFileSystem
{
    public string AppDataDirectory { get; }
    public string CacheDirectory { get; }
}