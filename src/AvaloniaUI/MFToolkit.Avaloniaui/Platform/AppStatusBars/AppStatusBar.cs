namespace MFToolkit.Avaloniaui.Platform.AppStatusBars;
public abstract class AppStatusBar
{
    private static AppStatusBar? _current;
    public static AppStatusBar Current => _current ??= new AppStatusBarDefault();
    protected AppStatusBar()
    {
        _current ??= this;
    }
    public abstract void SetTopStatusBarColor(string color);
    public abstract void SetTopStatusBarTransparent();
}
