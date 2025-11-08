namespace MFToolkit.Minecraft.Enums;

/// <summary>
/// 模组加载器类型（使用Flags特性支持复合分配）
/// </summary>
[Flags]
public enum ModLoaderType
{
    /// <summary>
    /// 无加载器（原生）
    /// </summary>
    None = 0,

    /// <summary>
    /// Forge加载器
    /// </summary>
    Forge = 1,

    /// <summary>
    /// Fabric加载器
    /// </summary>
    Fabric = 2,

    /// <summary>
    /// Quilt加载器
    /// </summary>
    Quilt = 4,

    /// <summary>
    /// NeoForge加载器
    /// </summary>
    NeoForge = 8,

    /// <summary>
    /// Rift加载器
    /// </summary>
    Rift = 16,

    /// <summary>
    /// LiteLoader
    /// </summary>
    LiteLoader = 32,

    // === 可搭配的优化/工具类加载器 ===

    /// <summary>
    /// OptiFine（可与Fabric、Forge等搭配）
    /// </summary>
    OptiFine = 64,

    /// <summary>
    /// Sodium（主要与Fabric搭配）
    /// </summary>
    Sodium = 128,

    /// <summary>
    /// Iris（Shader支持，与Fabric搭配）
    /// </summary>
    Iris = 256,

    // === 组合定义 ===

    /// <summary>
    /// Fabric + OptiFine 组合
    /// </summary>
    FabricWithOptiFine = Fabric | OptiFine,

    /// <summary>
    /// Fabric + Sodium 组合
    /// </summary>
    FabricWithSodium = Fabric | Sodium,

    /// <summary>
    /// Fabric + Iris 组合
    /// </summary>
    FabricWithIris = Fabric | Iris,

    /// <summary>
    /// Forge + OptiFine 组合
    /// </summary>
    ForgeWithOptiFine = Forge | OptiFine,

    // === 互斥组定义 ===

    /// <summary>
    /// 核心加载器（互斥组，同一时间只能选一个）
    /// </summary>
    CoreLoaders = Forge | Fabric | Quilt | NeoForge | Rift | LiteLoader,

    /// <summary>
    /// 优化类加载器（可与核心加载器搭配）
    /// </summary>
    OptimizationLoaders = OptiFine | Sodium | Iris
}