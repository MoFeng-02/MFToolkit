namespace MFToolkit.Extensions;
public static class ConvertExtensions
{
    /// <summary>
    /// 从对象转换为布尔值。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的布尔值。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为布尔值，则引发无效转换异常。</exception>
    public static bool ConvertToBoolean(this object obj)
    {
        return Convert.ToBoolean(obj);
    }

    /// <summary>
    /// 从对象转换为字节。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的字节。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为字节，则引发无效转换异常。</exception>
    public static byte ConvertToByte(this object obj)
    {
        return Convert.ToByte(obj);
    }

    /// <summary>
    /// 从对象转换为字符。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的字符。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为字符，则引发无效转换异常。</exception>
    public static char ConvertToChar(this object obj)
    {
        return Convert.ToChar(obj);
    }

    /// <summary>
    /// 从对象转换为日期时间。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的日期时间。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为日期时间，则引发无效转换异常。</exception>
    public static DateTime ConvertToDateTime(this object obj)
    {
        return Convert.ToDateTime(obj);
    }

    /// <summary>
    /// 从对象转换为十进制数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的十进制数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为十进制数，则引发无效转换异常。</exception>
    public static decimal ConvertToDecimal(this object obj)
    {
        return Convert.ToDecimal(obj);
    }

    /// <summary>
    /// 从对象转换为双精度浮点数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的双精度浮点数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为双精度浮点数，则引发无效转换异常。</exception>
    public static double ConvertToDouble(this object obj)
    {
        return Convert.ToDouble(obj);
    }

    /// <summary>
    /// 从对象转换为短整数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的短整数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为短整数，则引发无效转换异常。</exception>
    public static short ConvertToInt16(this object obj)
    {
        return Convert.ToInt16(obj);
    }

    /// <summary>
    /// 从对象转换为整数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的整数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为整数，则引发无效转换异常。</exception>
    public static int ConvertToInt32(this object obj)
    {
        return Convert.ToInt32(obj);
    }

    /// <summary>
    /// 从对象转换为长整数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的长整数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为长整数，则引发无效转换异常。</exception>
    public static long ConvertToInt64(this object obj)
    {
        return Convert.ToInt64(obj);
    }

    /// <summary>
    /// 从对象转换为有符号字节。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的有符号字节。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为有符号字节，则引发无效转换异常。</exception>
    public static sbyte ConvertToSByte(this object obj)
    {
        return Convert.ToSByte(obj);
    }

    /// <summary>
    /// 从对象转换为单精度浮点数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的单精度浮点数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为单精度浮点数，则引发无效转换异常。</exception>
    public static float ConvertToSingle(this object obj)
    {
        return Convert.ToSingle(obj);
    }

    /// <summary>
    /// 从对象转换为字符串。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的字符串。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为字符串，则引发无效转换异常。</exception>
    //public static string ConvertToString(this object obj)
    //{
    //    return Convert.ToString(obj);
    //}

    /// <summary>
    /// 从对象转换为无符号短整数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的无符号短整数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为无符号短整数，则引发无效转换异常。</exception>
    public static ushort ConvertToUInt16(this object obj)
    {
        return Convert.ToUInt16(obj);
    }

    /// <summary>
    /// 从对象转换为无符号整数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的无符号整数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为无符号整数，则引发无效转换异常。</exception>
    public static uint ConvertToUInt32(this object obj)
    {
        return Convert.ToUInt32(obj);
    }

    /// <summary>
    /// 从对象转换为无符号长整数。
    /// </summary>
    /// <param name="obj">要转换的对象。</param>
    /// <returns>转换后的无符号长整数。</returns>
    /// <exception cref="InvalidCastException">如果无法将对象转换为无符号长整数，则引发无效转换异常。</exception>
    public static ulong ConvertToUInt64(this object obj)
    {
        return Convert.ToUInt64(obj);
    }
}