namespace MFToolkit.Extensions;

/// <summary>
/// 类型转换扩展
/// </summary>
public static class TypeConversionExtension
{
    /// <summary>
    /// 转换为指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T ConvertTo<T>(this object value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>
    /// 从T类型转换为对象
    /// </summary>
    /// <param name="type">要转换的目标类型</param>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的值</returns>
    public static object ConvertFrom(this Type type, object value)
    {
        return Convert.ChangeType(value, type);
    }

    /// <summary>
    /// 从对象转换为整数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的整数，如果无法转换则返回类型默认值</returns>
    public static int ToInt32(this object value)
    {
        return int.TryParse(value.ToString(), out int result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为长整数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的长整数，如果无法转换则返回类型默认值</returns>
    public static long ToInt64(this object value)
    {
        return long.TryParse(value.ToString(), out long result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为十进制数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的十进制数，如果无法转换则返回类型默认值</returns>
    public static decimal ToDecimal(this object value)
    {
        return decimal.TryParse(value.ToString(), out decimal result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为双精度浮点数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的双精度浮点数，如果无法转换则返回类型默认值</returns>
    public static double ToDouble(this object value)
    {
        return double.TryParse(value.ToString(), out double result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为单精度浮点数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的单精度浮点数，如果无法转换则返回类型默认值</returns>
    public static float ToSingle(this object value)
    {
        return float.TryParse(value.ToString(), out float result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为字节
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的字节，如果无法转换则返回类型默认值</returns>
    public static byte ToByte(this object value)
    {
        return byte.TryParse(value.ToString(), out byte result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为负字节
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的负字节，如果无法转换则返回类型默认值</returns>
    public static sbyte ToSByte(this object value)
    {
        return sbyte.TryParse(value.ToString(), out sbyte result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为短整数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的短整数，如果无法转换则返回类型默认值</returns>
    public static short ToInt16(this object value)
    {
        return short.TryParse(value.ToString(), out short result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为字符
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的字符，如果无法转换则返回类型默认值</returns>
    public static char ToChar(this object value)
    {
        return char.TryParse(value.ToString(), out char result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为无符号短整数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的无符号短整数，如果无法转换则返回类型默认值</returns>
    public static ushort ToUInt16(this object value)
    {
        return ushort.TryParse(value.ToString(), out ushort result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为无符号整数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的无符号整数，如果无法转换则返回类型默认值</returns>
    public static uint ToUInt32(this object value)
    {
        return uint.TryParse(value.ToString(), out uint result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为无符号长整数
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的无符号长整数，如果无法转换则返回类型默认值</returns>
    public static ulong ToUInt64(this object value)
    {
        return ulong.TryParse(value.ToString(), out ulong result) ? result : default;
    }

    /// <summary>
    /// 从对象转换为布尔值
    /// </summary>
    /// <param name="value">要转换的对象</param>
    /// <returns>转换后的布尔值，如果无法转换则返回类型默认值</returns>
    public static bool ToBoolean(this object value)
    {
        return bool.TryParse(value.ToString(), out bool result) && result;
    }
}