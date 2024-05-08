using Avalonia;
using MFToolkit.Avaloniaui.WinUwp.Storage;

namespace MFToolkit.Avaloniaui.WinUwp.App;
public class MFDesktopApp<TApp> where TApp : Application, new()
{
    public static AppBuilder CreateApp()
    {
        var app = AppBuilder.Configure<TApp>();
        _ = new FileSystemUwp();
        return app;
    }
}
