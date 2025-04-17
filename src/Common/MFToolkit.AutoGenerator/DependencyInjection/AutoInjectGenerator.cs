﻿using System.Collections.Immutable;
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
            var assName = source.Left.AssemblyName;
            Execute(source.Left, source.Right, spc, assName);
        });
    }

    private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context, string? AssemblyName)
    {
        if (classes.IsDefaultOrEmpty) return;

        var registrations = new List<ServiceRegistration>();
        var attributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.AutoAttribute.DependencyInjection.AutoInjectAttribute`1");
        var nonGenericAttributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.AutoAttribute.DependencyInjection.AutoInjectAttribute");
        var nameSpaceAttributeSymbol = compilation.GetTypeByMetadataName("MFToolkit.AutoAttribute.DependencyInjection.AutoInjectServiceNameAttribute");

        string? serviceName = null;
        foreach (var classDecl in classes.Distinct())
        {
            var model = compilation.GetSemanticModel(classDecl.SyntaxTree);

            if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol || classSymbol.IsAbstract || classSymbol.IsStatic) continue;

            var attributes = classSymbol.GetAttributes()
                .Where(a => a.AttributeClass != null &&
                    (a.AttributeClass.OriginalDefinition?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true ||
                     a.AttributeClass.Equals(nonGenericAttributeSymbol, SymbolEqualityComparer.Default)))
                .ToList();

            var namespaceAttributes = classSymbol.GetAttributes()
                .Where(a => a.AttributeClass != null &&
                    a.AttributeClass.Equals(nameSpaceAttributeSymbol, SymbolEqualityComparer.Default))
                .FirstOrDefault();

            if (namespaceAttributes != null)
            {
                if (classSymbol.ContainingNamespace.ToDisplayString().Contains(AssemblyName))
                {
                    // 获取命名空间属性和服务注入名称
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

    //private static ServiceRegistration? ParseAttribute(INamedTypeSymbol classSymbol, AttributeData attribute)
    //{
    //    var serviceType = GetServiceType(attribute) ?? classSymbol; // 默认为实现类型
    //    var serviceKey = GetServiceKey(attribute);
    //    var lifetime = GetLifetime(attribute);

    //    if (serviceType == null && attribute.AttributeClass?.IsGenericType == true)
    //    {
    //        serviceType = attribute.AttributeClass.TypeArguments[0];
    //    }

    //    if (serviceType == null)
    //    {
    //        serviceType = classSymbol;
    //    }

    //    // 计算目标命名空间
    //    string originalNamespace = classSymbol.ContainingNamespace.ToDisplayString(); // 如 "Demo.DemoModel"
    //    string targetNamespace = GetTargetNamespace(originalNamespace); // 转换为 "Demo.AutoGenerated"

    //    return new ServiceRegistration(
    //        serviceType ?? classSymbol,
    //        classSymbol,
    //        serviceKey,
    //        lifetime,
    //        targetNamespace
    //    );
    //}

    private ServiceRegistration? ParseAttribute(INamedTypeSymbol classSymbol, AttributeData attribute, string? nameSpace)
    {
        // 优先从泛型属性中获取服务类型
        ITypeSymbol? serviceType = null;

        // 处理泛型 AutoInjectAttribute<T>
        if (attribute.AttributeClass?.IsGenericType == true &&
            attribute.AttributeClass.Name == "AutoInjectAttribute")
        {
            serviceType = attribute.AttributeClass.TypeArguments[0];
        }
        else
        {
            // 处理非泛型 AutoInjectAttribute 或旧逻辑
            serviceType = GetServiceType(attribute);
        }

        // 默认回退到实现类型
        serviceType ??= classSymbol;

        var serviceKey = GetServiceKey(attribute);
        var lifetime = GetLifetime(attribute);
        string targetNamespace = nameSpace != null ? nameSpace + ".DependencyInjection.AutoGenerated" : GetTargetNamespace(classSymbol.ContainingNamespace.ToDisplayString());

        return new ServiceRegistration(
            serviceType,
            classSymbol,
            serviceKey,
            lifetime,
            targetNamespace
        );
    }

    private static string GetTargetNamespace(string originalNamespace)
    {
        // 分割命名空间层级（如 ["Demo", "DemoModel"]）
        var parts = originalNamespace.Split('.');
        if (parts.Length == 0) return "MFToolkit.AutoGenerated"; // 根命名空间处理
        //string parentNamespace = string.Empty;
        //if (parts.Length > 3)
        //{
        //    parentNamespace = string.Join(".", parts.Take(3));
        //}
        //else if (parts.Length == 3)
        //{
        //    parentNamespace = string.Join(".", parts.Take(2));
        //}
        //else
        //{
        //    parentNamespace = string.Join(".", parts);
        //}
        // 提取父级命名空间（去掉最后一级）并拼接 "AutoGenerated"
        var parentNamespace = parts[0];
        return string.IsNullOrEmpty(parentNamespace)
            ? "MFToolkit.AutoGenerated"
            : $"{parentNamespace}.DependencyInjection.AutoGenerated";
    }


    private static ITypeSymbol? GetServiceType(AttributeData attribute)
    {
        // 处理非泛型属性中的 Type 参数（例如 [AutoInject(typeof(IService))]）
        foreach (var arg in attribute.ConstructorArguments)
        {
            if (arg.Value is ITypeSymbol typeSymbol)
            {
                return typeSymbol;
            }
        }
        return null;
    }


    //private static string? GetServiceKey(AttributeData attribute)
    //{
    //    if (attribute.ConstructorArguments.Length > 1 &&
    //        attribute.ConstructorArguments[1].Value is string key)
    //    {
    //        return key;
    //    }
    //    return null;
    //}

    private static string? GetServiceKey(AttributeData attribute)
    {
        // 检查是否有 string 参数
        foreach (var arg in attribute.ConstructorArguments)
        {
            if (arg.Value is string key)
            {
                return key;
            }
        }
        return null;
    }

    //private static Lifetime GetLifetime(AttributeData attribute)
    //{
    //    if (attribute.ConstructorArguments.Length > 2 &&
    //        attribute.ConstructorArguments[2].Value is int lifetime)
    //    {
    //        return (Lifetime)lifetime;
    //    }
    //    return Lifetime.Transient;
    //}

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
        // 按目标命名空间分组
        var groupedRegistrations = registrations
            .GroupBy(r => r.TargetNamespace)
            .ToList();

        foreach (var group in groupedRegistrations)
        {
            string targetNamespace = group.Key; // 如 "Demo.AutoGenerated"
            var registrationsInGroup = group.ToList();

            // 生成代码模板
            var source = $@"
// <auto-generated/>

using global::Microsoft.Extensions.DependencyInjection;
using global::System.Runtime.CompilerServices;

namespace {targetNamespace}
{{
    public static partial class AutoInjectExtensions
    {{

        /// <summary>
        /// 本类为生成器生成，用于自动注入服务
        /// </summary>
        /// <param name=""services"">服务注入</param>
        /// <returns>本类中所注入的服务</returns>
        [CompilerGenerated]
        public static IServiceCollection {serviceName ?? "AddAutoInjectServices"}(this IServiceCollection services)
        {{

            {BuildRegistrationCode(registrationsInGroup).Replace("\n", "\n            ")}
            return services;
        }}
    }}
}}";

            // 生成文件名（如 "Demo.AutoGenerated.AutoInject.g.cs"）
            string fileName = $"{targetNamespace}.AutoInject.g.cs";
            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }
    }


    private static string BuildRegistrationCode(IEnumerable<ServiceRegistration> registrations)
    {
        var sb = new StringBuilder();
        var currentLifetime = (Lifetime)(-1);

        foreach (var reg in registrations.OrderBy(r => r.Lifetime))
        {
            // 添加生命周期分组注释
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

            var line = !string.IsNullOrEmpty(reg.ServiceKey)
                ? BuildKeyedServiceLine(keyMethod, serviceType, implementationType, reg.ServiceKey)
                : BuildNormalServiceLine(method, serviceType, implementationType, isGeneric);

            sb.AppendLine(line);
        }
        return sb.ToString();
    }

    private static string BuildKeyedServiceLine(string method, string service, string impl, string? key)
    {
        var str = !string.IsNullOrWhiteSpace(impl) && service != impl
            ? $"services.{method}<{service}, {impl}>(\"{key}\")"
            : $"services.{method}<{service}>(\"{key}\")";
        return str + ";";
    }

    private static string BuildNormalServiceLine(string method, string service, string impl, bool isGeneric)
    {
        var str = isGeneric
            ? $"{method}(typeof({service}), typeof({impl}))"
            : service != impl
                ? $"services.{method}<{service}, {impl}>()"
                : $"services.{method}<{service}>()";
        return str + ";";
    }

    private class ServiceRegistration(
        ITypeSymbol serviceType,
        INamedTypeSymbol implementationType,
        string? serviceKey,
        Lifetime lifetime,
        string targetNamespace
    )
    {
        public ITypeSymbol ServiceType { get; set; } = serviceType;
        public INamedTypeSymbol ImplementationType { get; set; } = implementationType;
        public string? ServiceKey { get; set; } = serviceKey;
        public Lifetime Lifetime { get; set; } = lifetime;
        public string TargetNamespace { get; } = targetNamespace; // 新增属性
    }
}
