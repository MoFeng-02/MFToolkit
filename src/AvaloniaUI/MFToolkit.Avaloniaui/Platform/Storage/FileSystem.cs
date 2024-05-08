using MFToolkit.Avaloniaui.Platform.Types.Shared;

namespace MFToolkit.Avaloniaui.Platform.Storage;

/// <summary>
/// 提供一种访问设备文件夹位置的简便方法。
/// </summary>
public abstract class FileSystem : IFileSystem
{
    private static IFileSystem? _current;
    public static IFileSystem Current => _current ??= new DefaultFileSystem();

    protected FileSystem()
    {
        _current ??= this;
    }

    public string AppDataDirectory => AppDataDirectoryMethod();
    public string CacheDirectory => CacheDirectoryMethod();

    /// <summary>
    /// 获取可存储应用程序数据的位置。
    /// </summary>
    /// <remarks>这个位置通常对用户是不可见的，并且是备份的。</remarks>
    protected abstract string AppDataDirectoryMethod();

    /// <summary>
    /// 获取可存储临时数据的位置。
    /// </summary>
    /// <remarks>此位置通常对用户不可见，不备份，并且可能随时被操作系统清除。</remarks>
    protected abstract string CacheDirectoryMethod();

    public abstract Task<Stream> OpenAppPackageFileAsync(string fileName);

    public abstract Task<bool> AppPackageFileExistsAsync(string fileName);
}
class DefaultFileSystem : FileSystem
{
    Stream PlatformOpenAppPackageFile(string fileName)
    {
        if (fileName == null)
            throw new ArgumentNullException(nameof(fileName));

        fileName = FileSystemUtils.NormalizePath(fileName);

        try
        {
            var stream = (Stream)File.OpenRead(fileName);
            return stream;
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException(ex.Message, fileName, ex);
        }
    }
    public override Task<bool> AppPackageFileExistsAsync(string fileName) => PlatformAppPackageFileExistsAsync(fileName);
    Task<bool> PlatformAppPackageFileExistsAsync(string fileName)
    {
        try
        {
            using var stream = PlatformOpenAppPackageFile(fileName);
            return Task.FromResult(true);
        }
        catch (FileNotFoundException)
        {
            return Task.FromResult(false);
        }
    }
    public override Task<Stream> OpenAppPackageFileAsync(string fileName) => Task.FromResult(PlatformOpenAppPackageFile(fileName));

    protected override string AppDataDirectoryMethod()
    {
        var thisBaseCacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData");
        if (!Directory.Exists(thisBaseCacheDirectory)) Directory.CreateDirectory(thisBaseCacheDirectory);
        return thisBaseCacheDirectory;
    }

    protected override string CacheDirectoryMethod()
    {
        var thisBaseCacheDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
        if (!Directory.Exists(thisBaseCacheDirectory)) Directory.CreateDirectory(thisBaseCacheDirectory);
        return thisBaseCacheDirectory;
    }
}


public static class FileMimeTypes
{
    public const string All = "*/*";

    public const string ImageAll = "image/*";
    public const string ImagePng = "image/png";
    public const string ImageJpg = "image/jpeg";

    public const string VideoAll = "video/*";

    public const string EmailMessage = "message/rfc822";

    public const string Pdf = "application/pdf";

    public const string TextPlain = "text/plain";

    public const string OctetStream = "application/octet-stream";
}

public static class FileExtensions
{
    public const string Png = ".png";
    public const string Jpg = ".jpg";
    public const string Jpeg = ".jpeg";
    public const string Gif = ".gif";
    public const string Bmp = ".bmp";

    public const string Avi = ".avi";
    public const string Flv = ".flv";
    public const string Gifv = ".gifv";
    public const string Mp4 = ".mp4";
    public const string M4v = ".m4v";
    public const string Mpg = ".mpg";
    public const string Mpeg = ".mpeg";
    public const string Mp2 = ".mp2";
    public const string Mkv = ".mkv";
    public const string Mov = ".mov";
    public const string Qt = ".qt";
    public const string Wmv = ".wmv";

    public const string Pdf = ".pdf";

    public static string[] AllImage =>
        new[] { Png, Jpg, Jpeg, Gif, Bmp };

    public static string[] AllJpeg =>
        new[] { Jpg, Jpeg };

    public static string[] AllVideo =>
        new[] { Mp4, Mov, Avi, Wmv, M4v, Mpg, Mpeg, Mp2, Mkv, Flv, Gifv, Qt };

    public static string Clean(string extension, bool trimLeadingPeriod = false)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        extension = extension.TrimStart('*');
        extension = extension.TrimStart('.');

        if (!trimLeadingPeriod)
            extension = "." + extension;

        return extension;
    }
}

public partial class FileBase
{
    static string PlatformGetContentType(string extension) =>
        throw ExceptionUtils.NotSupportedOrImplementedException;

    protected void Init(FileBase file) =>
        throw ExceptionUtils.NotSupportedOrImplementedException;

    protected virtual Task<Stream> PlatformOpenReadAsync()
        => throw ExceptionUtils.NotSupportedOrImplementedException;

    void PlatformInit(FileBase file)
        => throw ExceptionUtils.NotSupportedOrImplementedException;
}
/// <summary>
/// 文件及其内容类型的表示形式。
/// </summary>
public abstract partial class FileBase
{
    internal const string DefaultContentType = FileMimeTypes.OctetStream;

    string? contentType;

    // 调用者必须至少设置FullPath !!
    internal FileBase()
    {
    }

    internal FileBase(string fullPath)
    {
        if (fullPath == null)
            throw new ArgumentNullException(nameof(fullPath));
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("The file path cannot be an empty string.", nameof(fullPath));
        if (string.IsNullOrWhiteSpace(Path.GetFileName(fullPath)))
            throw new ArgumentException("The file path must be a file path.", nameof(fullPath));

        FullPath = fullPath;
    }

    /// <summary>
    /// 从现有实例初始化类的新实例。 <see cref="FileBase"/> 
    /// </summary>
    /// <param name="file">A <see cref="FileBase"/> instance that will be used to clone.</param>
    public FileBase(FileBase file)
    {
        FullPath = file.FullPath;
        ContentType = file.ContentType;
        FileName = file.FileName;
        PlatformInit(file);
    }

    internal FileBase(string fullPath, string contentType)
        : this(fullPath)
    {
        FullPath = fullPath;
        ContentType = contentType;
    }

    /// <summary>
    /// 获取完整路径和文件名。
    /// </summary>
    public string FullPath { get; internal set; } = null!;

    /// <summary>
    /// 获取或设置文件的内容类型为MIME类型 (e.g.: <c>image/png</c>).
    /// </summary>
    public string ContentType
    {
        get => GetContentType();
        set => contentType = value;
    }

    internal string GetContentType()
    {
        // 尝试提供的类型
        if (!string.IsNullOrWhiteSpace(contentType))
            return contentType!;

        // 尝试从文件扩展名get
        var ext = Path.GetExtension(FullPath);
        if (!string.IsNullOrWhiteSpace(ext))
        {
            var content = PlatformGetContentType(ext);
            if (!string.IsNullOrWhiteSpace(content))
                return content;
        }

        return DefaultContentType;
    }

    string? fileName;

    /// <summary>
    /// 获取或设置此文件的文件名。
    /// </summary>
    public string FileName
    {
        get => GetFileName();
        set => fileName = value;
    }

    internal string GetFileName()
    {
        // try the provided file name
        if (!string.IsNullOrWhiteSpace(fileName))
            return fileName!;

        // try get from the path
        if (!string.IsNullOrWhiteSpace(FullPath))
            return Path.GetFileName(FullPath);

        // this should never happen as the path is validated in the constructor
        throw new InvalidOperationException($"Unable to determine the file name from '{FullPath}'.");
    }

    /// <summary>
    ///打开一个 <see cref="Stream"/> 到文件系统上相应的文件。
    /// </summary>
    /// <returns>一个 <see cref="Stream"/> 包含文件数据。</returns>
    public Task<Stream> OpenReadAsync()
        => PlatformOpenReadAsync();
}

/// <summary>
/// 文件的表示形式(即只读)及其内容类型。
/// </summary>
public class ReadOnlyFile : FileBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyFile"/> class from a file path.
    /// </summary>
    /// <param name="fullPath">Full file path to the corresponding file on the filesystem.</param>
    public ReadOnlyFile(string fullPath)
        : base(fullPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyFile"/> class from a file path, explicitly specifying the content type.
    /// </summary>
    /// <param name="fullPath">Full file path to the corresponding file on the filesystem.</param>
    /// <param name="contentType">Content type (MIME type) of the file (e.g.: <c>image/png</c>).</param>
    public ReadOnlyFile(string fullPath, string contentType)
        : base(fullPath, contentType)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyFile"/> class from an existing instance.
    /// </summary>
    /// <param name="file">A <see cref="FileBase"/> instance that will be used to clone.</param>
    public ReadOnlyFile(FileBase file)
        : base(file)
    {
    }
}

/// <summary>
/// 由用户选择操作产生的文件的表示形式及其内容类型。
/// </summary>
public partial class FileResult : FileBase
{
    // 调用者必须至少设置FullPath !!
    protected FileResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileResult"/> class from a file path.
    /// </summary>
    /// <param name="fullPath">Full file path to the corresponding file on the filesystem.</param>
    public FileResult(string fullPath)
        : base(fullPath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileResult"/> class from a file path, explicitly specifying the content type.
    /// </summary>
    /// <param name="fullPath">Full file path to the corresponding file on the filesystem.</param>
    /// <param name="contentType">Content type (MIME type) of the file (e.g.: <c>image/png</c>).</param>
    public FileResult(string fullPath, string contentType)
        : base(fullPath, contentType)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileResult"/> class from an existing instance.
    /// </summary>
    /// <param name="file">A <see cref="FileBase"/> instance that will be used to clone.</param>
    public FileResult(FileBase file)
        : base(file)
    {
    }
    public string? ContentType { get; set; }

    public string FullPath { get; protected set; } = null!;
}