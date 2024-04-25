using Avalonia;
using Avalonia.Android;
using MFToolkit.Avaloniaui.Android.AndroidOverride;
using MFToolkit.Avaloniaui.Android.AppStyle;
using MFToolkit.Avaloniaui.Android.Storage;

namespace MFToolkit.Avaloniaui.Android.App;
/// <summary>
/// MFToolkit重载的Android启动程序类
/// <para>拓展了一些基本操作类和方法</para>
/// </summary>
/// <typeparam name="TApp"></typeparam>
public class MainActivity<TApp> : AvaloniaMainActivity<TApp> where TApp : Avalonia.Application, new()
{
    private BackPressed _backPressed = null!;

    /// <summary>
    /// 是否启用内部重载方法
    /// <para>即在本类中已经重写了AvaloniaMainActivity的一些基础方法，其中包括了OnBackPressed</para>
    /// </summary>
    public bool IsOverride { get; set; } = true;

    public MainActivity() : base()
    {
        FileSystemAndroid fileSystemAndroid = new();
        AndroidAppStatusBar statusBarAndroid = new(Window);

        #region 是否启用内部已默认重写的方法
        if (!IsOverride) return;
        _backPressed = new(this);
        #endregion

    }

    public override async void OnBackPressed()
    {
        if (!IsOverride)
        {
            base.OnBackPressed();
            return;
        }
        await _backPressed.OnBackPressedTwoAsync(base.OnBackPressed);

    }
}
