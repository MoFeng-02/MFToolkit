using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MFToolkit.AutoGenerator.DependencyInjection;

/// <summary>
/// 自动注入生成器
/// </summary>
[Generator(LanguageNames.CSharp)]
public class AutoInjectGenerator : IIncrementalGenerator
{
    /// <summary>
    /// 初始化生成器
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(c => c.AttributeLists.Count > 0);

        var compilation = context.CompilationProvider.Combine(provider.Collect());
        context.RegisterSourceOutput(compilation, (spc, source) =>
        {
            var assName = source.Left.AssemblyName;
            Execute(source.Left, source.Right, spc, assName);
        });
    }

    private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context, string? AssemblyName)
    {
        if (classes.IsDefaultOrEmpty) return;

        var registrations = new List<ServiceRegistration>();
        // 原有特性Symbol
        var attributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.AutoInjectAttribute`1");
        var nonGenericAttributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.AutoInjectAttribute");
        var tryAttributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.AutoTryInjectAttribute`1");
        var tryNonGenericAttributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.AutoTryInjectAttribute");

        // 新增：单独生命周期特性Symbol（泛型+非泛型）
        // 普通注入系列
        var singletonAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.SingletonAttribute`1");
        var nonGenericSingletonAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.SingletonAttribute");
        var scopedAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.ScopedAttribute`1");
        var nonGenericScopedAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.ScopedAttribute");
        var transientAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.TransientAttribute`1");
        var nonGenericTransientAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.TransientAttribute");

        // Try注入系列
        var trySingletonAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.TrySingletonAttribute`1");
        var nonGenericTrySingletonAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.TrySingletonAttribute");
        var tryScopedAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.TryScopedAttribute`1");
        var nonGenericTryScopedAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.TryScopedAttribute");
        var tryTransientAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.TryTransientAttribute`1");
        var nonGenericTryTransientAttrSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.TryTransientAttribute");

        var nameSpaceAttributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.Abstractions.DependencyInjection.AutoInjectServiceNameAttribute");
        string? serviceName = null;

        foreach (var classDecl in classes.Distinct())
        {
            var model = compilation.GetSemanticModel(classDecl.SyntaxTree);

            if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol || classSymbol.IsAbstract || classSymbol.IsStatic) continue;

            // 扩展属性筛选，包含新的生命周期特性
            var attributes = classSymbol.GetAttributes()
                .Where(a => a.AttributeClass != null &&
                    (a.AttributeClass.OriginalDefinition?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericAttributeSymbol, SymbolEqualityComparer.Default) ||
                     a.AttributeClass.OriginalDefinition?.Equals(tryAttributeSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(tryNonGenericAttributeSymbol, SymbolEqualityComparer.Default) ||
                     // 新增：单独生命周期特性判断
                     a.AttributeClass.OriginalDefinition?.Equals(singletonAttrSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericSingletonAttrSymbol, SymbolEqualityComparer.Default) ||
                     a.AttributeClass.OriginalDefinition?.Equals(scopedAttrSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericScopedAttrSymbol, SymbolEqualityComparer.Default) ||
                     a.AttributeClass.OriginalDefinition?.Equals(transientAttrSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericTransientAttrSymbol, SymbolEqualityComparer.Default) ||
                     a.AttributeClass.OriginalDefinition?.Equals(trySingletonAttrSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericTrySingletonAttrSymbol, SymbolEqualityComparer.Default) ||
                     a.AttributeClass.OriginalDefinition?.Equals(tryScopedAttrSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericTryScopedAttrSymbol, SymbolEqualityComparer.Default) ||
                     a.AttributeClass.OriginalDefinition?.Equals(tryTransientAttrSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericTryTransientAttrSymbol, SymbolEqualityComparer.Default)))
                .ToList();

            var namespaceAttributes = classSymbol.GetAttributes()
                .Where(a => a.AttributeClass != null &&
                    a.AttributeClass.Equals(nameSpaceAttributeSymbol, SymbolEqualityComparer.Default))
                .FirstOrDefault();

            if (namespaceAttributes != null)
            {
                if (classSymbol.ContainingNamespace.ToDisplayString().Contains(AssemblyName))
                {
                    serviceName = namespaceAttributes.ConstructorArguments[0].Value?.ToString();
                }
            }

            foreach (var attribute in attributes)
            {
                var registration = ParseAttribute(classSymbol, attribute, AssemblyName);
                if (registration != null)
                {
                    registrations.Add(registration);
                }
            }
        }

        GenerateSource(context, registrations, serviceName);
    }


    private ServiceRegistration? ParseAttribute(INamedTypeSymbol classSymbol, AttributeData attribute, string? nameSpace)
    {
        // 优先从泛型属性中获取服务类型
        ITypeSymbol? serviceType = null;
        var attributeClassName = attribute.AttributeClass?.Name;

        // 新增：判断是否为单独生命周期特性
        bool isLifetimeAttribute = IsLifetimeAttribute(attributeClassName);
        // 新增：从特性名称判断是否为Try注入
        bool isTryInject = attributeClassName?.StartsWith("Try") == true ||
                          attributeClassName == "AutoTryInjectAttribute";

        // 处理泛型属性（包括新的生命周期泛型特性）
        if (attribute.AttributeClass?.IsGenericType == true)
        {
            serviceType = attribute.AttributeClass.TypeArguments[0];
        }
        else
        {
            // 处理非泛型属性中的Type参数
            serviceType = GetServiceType(attribute);
        }

        // 默认回退到实现类型
        serviceType ??= classSymbol;

        var serviceKey = GetServiceKey(attribute);
        // 新增：从单独生命周期特性获取生命周期
        var lifetime = isLifetimeAttribute
            ? GetLifetimeFromAttributeName(attributeClassName)
            : GetLifetime(attribute);

        string targetNamespace = nameSpace != null ? nameSpace + ".DependencyInjection.AutoGenerated" : GetTargetNamespace(classSymbol.ContainingNamespace.ToDisplayString());

        return new ServiceRegistration(
            serviceType,
            classSymbol,
            serviceKey,
            lifetime,
            targetNamespace,
            isTryInject
        );
    }

    // 新增：判断是否为单独生命周期特性
    private bool IsLifetimeAttribute(string? attributeClassName)
    {
        if (string.IsNullOrEmpty(attributeClassName)) return false;

        return attributeClassName is "SingletonAttribute" or "ScopedAttribute" or "TransientAttribute" or
               "TrySingletonAttribute" or "TryScopedAttribute" or "TryTransientAttribute";
    }

    // 新增：从特性名称获取生命周期
    private Lifetime GetLifetimeFromAttributeName(string? attributeClassName)
    {
        if (string.IsNullOrEmpty(attributeClassName))
            return Lifetime.Transient;

        if (attributeClassName.Contains("Singleton"))
            return Lifetime.Singleton;
        if (attributeClassName.Contains("Scoped"))
            return Lifetime.Scoped;
        if (attributeClassName.Contains("Transient"))
            return Lifetime.Transient;

        return Lifetime.Transient;
    }

    private static string GetTargetNamespace(string originalNamespace)
    {
        var parts = originalNamespace.Split('.');
        if (parts.Length == 0) return "MFToolkit.AutoGenerated";

        var parentNamespace = parts[0];
        return string.IsNullOrEmpty(parentNamespace)
            ? "MFToolkit.AutoGenerated"
            : $"{parentNamespace}.DependencyInjection.AutoGenerated";
    }


    private static ITypeSymbol? GetServiceType(AttributeData attribute)
    {
        foreach (var arg in attribute.ConstructorArguments)
        {
            if (arg.Value is ITypeSymbol typeSymbol)
            {
                return typeSymbol;
            }
        }
        return null;
    }


    private static string? GetServiceKey(AttributeData attribute)
    {
        foreach (var arg in attribute.ConstructorArguments)
        {
            if (arg.Value is string key)
            {
                return key;
            }
        }
        return null;
    }


    private Lifetime GetLifetime(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 0)
        {
            for (int i = 0; i < attribute.ConstructorArguments.Length; i++)
            {
                if (attribute.ConstructorArguments[i].Value is int lifetime)
                {
                    return (Lifetime)lifetime;
                }
            }
        }
        return Lifetime.Transient;
    }

    private void GenerateSource(
    SourceProductionContext context,
    IEnumerable<ServiceRegistration> registrations,
    string? serviceName
)
    {
        var groupedRegistrations = registrations
            .GroupBy(r => r.TargetNamespace)
            .ToList();

        foreach (var group in groupedRegistrations)
        {
            string targetNamespace = group.Key;
            var registrationsInGroup = group.ToList();

            var source = $@"
// <auto-generated/>

// Bug Issue: https://github.com/MoFeng-02/MFToolkit/issues

namespace {targetNamespace}
{{
    public static partial class AutoInjectExtensions
    {{

        /// <summary>
        /// 本类为生成器生成，用于自动注入服务
        /// </summary>
        /// <param name=""services"">服务注入</param>
        /// <returns>本类中所注入的服务</returns>
        [global::System.Runtime.CompilerServices.CompilerGenerated]
        public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection {serviceName ?? "AddAutoInjectServices"}(this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
        {{

            {BuildRegistrationCode(registrationsInGroup).Replace("\n", "\n            ")}
            return services;
        }}
    }}
}}";

            string fileName = $"{targetNamespace}.AutoInject.g.cs";
            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }
    }


    private static string BuildRegistrationCode(IEnumerable<ServiceRegistration> registrations)
    {
        var sb = new StringBuilder();
        var currentLifetime = (Lifetime)(-1);

        foreach (var reg in registrations.OrderBy(r => r.Lifetime).ThenBy(r => r.IsTry))
        {
            if (currentLifetime != reg.Lifetime)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.AppendLine($"// {reg.Lifetime} 服务注册");
                currentLifetime = reg.Lifetime;
            }

            var serviceType = reg.ServiceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var implementationType = reg.ImplementationType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var isGeneric = reg.ServiceType is INamedTypeSymbol { IsGenericType: true };

            var method = reg.Lifetime switch
            {
                Lifetime.Singleton => "AddSingleton",
                Lifetime.Scoped => "AddScoped",
                _ => "AddTransient"
            };

            var keyMethod = reg.Lifetime switch
            {
                Lifetime.Singleton => "AddKeyedSingleton",
                Lifetime.Scoped => "AddKeyedScoped",
                _ => "AddKeyedTransient"
            };

            var tryMethod = reg.IsTry
                ? reg.Lifetime switch
                {
                    Lifetime.Singleton => "TryAddSingleton",
                    Lifetime.Scoped => "TryAddScoped",
                    _ => "TryAddTransient"
                }
                : method;
            var tryKeyMethod = reg.IsTry
                ? reg.Lifetime switch
                {
                    Lifetime.Singleton => "TryAddKeyedSingleton",
                    Lifetime.Scoped => "TryAddKeyedScoped",
                    _ => "TryAddKeyedTransient"
                } : keyMethod;

            var line = !string.IsNullOrEmpty(reg.ServiceKey)
                ? reg.IsTry
                    ? BuildTryKeyedServiceLine(tryKeyMethod, serviceType, implementationType, reg.ServiceKey)
                    : BuildKeyedServiceLine(keyMethod, serviceType, implementationType, reg.ServiceKey)
                : reg.IsTry
                    ? BuildNormalTryServiceLine(tryMethod, serviceType, implementationType, isGeneric)
                    : BuildNormalServiceLine(method, serviceType, implementationType, isGeneric);

            sb.AppendLine(line);
        }
        return sb.ToString();
    }

    private static string BuildKeyedServiceLine(string method, string service, string impl, string? key)
    {
        var str = !string.IsNullOrWhiteSpace(impl) && service != impl
            ? $"global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.{method}<{service}, {impl}>(services, \"{key}\")"
            : $"global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.{method}<{service}>(services, \"{key}\")";
        return str + ";";
    }

    private static string BuildTryKeyedServiceLine(string method, string service, string impl, string? key)
    {
        var str = !string.IsNullOrWhiteSpace(impl) && service != impl
            ? $"global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.{method}<{service}, {impl}>(services, \"{key}\")"
            : $"global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.{method}<{service}>(services, \"{key}\")";
        return str + ";";
    }

    private static string BuildNormalServiceLine(string method, string service, string impl, bool isGeneric)
    {
        var str = isGeneric
            ? $"global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.{method}(services, typeof({service}), typeof({impl}))"
            : service != impl
                ? $"global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.{method}<{service}, {impl}>(services)"
                : $"global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.{method}<{service}>(services)";
        return str + ";";
    }

    private static string BuildNormalTryServiceLine(string method, string service, string impl, bool isGeneric)
    {
        var str = isGeneric
            ? $"global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.{method}(services, typeof({service}), typeof({impl}))"
            : service != impl
                ? $"global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.{method}<{service}, {impl}>(services)"
                : $"global::Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.{method}<{service}>(services)";
        return str + ";";
    }

    private class ServiceRegistration(
        ITypeSymbol serviceType,
        INamedTypeSymbol implementationType,
        string? serviceKey,
        Lifetime lifetime,
        string targetNamespace,
        bool isTry = false,
        string? debugInfo = null
    )
    {
        public ITypeSymbol ServiceType { get; set; } = serviceType;
        public INamedTypeSymbol ImplementationType { get; set; } = implementationType;
        public string? ServiceKey { get; set; } = serviceKey;
        public Lifetime Lifetime { get; set; } = lifetime;
        public string TargetNamespace { get; } = targetNamespace;
        public bool IsTry { get; set; } = isTry;
        public string? DebugInfo { get; set; } = debugInfo;
    }
}
