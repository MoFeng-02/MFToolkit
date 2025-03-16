
using Avalonia.Android;
using Java.Lang;
using MFToolkit.Avaloniaui.Routes;
using Toast = Android.Widget.Toast;

namespace MFToolkit.Avaloniaui.Android.Override;

/// <summary>
/// 封装的返回操作
/// </summary>
public class BackPressed(AvaloniaMainActivity app)
{
    // 上次点击时间
    private DateTime PrevCheckTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 需要在两秒内返回两次才可退出应用程序，如果有上一页则不会退出只会返回上一页
    /// </summary>
    /// <param name="baseOnBackPressed">基本的返回</param>
    public async Task OnBackPressedTwoAsync(Action? baseOnBackPressed)
    {

        var isPrev = Navigation.GetPrevRouting();
        // 如果还有上一页就返回上一页
        if (isPrev)
        {
            await Navigation.GoToAsync("..");
            return;
        }

        // 没有就检测是否返回了两次退出
        var minusTime = DateTime.UtcNow - PrevCheckTime;

        if (minusTime.Seconds < 2)
        {
            app.Finish();
            try
            {
                baseOnBackPressed?.Invoke();
                Environment.Exit(0);
            }
            catch (InterruptedException e)
            {
                e.PrintStackTrace();
            }
        }
        else
        {
            PrevCheckTime = DateTime.UtcNow;
            Toast.MakeText(app, "再按一次退出程序", ToastLength.Long)?.Show();
        }
        // base.OnBackPressed();
    }
}