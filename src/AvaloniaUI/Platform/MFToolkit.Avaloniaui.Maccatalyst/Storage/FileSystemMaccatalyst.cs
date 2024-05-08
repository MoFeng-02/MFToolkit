using MFToolkit.Avaloniaui.Platform.Storage;

namespace MFToolkit.Avaloniaui.Maccatalyst.Storage;
public class FileSystemMaccatalyst : FileSystem
{
    protected override string AppDataDirectoryMethod() => GetDirectory(NSSearchPathDirectory.LibraryDirectory);

    protected override string CacheDirectoryMethod() => GetDirectory(NSSearchPathDirectory.CachesDirectory);

    public static string GetDirectory(NSSearchPathDirectory directory)
    {
        var dirs = NSSearchPath.GetDirectories(directory, NSSearchPathDomain.User);
        if (dirs == null || dirs.Length == 0)
        {
            // this should never happen...
            return null!;
        }
        return dirs[0];
    }

    public override Task<Stream> OpenAppPackageFileAsync(string fileName)
    {
        var file = FileSystemUtils.PlatformGetFullAppPackageFilePath(fileName);
        return Task.FromResult((Stream)File.OpenRead(file));
    }

    public override Task<bool> AppPackageFileExistsAsync(string fileName)
    {
        var file = FileSystemUtils.PlatformGetFullAppPackageFilePath(fileName);
        return Task.FromResult(File.Exists(file));
    }
}
public static class FileSystemUtils
{
    public static string PlatformGetFullAppPackageFilePath(string filename)
    {
        if (filename == null)
            throw new ArgumentNullException(nameof(filename));

        filename = MFToolkit.Avaloniaui.Platform.Storage.FileSystemUtils.NormalizePath(filename);

        var root = NSBundle.MainBundle.BundlePath;
#if MACCATALYST || MACOS
        root = Path.Combine(root, "Contents", "Resources");
#endif
        return Path.Combine(root, filename);
    }

    public static string GetDirectory(NSSearchPathDirectory directory)
    {
        var dirs = NSSearchPath.GetDirectories(directory, NSSearchPathDomain.User);
        if (dirs == null || dirs.Length == 0)
        {
            // this should never happen...
            return null!;
        }
        return dirs[0];
    }
}