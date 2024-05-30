using MFToolkit.DI.DynamicInject;

namespace MFToolkit.DI.DynamicInjection;
public class MFTransientAttribute : DynamicInjectAttribute
{
    public override Dependencies Dependencies { get; set; } = Dependencies.Transient;
}
