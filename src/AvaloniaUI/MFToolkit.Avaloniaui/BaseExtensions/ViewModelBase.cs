using CommunityToolkit.Mvvm.ComponentModel;

namespace MFToolkit.Avaloniaui.BaseExtensions;

/// <summary>
/// MVVM 拓展基本类
/// </summary>
public class ViewModelBase : ObservableObject, IQueryAttributable
{
    public virtual void ApplyQueryAttributes(IDictionary<string, object?> query)
    {
    }
}