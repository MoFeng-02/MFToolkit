using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MFToolkit.AutoGenerator;

[Generator]
public class AutoInjectGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 输出调试信息
        System.Diagnostics.Debug.WriteLine("AutoInjectGenerator initialized!");
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(c => c.AttributeLists.Count > 0);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        context.RegisterSourceOutput(compilation, (spc, source) =>
        {
            Execute(source.Left, source.Right, spc);
        });
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        var registrations = new List<ServiceRegistration>();
        var attributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.AutoAttribute.AutoInjectAttribute`1");
        var nonGenericAttributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.AutoAttribute.AutoInjectAttribute");

        foreach (var classDecl in classes.Distinct())
        {
            var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

            if (classSymbol == null || classSymbol.IsAbstract || classSymbol.IsStatic) continue;

            var attributes = classSymbol.GetAttributes()
                .Where(a => a.AttributeClass != null &&
                    (a.AttributeClass.OriginalDefinition?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericAttributeSymbol, SymbolEqualityComparer.Default)))
                .ToList();

            foreach (var attribute in attributes)
            {
                var registration = ParseAttribute(classSymbol, attribute);
                if (registration != null)
                {
                    registrations.Add(registration);
                }
            }
        }

        GenerateSource(context, registrations);
    }

    private static ServiceRegistration? ParseAttribute(INamedTypeSymbol classSymbol, AttributeData attribute)
    {
        var serviceType = GetServiceType(attribute);
        var serviceKey = GetServiceKey(attribute);
        var lifetime = GetLifetime(attribute);

        if (serviceType == null && attribute.AttributeClass?.IsGenericType == true)
        {
            serviceType = attribute.AttributeClass.TypeArguments[0];
        }

        if (serviceType == null)
        {
            serviceType = classSymbol;
        }

        return new ServiceRegistration(
            serviceType,
             classSymbol,
             serviceKey,
             lifetime
        );
    }

    private static ITypeSymbol? GetServiceType(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is ITypeSymbol typeSymbol)
        {
            return typeSymbol;
        }
        return null;
    }

    private static string? GetServiceKey(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 1 &&
            attribute.ConstructorArguments[1].Value is string key)
        {
            return key;
        }
        return null;
    }

    private static Lifetime GetLifetime(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 2 &&
            attribute.ConstructorArguments[2].Value is int lifetime)
        {
            return (Lifetime)lifetime;
        }
        return Lifetime.Transient;
    }

    private static void GenerateSource(SourceProductionContext context, IEnumerable<ServiceRegistration> registrations)
    {
        var source = $@"
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

public static class AutoInjectExtensions
{{
    [CompilerGenerated]
    public static IServiceCollection AddAutoInjectServices(this IServiceCollection services)
    {{
        {BuildRegistrationCode(registrations)}
        return services;
    }}
}}";

        context.AddSource("AutoInject.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string BuildRegistrationCode(IEnumerable<ServiceRegistration> registrations)
    {
        var sb = new StringBuilder();

        foreach (var reg in registrations)
        {
            var serviceType = reg.ServiceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var implementationType = reg.ImplementationType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var isGeneric = reg.ServiceType is INamedTypeSymbol { IsGenericType: true };

            var method = reg.Lifetime switch
            {
                Lifetime.Singleton => "AddSingleton",
                Lifetime.Scoped => "AddScoped",
                _ => "AddTransient"
            };

            if (!string.IsNullOrEmpty(reg.ServiceKey))
            {
                method = $"AddKeyed{method}";
                sb.AppendLine(
                    $@"services.{method}<{serviceType}>(""{reg.ServiceKey}"", typeof({implementationType}));");
            }
            else if (isGeneric)
            {
                sb.AppendLine(
                    $@"services.{method}(typeof({serviceType}), typeof({implementationType}));");
            }
            else if (serviceType != implementationType)
            {
                sb.AppendLine(
                    $@"services.{method}<{serviceType}, {implementationType}>();");
            }
            else
            {
                sb.AppendLine(
                    $@"services.{method}<{serviceType}>();");
            }
        }

        return sb.ToString();
    }

    private class ServiceRegistration(
        ITypeSymbol serviceType,
        INamedTypeSymbol implementationType,
        string? serviceKey,
        Lifetime lifetime
    )
    {
        public ITypeSymbol ServiceType { get; set; } = serviceType;
        public INamedTypeSymbol ImplementationType { get; set; } = implementationType;
        public string? ServiceKey { get; set; } = serviceKey;
        public Lifetime Lifetime { get; set; } = lifetime;
    }
}