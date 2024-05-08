using MFToolkit.Avaloniaui.Platform.Storage;

namespace MFToolkit.Avaloniaui.Android.Storage;
public class FileSystemAndroid : FileSystem
{
    public override Task<bool> AppPackageFileExistsAsync(string fileName)
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

    Stream PlatformOpenAppPackageFile(string filename)
    {
        if (filename == null)
            throw new ArgumentNullException(nameof(filename));

        filename = FileSystemUtils.NormalizePath(filename);

        try
        {
            return Application.Context.Assets!.Open(filename);
        }
        catch (Java.IO.FileNotFoundException ex)
        {
            throw new FileNotFoundException(ex.Message, filename, ex);
        }
    }

    protected override string AppDataDirectoryMethod() =>
        Application.Context.FilesDir?.AbsolutePath ?? throw new("路径值为空");

    protected override string CacheDirectoryMethod() =>
        Application.Context.CacheDir?.AbsolutePath ?? throw new("路径值为空");
}