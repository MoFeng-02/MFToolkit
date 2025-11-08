using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Extensions;

/// <summary>
/// ModLoaderType的扩展方法
/// </summary>
public static class ModLoaderTypeExtensions
{
    /// <summary>
    /// 获取核心加载器（排除优化类加载器）
    /// </summary>
    /// <param name="loader">加载器类型</param>
    /// <returns>核心加载器</returns>
    public static ModLoaderType GetCoreLoader(this ModLoaderType loader)
    {
        return loader & ModLoaderType.CoreLoaders;
    }

    /// <summary>
    /// 获取优化类加载器
    /// </summary>
    /// <param name="loader">加载器类型</param>
    /// <returns>优化类加载器</returns>
    public static ModLoaderType GetOptimizationLoaders(this ModLoaderType loader)
    {
        return loader & ModLoaderType.OptimizationLoaders;
    }

    /// <summary>
    /// 检查加载器组合是否有效
    /// </summary>
    /// <param name="loader">加载器类型</param>
    /// <returns>是否有效组合</returns>
    public static bool IsValidCombination(this ModLoaderType loader)
    {
        var coreLoaders = loader.GetCoreLoader();

        // 检查是否有且仅有一个核心加载器
        if (coreLoaders == ModLoaderType.None ||
            (coreLoaders & (coreLoaders - 1)) != 0) // 检查是否为2的幂（只有一个核心加载器）
        {
            return false;
        }

        // 检查特定组合的有效性
        if (coreLoaders == ModLoaderType.Forge || coreLoaders == ModLoaderType.NeoForge)
        {
            // Forge/NeoForge 不能与 Sodium/Iris 搭配
            if ((loader & ModLoaderType.Sodium) != 0 || (loader & ModLoaderType.Iris) != 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 获取加载器的显示名称
    /// </summary>
    /// <param name="loader">加载器类型</param>
    /// <returns>显示名称</returns>
    public static string GetDisplayName(this ModLoaderType loader)
    {
        if (loader == ModLoaderType.None) return "Vanilla";

        var names = new List<string>();

        // 添加核心加载器名称
        var coreLoader = loader.GetCoreLoader();
        if (coreLoader != ModLoaderType.None)
        {
            names.Add(coreLoader.ToString());
        }

        // 添加优化加载器名称
        var optimizations = loader.GetOptimizationLoaders();
        if (optimizations != ModLoaderType.None)
        {
            foreach (ModLoaderType opt in Enum.GetValues<ModLoaderType>())
            {
                if ((optimizations & opt) == opt && opt != ModLoaderType.None)
                {
                    names.Add(opt.ToString());
                }
            }
        }

        return string.Join(" + ", names);
    }
}