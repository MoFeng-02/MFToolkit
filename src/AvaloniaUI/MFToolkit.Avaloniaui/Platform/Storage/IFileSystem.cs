namespace MFToolkit.Avaloniaui.Platform.Storage;

/// <summary>
/// 提供一种访问设备文件夹位置的简便方法。
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// 获取可存储应用程序数据的位置。
    /// </summary>
    /// <remarks>这个位置通常对用户是不可见的，并且是备份的。</remarks>
    public string AppDataDirectory { get; }
    /// <summary>
    /// 获取可存储临时数据的位置。
    /// </summary>
    /// <remarks>此位置通常对用户不可见，不备份，并且可能随时被操作系统清除。</remarks>
    public string CacheDirectory { get; }

    /// <summary>
    /// 打开一个流到app包中包含的文件。
    /// </summary>
    /// <param name="fileName">要从应用程序包中加载的文件名(不包括路径)。</param>
    /// <returns>一个 <see cref="Stream"/>包含(只读)文件数据。</returns>
    Task<Stream> OpenAppPackageFileAsync(string fileName);

    /// <summary>
    /// 确定某个文件是否存在于应用程序包中。
    /// </summary>
    /// <param name="fileName">要从应用程序包中加载的文件名(不包括路径)。</param>
    /// <returns><see langword="true"/> 当指定的文件存在于app包中时，否则 <see langword="false"/>.</returns>
    Task<bool> AppPackageFileExistsAsync(string fileName);
}