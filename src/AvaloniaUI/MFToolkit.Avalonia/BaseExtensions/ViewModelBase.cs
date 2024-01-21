using CommunityToolkit.Mvvm.ComponentModel;

namespace MFToolkit.Avaloniaui.BaseExtensions;

/// <summary>
/// MVVM 拓展基本类
/// </summary>
public class ViewModelBase : ObservableObject
{
    /// <summary>
    /// 用于获取传输的值
    /// </summary>
    /// <example>
    /// Route: home?title=你好
    /// <para>query: [{"title","你好"}]</para>
    /// </example>
    /// <param name="query"></param>
    public virtual void ApplyQueryAttributes(IDictionary<string, object?> query)
    {
    }
}
