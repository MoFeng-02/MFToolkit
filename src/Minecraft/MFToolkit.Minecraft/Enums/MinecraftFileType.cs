namespace MFToolkit.Minecraft.Enums;

/// <summary>
/// Minecraft文件类型
/// </summary>
public enum MinecraftFileType
{
    /// <summary>
    /// 游戏版本文件（客户端jar、服务端jar）
    /// </summary>
    Version,

    /// <summary>
    /// 资源索引文件
    /// </summary>
    AssetIndex,

    /// <summary>
    /// 游戏资源文件（纹理、声音、语言文件等）
    /// </summary>
    Assets,

    /// <summary>
    /// 库文件（依赖的Java库）
    /// </summary>
    Libraries,

    /// <summary>
    /// 模组文件
    /// </summary>
    Mod,

    /// <summary>
    /// 模组加载器（Forge、Fabric等安装文件）
    /// </summary>
    Loader,

    /// <summary>
    /// 资源包
    /// </summary>
    ResourcePack,

    /// <summary>
    /// 数据包
    /// </summary>
    DataPack,

    /// <summary>
    /// 光影包
    /// </summary>
    ShaderPack,

    /// <summary>
    /// 整合包
    /// </summary>
    ModPack,

    /// <summary>
    /// 世界存档/地图
    /// </summary>
    WorldSave,

    /// <summary>
    /// 配置文件
    /// </summary>
    Config,

    /// <summary>
    /// 日志文件
    /// </summary>
    Logs,

    /// <summary>
    /// 崩溃报告
    /// </summary>
    CrashReports,

    /// <summary>
    /// 截图文件
    /// </summary>
    Screenshots,

    /// <summary>
    /// 启动器相关文件
    /// </summary>
    Launcher,

    /// <summary>
    /// 游戏原生库（native libraries）
    /// </summary>
    Natives,

    /// <summary>
    /// 游戏核心文件（除版本jar外的其他核心文件）
    /// </summary>
    CoreFiles,

    /// <summary>
    /// 插件文件（Bukkit/Spigot等服务器插件）
    /// </summary>
    Plugin,

    /// <summary>
    /// 服务器文件（服务端核心、服务器配置等）
    /// </summary>
    Server,

    /// <summary>
    /// 其他未分类文件
    /// </summary>
    Other
}