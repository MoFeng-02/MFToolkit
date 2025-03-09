using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;

namespace DemoDesktop.Language;

public class LangExtension : MarkupExtension
{
    public string Key { get; set; }

    public LangExtension(string key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding
        {
            Source = LocalizationManager.Instance,
            Path = $"[{Key}]",
            Mode = BindingMode.OneWay
        };
        return binding;
    }
}