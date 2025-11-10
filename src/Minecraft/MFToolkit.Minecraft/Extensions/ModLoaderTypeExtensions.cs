using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Extensions;

/// <summary>
/// 模组加载器类型扩展方法
/// </summary>
/// <summary>
/// 模组加载器类型扩展方法
/// 提供验证、显示和兼容性检查功能
/// </summary>
public static class ModLoaderTypeExtensions
{
    /// <summary>
    /// 检查是否为有效的模组加载器组合
    /// </summary>
    /// <param name="loaderType">模组加载器类型</param>
    /// <returns>如果是有效组合返回true，否则返回false</returns>
    public static bool IsValid(this ModLoaderType loaderType)
    {
        // 检查是否设置了核心加载器（纯净版也是有效的）
        var coreLoaders = loaderType & ModLoaderType.CoreLoaderMask;
        
        // 如果有核心加载器，确保只设置了一个（使用位运算技巧检查是否为2的幂）
        if (coreLoaders != ModLoaderType.None && (coreLoaders & (coreLoaders - 1)) != ModLoaderType.None)
        {
            return false; // 设置了多个核心加载器
        }

        // 检查优化模组的兼容性
        return AreOptimizationsCompatible(loaderType);
    }

    /// <summary>
    /// 检查优化模组的兼容性
    /// </summary>
    private static bool AreOptimizationsCompatible(ModLoaderType loaderType)
    {
        // 检查Iris必须与Sodium同时使用
        if (loaderType.HasFlag(ModLoaderType.Iris) && !loaderType.HasFlag(ModLoaderType.Sodium))
        {
            return false;
        }

        // 检查Sodium和Iris的加载器兼容性（主要与Fabric/Quilt兼容）
        if (loaderType.HasFlag(ModLoaderType.Sodium) || loaderType.HasFlag(ModLoaderType.Iris))
        {
            var coreLoader = loaderType & ModLoaderType.CoreLoaderMask;
            if (coreLoader != ModLoaderType.Fabric && 
                coreLoader != ModLoaderType.Quilt && 
                coreLoader != ModLoaderType.None) // 原版理论上可以，但不推荐
            {
                // Sodium/Iris 主要与 Fabric/Quilt 兼容
                return false;
            }
        }

        // OptiFine 与 Forge、Fabric、原版都兼容
        // 不需要特殊检查

        return true;
    }

    /// <summary>
    /// 获取核心加载器类型（返回单一的核心加载器）
    /// </summary>
    /// <param name="loaderType">模组加载器类型</param>
    /// <returns>核心加载器类型，如果多个或没有返回None</returns>
    public static ModLoaderType GetCoreLoader(this ModLoaderType loaderType)
    {
        var coreLoaders = loaderType & ModLoaderType.CoreLoaderMask;
        
        // 如果设置了多个核心加载器，返回None表示无效
        if (coreLoaders != ModLoaderType.None && (coreLoaders & (coreLoaders - 1)) != ModLoaderType.None)
        {
            return ModLoaderType.None;
        }

        return coreLoaders == ModLoaderType.None ? ModLoaderType.None : coreLoaders;
    }

    /// <summary>
    /// 获取所有优化模组
    /// </summary>
    /// <param name="loaderType">模组加载器类型</param>
    /// <returns>优化模组的组合</returns>
    public static ModLoaderType GetOptimizations(this ModLoaderType loaderType)
    {
        return loaderType & ModLoaderType.OptimizationMask;
    }

    /// <summary>
    /// 获取用户友好的显示名称
    /// </summary>
    /// <param name="loaderType">模组加载器类型</param>
    /// <returns>显示名称</returns>
    public static string GetDisplayName(this ModLoaderType loaderType)
    {
        // 优先匹配预定义的组合
        switch (loaderType)
        {
            case ModLoaderType.None:
                return "";
            case ModLoaderType.VanillaWithOptiFine:
                return "OptiFine";
            case ModLoaderType.FabricWithOptiFine:
                return "Fabric_OptiFine";
            case ModLoaderType.FabricWithSodium:
                return "Fabric_Sodium";
            case ModLoaderType.FabricWithIris:
                return "Fabric_Iris";
            case ModLoaderType.ForgeWithOptiFine:
                return "Forge_OptiFine";
            case ModLoaderType.QuiltWithSodium:
                return "Quilt_Sodium";
            default:
                break;
        }

        // 动态生成名称
        var coreLoader = loaderType.GetCoreLoader();
        var optimizations = loaderType.GetOptimizations();

        if (optimizations == ModLoaderType.None)
        {
            return coreLoader switch
            {
                ModLoaderType.None => "",
                ModLoaderType.Forge => "Forge",
                ModLoaderType.Fabric => "Fabric",
                ModLoaderType.Quilt => "Quilt",
                ModLoaderType.NeoForge => "NeoForge",
                ModLoaderType.LiteLoader => "LiteLoader",
                _ => "未知加载器"
            };
        }

        var coreName = coreLoader.GetDisplayName();
        var optNames = new List<string>();

        if (optimizations.HasFlag(ModLoaderType.OptiFine)) optNames.Add("OptiFine");
        if (optimizations.HasFlag(ModLoaderType.Sodium)) optNames.Add("Sodium");
        if (optimizations.HasFlag(ModLoaderType.Iris)) optNames.Add("Iris");

        return $"{coreName} + {string.Join(" + ", optNames)}";
    }

    /// <summary>
    /// 获取详细的兼容性描述
    /// </summary>
    /// <param name="loaderType">模组加载器类型</param>
    /// <returns>兼容性描述</returns>
    public static string GetCompatibilityDescription(this ModLoaderType loaderType)
    {
        if (!loaderType.IsValid())
        {
            var issues = new List<string>();

            // 检查具体问题
            var coreLoaders = loaderType & ModLoaderType.CoreLoaderMask;
            if (coreLoaders != ModLoaderType.None && (coreLoaders & (coreLoaders - 1)) != ModLoaderType.None)
            {
                issues.Add("不能同时选择多个核心加载器");
            }

            if (loaderType.HasFlag(ModLoaderType.Iris) && !loaderType.HasFlag(ModLoaderType.Sodium))
            {
                issues.Add("Iris需要Sodium作为基础");
            }

            if ((loaderType.HasFlag(ModLoaderType.Sodium) || loaderType.HasFlag(ModLoaderType.Iris)) &&
                !loaderType.HasFlag(ModLoaderType.Fabric) && !loaderType.HasFlag(ModLoaderType.Quilt))
            {
                issues.Add("Sodium和Iris主要与Fabric/Quilt加载器兼容");
            }

            return $"配置无效：{string.Join("；", issues)}";
        }

        var warnings = new List<string>();

        // 警告信息
        if (loaderType.HasFlag(ModLoaderType.Fabric) && loaderType.HasFlag(ModLoaderType.OptiFine))
        {
            warnings.Add("Fabric与OptiFine可能存在兼容性问题，建议使用Sodium替代");
        }

        if (loaderType.HasFlag(ModLoaderType.Quilt) && loaderType.HasFlag(ModLoaderType.OptiFine))
        {
            warnings.Add("Quilt与OptiFine兼容性较差，建议使用Sodium + Iris");
        }

        if (loaderType.GetCoreLoader() == ModLoaderType.None && 
            (loaderType.HasFlag(ModLoaderType.Sodium) || loaderType.HasFlag(ModLoaderType.Iris)))
        {
            warnings.Add("原版使用Sodium/Iris需要额外支持，建议使用Fabric加载器");
        }

        return warnings.Count == 0 ? "配置兼容" : $"配置兼容（注意：{string.Join("；", warnings)}）";
    }

    /// <summary>
    /// 检查是否包含指定的优化模组
    /// </summary>
    /// <param name="loaderType">模组加载器类型</param>
    /// <param name="optimization">要检查的优化模组</param>
    /// <returns>如果包含返回true，否则返回false</returns>
    public static bool HasOptimization(this ModLoaderType loaderType, ModLoaderType optimization)
    {
        // 确保传入的是优化模组
        if ((optimization & ModLoaderType.OptimizationMask) == ModLoaderType.None)
            return false;

        return loaderType.HasFlag(optimization);
    }

    /// <summary>
    /// 获取用于启动器显示的简短名称
    /// </summary>
    /// <param name="loaderType">模组加载器类型</param>
    /// <returns>简短名称</returns>
    public static string GetShortName(this ModLoaderType loaderType)
    {
        return loaderType switch
        {
            ModLoaderType.None => "vanilla",
            ModLoaderType.Forge => "forge",
            ModLoaderType.Fabric => "fabric",
            ModLoaderType.Quilt => "quilt",
            ModLoaderType.NeoForge => "neoforge",
            ModLoaderType.LiteLoader => "liteloader",
            ModLoaderType.VanillaWithOptiFine => "vanilla_optifine",
            ModLoaderType.FabricWithOptiFine => "fabric_optifine",
            ModLoaderType.FabricWithSodium => "fabric_sodium",
            ModLoaderType.FabricWithIris => "fabric_iris",
            ModLoaderType.ForgeWithOptiFine => "forge_optifine",
            ModLoaderType.QuiltWithSodium => "quilt_sodium",
            _ => loaderType.GetCoreLoader().GetShortName() // 回退到核心加载器的短名称
        };
    }
}