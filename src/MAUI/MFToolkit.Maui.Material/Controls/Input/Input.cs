#nullable disable
namespace MFToolkit.Maui.Material.Controls;

[ContentProperty(nameof(Content))]
public partial class Input : Grid
{

    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(Input), string.Empty, defaultBindingMode: BindingMode.TwoWay);
    /// <summary>
    /// 输入内容
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    private IView content;
    public virtual IView Content
    {
        get => content; set
        {
            content = value;
        }
    }
}
