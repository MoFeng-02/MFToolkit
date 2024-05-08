using Avalonia;

namespace MFToolkit.Avaloniaui.Windows.App;
public class MFDesktopApp<TApp> where TApp : Application, new()
{
    public static AppBuilder CreateApp()
    {
        var app = AppBuilder.Configure<TApp>();
        return app;
    }
}
