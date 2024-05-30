using MFToolkit.DI.DynamicInject;

namespace MFToolkit.DI.DynamicInjection;
public class MFSingletonAttribute : DynamicInjectAttribute
{
    public override Dependencies Dependencies { get; set; } = Dependencies.Singleton;
}
