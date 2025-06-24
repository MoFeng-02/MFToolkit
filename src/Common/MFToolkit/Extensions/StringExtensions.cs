using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace MFToolkit.Extensions;

/// <summary>
/// String 扩展方法
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// 判断字符串是否为 Null 或空
    /// </summary>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
        => string.IsNullOrEmpty(str);

    /// <summary>
    /// 判断字符串是否为 Null、空或空白字符串
    /// </summary>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str)
        => string.IsNullOrWhiteSpace(str);

    /// <summary>
    /// 判断字符串是否包含指定的子字符串（区分大小写）
    /// </summary>
    public static bool Contains(this string source, string value, StringComparison comparison)
        => source?.IndexOf(value, comparison) >= 0;

    /// <summary>
    /// 返回首字母大写的字符串
    /// </summary>
    public static string Capitalize(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return string.Concat(str[0].ToString().ToUpper(), str.AsSpan(1));
    }

    /// <summary>
    /// 将字符串转换为驼峰命名格式（首字母小写）
    /// </summary>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return string.Concat(str[0].ToString().ToLower(), str.AsSpan(1));
    }

    /// <summary>
    /// 安全截取字符串（避免超出长度和空引用）
    /// </summary>
    public static string SafeSubstring(this string str, int startIndex, int? maxLength = null)
    {
        if (str == null || startIndex > str.Length)
            return string.Empty;

        maxLength = maxLength.HasValue
            ? Math.Min(maxLength.Value, str.Length - startIndex)
            : str.Length - startIndex;

        return str.Substring(startIndex, maxLength.Value);
    }

    /// <summary>
    /// 从Base64字符串解码
    /// </summary>
    public static string DecodeBase64(this string base64)
        => base64.IsNullOrEmpty()
        ? ""
        : Encoding.UTF8.GetString(Convert.FromBase64String(base64));

    /// <summary>
    /// 转换为Base64字符串
    /// </summary>
    public static string ToBase64(this string plainText)
        => plainText.IsNullOrEmpty()
        ? ""
        : Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

    /// <summary>
    /// 移除所有空格和换行符
    /// </summary>
    public static string RemoveWhitespace(this string input)
        => input.IsNullOrEmpty()
        ? ""
        : new string([.. input.Where(c => !char.IsWhiteSpace(c))]);

    /// <summary>
    /// 尝试转换为整数（转换失败时返回默认值）
    /// </summary>
    public static int ToInt(this string input, int defaultValue = 0)
        => int.TryParse(input, out int result) ? result : defaultValue;

    /// <summary>
    /// 按分隔符分割字符串并移除空项
    /// </summary>
    public static string[] SplitAndRemoveEmpty(this string input, params char[] separators)
        => input.Split(separators, StringSplitOptions.RemoveEmptyEntries);


    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    internal static partial Regex ValidEmail();

    /// <summary>
    /// 检查字符串是否是有效的电子邮件格式
    /// </summary>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return ValidEmail().IsMatch(email);
    }
}
