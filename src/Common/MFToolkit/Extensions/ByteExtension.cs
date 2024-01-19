namespace MFToolkit.Extensions;
/// <summary>
/// byte 拓展类
/// </summary>
public static class ByteExtension
{
    public static string ToBase64String(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }
}
