namespace MFToolkit.Platform.AppStatusBars;

/// <summary>
/// App 端的状态栏即顶部的状态类
/// </summary>
public abstract class AppStatusBar
{
    private static AppStatusBar? _current;
    public static AppStatusBar Current => _current ??= new AppStatusBarDefault();

    protected AppStatusBar()
    {
        _current ??= this;
    }

    /// <summary>
    /// 设置顶部栏的颜色
    /// </summary>
    /// <param name="color"></param>
    public abstract void SetTopStatusBarColor(string color);

    /// <summary>
    /// 设置顶部状态栏透明
    /// </summary>
    public abstract void SetTopStatusBarTransparent();
}