using Avalonia;
#if ANDROID21_0_OR_GREATER
using MFToolkit.Avaloniaui.Android.App;
#endif
#if IOS
using MFToolkit.Avaloniaui.Ios.App;
#elif MACCATALYST
using MFToolkit.Avaloniaui.Maccatalyst.App;
#elif WINDOWS10_0_17763_0_OR_GREATER
using MFToolkit.Avaloniaui.WinUwp.App;
#elif WINDOWS
using MFToolkit.Avaloniaui.Windows.App;
#endif

namespace MFToolkit.Avaloniaui.Platform;
#if WINDOWS10_0_17763_0_OR_GREATER || MACCATALYST
public class MFPlatformBuilder
{
    /// <summary>
    /// 创建应用程序
    /// <para>桌面端共享，已内部实现重载</para>
    /// </summary>
    /// <returns></returns>
    public static AppBuilder CreateApp<TApp>() where TApp : Avalonia.Application, new()
    {
        var app = MFDesktopApp<TApp>.CreateApp();

        return app;
    }
}
#endif
/// <summary>
/// MFToolkit重载的Android/IOS启动程序类
/// <para>拓展了一些基本操作类和方法</para>
/// </summary>
/// <typeparam name="TApp"></typeparam>
public class MFPlatformBuilder<TApp>
#if ANDROID21_0_OR_GREATER
    : MainActivity<TApp> where TApp : Avalonia.Application, new()
#elif IOS
    : AppDelegate<TApp> where TApp : Avalonia.Application, new()
#endif
{

}
