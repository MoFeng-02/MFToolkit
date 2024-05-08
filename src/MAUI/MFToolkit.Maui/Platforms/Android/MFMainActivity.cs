using Android.OS;
using Microsoft.Maui.Controls;
namespace MFToolkit.Maui;

// All the code in this file is only included on Android.

public class MFMainActivity : MauiAppCompatActivity
{
#if ANDROID33_0_OR_GREATER
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        OnBackPressed(true, () =>
        {

        });
    }
    void OnBackPressed(bool isEnabled, Action callback)
    {

    }
#else
    
#endif
    public override void OnBackPressed()
    {
        base.OnBackPressed();
    }

}
