using Avalonia;
using MFToolkit.Avaloniaui.Maccatalyst.Storage;

namespace MFToolkit.Avaloniaui.Maccatalyst.App;
public class MFDesktopApp<TApp> where TApp : Application, new()
{
    public static AppBuilder CreateApp()
    {
        var app = AppBuilder.Configure<TApp>();
        _ = new FileSystemMaccatalyst();
        return app;
    }
}
