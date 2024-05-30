using MFToolkit.DI.DynamicInject;

namespace MFToolkit.DI.DynamicInjection;
public class MFScopedAttribute : DynamicInjectAttribute
{
    public override Dependencies Dependencies { get; set; } = Dependencies.Scoped;
}
