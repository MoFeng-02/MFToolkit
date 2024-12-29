using System;
using System.Linq;
using System.Text;
using MFToolkit.DI.DynamicInjection.Enumerate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MFToolkit.DI.DynamicInjection.Generators;

[Generator]
public class MFInjectableSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // 初始化逻辑可以放在这里
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var registrations = new StringBuilder();
        registrations.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        registrations.AppendLine("public static class MFInjectableRegistrations");
        registrations.AppendLine("{");
        registrations.AppendLine("    public static IServiceCollection AddMFInjectables(this IServiceCollection services)");
        registrations.AppendLine("    {");

        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot(context.CancellationToken);

            var attributeType = context.Compilation.GetTypeByMetadataName("MFToolkit.DI.DynamicInjection.Attributes.MFInjectableAttribute");

            if (attributeType != null)
            {
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classDeclaration in classDeclarations)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                    if (symbol != null && HasMFInjectableAttribute(symbol, attributeType))
                    {
                        GenerateRegistrationCode(symbol, registrations, context);
                    }
                }
            }
        }

        registrations.AppendLine("        return services;");
        registrations.AppendLine("    }");
        registrations.AppendLine("}");

        context.AddSource("MFInjectableRegistrations.g.cs", SourceText.From(registrations.ToString(), Encoding.UTF8));
    }

    private bool HasMFInjectableAttribute(ISymbol symbol, INamedTypeSymbol attributeType)
    {
        return symbol.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));
    }

    private void GenerateRegistrationCode(ISymbol symbol, StringBuilder registrations, GeneratorExecutionContext context)
    {
        var attributes = symbol.GetAttributes();
        foreach (var attr in attributes)
        {
            if (attr.AttributeClass?.ToString() == "MFToolkit.DI.DynamicInjection.Attributes.MFInjectableAttribute")
            {
                Dependencies dependencies = attr.NamedArguments.Length > 0
                    ? (Dependencies)attr.NamedArguments.FirstOrDefault(x => x.Key == "Dependencies").Value.Value
                    : Dependencies.Transient;

                // 使用INamedTypeSymbol来处理服务类型和服务实现类型
                INamedTypeSymbol? serviceType = symbol as INamedTypeSymbol;
                INamedTypeSymbol? implementationType = null;

                if (attr.NamedArguments.Any(x => x.Key == "ImplementationType"))
                {
                    // 如果提供了ImplementationType，则获取其值并转换为INamedTypeSymbol
                    implementationType = (INamedTypeSymbol)attr.NamedArguments.First(x => x.Key == "ImplementationType").Value.Value!;
                }
                else if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.TypeParameters.Any())
                {
                    // 对于泛型类，如果没有指定ImplementationType，则默认使用第一个类型参数
                    implementationType = namedTypeSymbol.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
                }

                string registrationMethod = dependencies switch
                {
                    Dependencies.Singleton => "AddSingleton",
                    Dependencies.Scoped => "AddScoped",
                    _ => "AddTransient"
                };

                // 构建服务注册代码
                if (implementationType != null && !object.ReferenceEquals(serviceType, implementationType))
                {
                    registrations.AppendLine($"        services.{registrationMethod}<{serviceType}, {implementationType}>();");
                }
                else
                {
                    registrations.AppendLine($"        services.{registrationMethod}<{serviceType}>();");
                }
            }
        }
    }
}