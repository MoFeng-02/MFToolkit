using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#pragma warning disable RS1035

namespace MFToolkit.DI.DynamicInjection;

[Generator]
public class DynamicInjectGenerator : ISourceGenerator
{
    private const string attributeDependenciesText = @"
namespace MFToolkit.DI.DynamicInjection;

public enum Dependencies
{
    /// <summary>
    /// 瞬态
    /// </summary>
    Transient,
    /// <summary>
    /// 范围，领域
    /// </summary>
    Scoped,
    /// <summary>
    /// 单例
    /// </summary>
    Singleton
}";
    private const string attributeMainText = @"
using System;
using MFToolkit.DI.DynamicInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MFToolkit.DI.DynamicInject
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [System.Diagnostics.Conditional(""DynamicInjectGenerator_DEBUG"")]
    public class DynamicInjectAttribute : Attribute
    {
        public static IServiceCollection DefaultServices = new ServiceCollection();
        public virtual Dependencies Dependencies { get; set; }
        public Type ServiceType { get; set; }
        public Type ImplementationType { get; set; }
        public DynamicInjectAttribute()
        {
            if (Dependencies == Dependencies.Transient)
            {
                if (ServiceType != null && ImplementationType != null)
                {
                    DefaultServices.AddTransient(ServiceType, ServiceType);
                }
                else if (ServiceType != null)
                {
                    DefaultServices.AddTransient(ServiceType);
                }
            }
            else if (Dependencies == Dependencies.Scoped)
            {
                if (ServiceType != null && ImplementationType != null)
                {
                    DefaultServices.AddScoped(ServiceType, ServiceType);
                }
                else if (ServiceType != null)
                {
                    DefaultServices.AddScoped(ServiceType);
                }
            }
            else if (Dependencies == Dependencies.Singleton)
            {
                if (ServiceType != null && ImplementationType != null)
                {
                    DefaultServices.AddSingleton(ServiceType, ServiceType);
                }
                else if (ServiceType != null)
                {
                    DefaultServices.AddSingleton(ServiceType);
                }
            }
        }
    }
}";
    private const string attributeTransientText = @"
using MFToolkit.DI.DynamicInject;

namespace MFToolkit.DI.DynamicInjection;
public class MFTransientAttribute : DynamicInjectAttribute
{
    public override Dependencies Dependencies { get; set; } = Dependencies.Transient;
}
";
    private const string attributeScopedText = @"
using MFToolkit.DI.DynamicInject;

namespace MFToolkit.DI.DynamicInjection;
public class MFScopedAttribute : DynamicInjectAttribute
{
    public override Dependencies Dependencies { get; set; } = Dependencies.Scoped;
}
";
    private const string attributeSingletonText = @"
using MFToolkit.DI.DynamicInject;

namespace MFToolkit.DI.DynamicInjection;
public class MFSingletonAttribute : DynamicInjectAttribute
{
    public override Dependencies Dependencies { get; set; } = Dependencies.Singleton;
}
";
    public void Execute(GeneratorExecutionContext context)
    {
        // retrieve the populated receiver 
        if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
            return;

    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization((i) =>
        {
            i.AddSource("Dependencies.g.cs", attributeDependenciesText);
            i.AddSource("DynamicInjectAttribute.g.cs", attributeMainText);
            i.AddSource("MFTransientAttribute.g.cs", attributeTransientText);
            i.AddSource("MFScopedAttribute.g.cs", attributeScopedText);
            i.AddSource("MFSingletonAttribute.g.cs", attributeSingletonText);
        });
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    /// <summary>
    /// Created on demand before each generation pass
    /// </summary>
    class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
        /// </summary>
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            // any field with at least one attribute is a candidate for property generation
            if (context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
                && fieldDeclarationSyntax.AttributeLists.Count > 0)
            {
                foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
                {
                    // Get the symbol being declared by the field, and keep it if its annotated
                    IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                    if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "AutoNotify.AutoNotifyAttribute"))
                    {
                        Fields.Add(fieldSymbol);
                    }
                }
            }
        }
    }
}
