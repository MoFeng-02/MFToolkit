using Android.App;
using MFToolkit.Avaloniaui.Storage;

namespace MFToolkit.Avaloniaui.Android.Storage;
public class FileSystemAndroid : FileSystem
{
    protected override string AppDataDirectoryMethod() =>
        Application.Context.FilesDir?.AbsolutePath ?? base.AppDataDirectoryMethod();

    protected override string CacheDirectoryMethod() =>
        Application.Context.CacheDir?.AbsolutePath ?? base.CacheDirectoryMethod();
}