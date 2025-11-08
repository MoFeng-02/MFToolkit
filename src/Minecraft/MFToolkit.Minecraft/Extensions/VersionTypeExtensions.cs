using MFToolkit.Minecraft.Enums;

namespace MFToolkit.Minecraft.Extensions;


public static class VersionTypeExtensions
{
    /// <summary>
    /// 将字符转为对应的类型，若不支持则返回None，则表示非支持类型
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static VersionType ToVersionType(this string value) => value.ToLower() switch
    {
        "release" => VersionType.Release,
        "snapshot" => VersionType.Snapshot,
        "old_alpha" => VersionType.OldAlpha,
        "old_beta" => VersionType.OldBeta,
        _ => VersionType.None
    };

}