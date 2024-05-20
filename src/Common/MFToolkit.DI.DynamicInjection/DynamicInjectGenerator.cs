using Microsoft.CodeAnalysis;

#pragma warning disable RS1035

namespace MFToolkit.DI.DynamicInjection;

[Generator]
public class DynamicInjectGenerator : ISourceGenerator
{

    public void Execute(GeneratorExecutionContext context)
    {
        // Find the main method
        var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);
        var attributeText = @"
using System;
namespace MFToolkit.DI.DynamicInject
{
    public sealed class DynamicInjectAttribute : Attribute
    {
        
    }
}
";

    }

    public void Initialize(GeneratorInitializationContext context)
    {

    }
}
