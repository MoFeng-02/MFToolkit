using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Avalonia.Threading;

namespace DemoDesktop.Language;

public class LocalizationManager : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationManager> _instance =
        new(() => new LocalizationManager());

    private ResourceManager _resourceManager;
    public event PropertyChangedEventHandler? PropertyChanged;

    public static LocalizationManager Instance => _instance.Value;

    private LocalizationManager()
    {
        var name = typeof(AppLang).FullName!;
        // 指向你的资源文件基名（不包含语言后缀）
        _resourceManager = new ResourceManager(name, typeof(LocalizationManager).Assembly);
    }

    public string this[string key] =>
        _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? $"#{key}#";

    public void SwitchLanguage(CultureInfo culture)
    {
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;

        Dispatcher.UIThread.Post(() =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        });
    }
}