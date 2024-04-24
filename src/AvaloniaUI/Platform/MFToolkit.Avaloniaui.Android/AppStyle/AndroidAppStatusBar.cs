using Android.Graphics;
using Android.OS;
using Android.Views;
using MFToolkit.Avaloniaui.Helpers;
using MFToolkit.Avaloniaui.Platform.AppStatusBars;

namespace MFToolkit.Avaloniaui.Android.AppStyle;

public class AndroidAppStatusBar(Window? window) : AppStatusBar
{

  public override void SetTopStatusBarColor(string color)
    {
        if (window == null) return;
        if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop) return;
        window.ClearFlags(WindowManagerFlags.TranslucentStatus);
        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
        var x16Color = ColorHelper.GenerateColorX8(color);
        window.SetStatusBarColor(Color.ParseColor(x16Color));
    }

    public override void SetTopStatusBarTransparent()
    {
        if (window == null) return;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
        {
            window.AddFlags(WindowManagerFlags.TranslucentStatus);
            return;
        }

        if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop) return;
        window.ClearFlags(WindowManagerFlags.TranslucentStatus);
        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
        window.SetStatusBarColor(Color.Transparent);
    }
}