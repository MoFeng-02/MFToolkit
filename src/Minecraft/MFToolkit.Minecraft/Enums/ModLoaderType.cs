namespace MFToolkit.Minecraft.Enums;

/// <summary>
/// 模组加载器类型（使用Flags特性支持复合分配）
/// 参考PCL2、HMCL等主流启动器的设计
/// </summary>
/// <remarks>
/// Flags枚举使用注意事项：
/// 1. 每个值必须是2的幂（1, 2, 4, 8, 16, 32, 64, 128...）
/// 2. 使用按位OR操作符组合多个值：Forge | OptiFine
/// 3. 使用HasFlag()方法检查是否包含特定标志
/// 4. 组合值只是语法糖，不会产生新的枚举值
/// </remarks>
[Flags]
public enum ModLoaderType
{
    /// <summary>
    /// 无加载器（纯净版/原版）
    /// </summary>
    None = 0,

    // ===== 核心模组加载器（互斥，实际使用时应该只选一个）=====

    /// <summary>
    /// Forge加载器 - 最传统的模组加载器
    /// </summary>
    Forge = 1,

    /// <summary>
    /// Fabric加载器 - 轻量级现代加载器
    /// </summary>
    Fabric = 2,

    /// <summary>
    /// Quilt加载器 - Fabric的改进分支
    /// </summary>
    Quilt = 4,

    /// <summary>
    /// NeoForge加载器 - Forge的分支
    /// </summary>
    NeoForge = 8,

    /// <summary>
    /// LiteLoader - 轻量级客户端加载器
    /// </summary>
    LiteLoader = 16,

    // ===== 优化/工具类模组（可与核心加载器搭配）=====

    /// <summary>
    /// OptiFine - 高清修复和性能优化，支持Forge、Fabric和原版
    /// </summary>
    OptiFine = 32,

    /// <summary>
    /// Sodium - 高性能渲染引擎，主要与Fabric/Quilt搭配
    /// </summary>
    Sodium = 64,

    /// <summary>
    /// Iris - 着色器支持，需要Sodium作为基础
    /// </summary>
    Iris = 128,

    // ===== 常用组合定义（语法糖，方便使用）=====

    /// <summary>
    /// 原版 + OptiFine（经典组合）
    /// </summary>
    VanillaWithOptiFine = None | OptiFine,

    /// <summary>
    /// Fabric + OptiFine组合
    /// </summary>
    FabricWithOptiFine = Fabric | OptiFine,

    /// <summary>
    /// Fabric + Sodium组合（性能优化组合）
    /// </summary>
    FabricWithSodium = Fabric | Sodium,

    /// <summary>
    /// Fabric + Sodium + Iris组合（完整着色器支持）
    /// </summary>
    FabricWithIris = Fabric | Sodium | Iris,

    /// <summary>
    /// Forge + OptiFine组合（传统组合）
    /// </summary>
    ForgeWithOptiFine = Forge | OptiFine,

    /// <summary>
    /// Quilt + Sodium组合
    /// </summary>
    QuiltWithSodium = Quilt | Sodium,

    // ===== 分类掩码（用于验证和分类，不作为实际值使用）=====

    /// <summary>
    /// 所有核心加载器的掩码（用于验证）
    /// </summary>
    CoreLoaderMask = Forge | Fabric | Quilt | NeoForge | LiteLoader,

    /// <summary>
    /// 所有优化模组的掩码（用于验证）
    /// </summary>
    OptimizationMask = OptiFine | Sodium | Iris
}