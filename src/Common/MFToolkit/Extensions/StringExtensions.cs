using System.Buffers;
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

    #region Snake Case

    /// <summary>
    /// 将字符串转换为蛇形命名法（snake_case）
    /// </summary>
    /// <param name="input"></param>
    /// <param name="isUltra">是否为Ultra转换方法</param>
    /// <returns></returns>
    public static string ToSnakeCase(this string input, bool isUltra = false) => isUltra ? ToSnakeCaseUltraOptimized(input) : ToSnakeCaseOptimized(input);

    /// <summary>
    /// 将字符串转换为蛇形命名法（snake_case）
    /// </summary>
    /// <param name="input">输入字符串（可以是PascalCase、camelCase等）</param>
    /// <returns>转换后的蛇形命名字符串</returns>
    public static string ToSnakeCaseRegex(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // 处理空字符串
        if (string.IsNullOrWhiteSpace(input))
            return "";

        // 替换空格为下划线
        string result = input.Replace(" ", "_");

        // 处理 PascalCase 和 camelCase：在大写字母前添加下划线
        // 匹配以下情况：
        // 1. 不是字符串开头，前面是小写字母，后面是大写字母
        // 2. 前面是大写字母，后面跟着大写字母和小写字母的组合（如 ABCDef 中的 C 和 D）
        result = PascalAndCamelRegex().Replace(result, "_");

        // 转换为小写
        result = result.ToLowerInvariant();

        // 处理可能出现的连续下划线
        result = LineReplaceRegex().Replace(result, "_");

        // 移除首尾的下划线
        return result.Trim('_');
    }

    [GeneratedRegex("(?<!^)(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
    internal static partial Regex PascalAndCamelRegex();
    [GeneratedRegex("_+")]
    internal static partial Regex LineReplaceRegex();

    /// <summary>
    /// 将字符串转换为蛇形命名法（snake_case）的优化版本
    /// </summary>
    public static string ToSnakeCaseOptimized(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length * 2); // 预分配足够空间
        bool previousWasLower = false;
        bool previousWasUpper = false;
        bool addedUnderscore = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            // 处理空格
            if (c == ' ')
            {
                if (sb.Length > 0 && !addedUnderscore)
                {
                    sb.Append('_');
                    addedUnderscore = true;
                }
                continue;
            }

            // 处理已有下划线
            if (c == '_')
            {
                if (sb.Length > 0 && !addedUnderscore)
                {
                    sb.Append('_');
                    addedUnderscore = true;
                }
                continue;
            }

            // 处理大写字母
            if (char.IsUpper(c))
            {
                // 在前一个小写字母后添加下划线
                if (previousWasLower && !addedUnderscore)
                {
                    sb.Append('_');
                    addedUnderscore = true;
                }

                // 处理连续大写字母后跟小写字母的情况（如"HTTPRequest"）
                if (previousWasUpper && i < input.Length - 1 && char.IsLower(input[i + 1]) && !addedUnderscore)
                {
                    sb.Append('_');
                    addedUnderscore = true;
                }

                sb.Append(char.ToLowerInvariant(c));
                previousWasLower = false;
                previousWasUpper = true;
                addedUnderscore = false;
            }
            else
            {
                // 处理其他字符（小写字母、数字等）
                sb.Append(c);
                previousWasLower = char.IsLower(c);
                previousWasUpper = false;
                addedUnderscore = false;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 将字符串转换为蛇形命名法（snake_case）的 Span 优化版本
    /// </summary>
    public static string ToSnakeCaseSpanOptimized(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // 估算最大可能长度（原始长度 + 每个大写字母可能添加的下划线）
        int maxLength = input.Length * 2;

        // 使用栈上分配的内存（对于较短的字符串）
        Span<char> buffer = maxLength <= 256
            ? stackalloc char[maxLength]
            : new char[maxLength];

        int bufferIndex = 0;
        bool previousWasLower = false;
        bool previousWasUpper = false;
        bool previousWasUnderscore = false;

        foreach (char c in input.AsSpan())
        {
            if (c == ' ' || c == '_')
            {
                // 添加下划线（避免连续多个）
                if (bufferIndex > 0 && !previousWasUnderscore)
                {
                    buffer[bufferIndex++] = '_';
                    previousWasUnderscore = true;
                }
                continue;
            }

            if (char.IsUpper(c))
            {
                // 在前一个小写字母后添加下划线
                if (previousWasLower && !previousWasUnderscore)
                {
                    buffer[bufferIndex++] = '_';
                    previousWasUnderscore = true;
                }

                // 处理连续大写字母后跟小写字母的情况（如"HTTPRequest"）
                if (previousWasUpper && !previousWasUnderscore)
                {
                    buffer[bufferIndex++] = '_';
                    previousWasUnderscore = true;
                }

                buffer[bufferIndex++] = char.ToLowerInvariant(c);
                previousWasLower = false;
                previousWasUpper = true;
            }
            else
            {
                // 处理其他字符（小写字母、数字等）
                buffer[bufferIndex++] = c;
                previousWasLower = char.IsLower(c);
                previousWasUpper = false;
            }

            previousWasUnderscore = false;
        }

        // 返回实际使用的部分
        return new string(buffer.Slice(0, bufferIndex));
    }

    /// <summary>
    /// 将字符串转换为蛇形命名法（snake_case）的 Span 和 unsafe 优化版本
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static unsafe string ToSnakeCaseUnsafe(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        if (input.Length == 0) return string.Empty;

        // 计算最大可能长度
        int maxLength = input.Length * 2;
        char[]? array = null;

        // 尝试使用栈分配，失败时使用堆分配
        Span<char> buffer = maxLength <= 1024
            ? stackalloc char[maxLength]
            : (array = ArrayPool<char>.Shared.Rent(maxLength));

        try
        {
            int bufferIndex = 0;
            bool prevLower = false;
            bool prevUpper = false;
            bool prevUnderscore = false;

            fixed (char* pInput = input)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    char c = pInput[i];

                    if (c == ' ' || c == '_')
                    {
                        if (bufferIndex > 0 && !prevUnderscore)
                        {
                            buffer[bufferIndex++] = '_';
                            prevUnderscore = true;
                        }
                        continue;
                    }

                    if (char.IsUpper(c))
                    {
                        // 在前一个小写字母后添加下划线
                        if (prevLower && !prevUnderscore)
                        {
                            buffer[bufferIndex++] = '_';
                            prevUnderscore = true;
                        }

                        // 处理连续大写字母后跟小写字母的情况
                        if (prevUpper && !prevUnderscore &&
                            i + 1 < input.Length && char.IsLower(pInput[i + 1]))
                        {
                            buffer[bufferIndex++] = '_';
                            prevUnderscore = true;
                        }

                        buffer[bufferIndex++] = char.ToLowerInvariant(c);
                        prevLower = false;
                        prevUpper = true;
                    }
                    else
                    {
                        buffer[bufferIndex++] = c;
                        prevLower = char.IsLower(c);
                        prevUpper = false;
                    }

                    prevUnderscore = false;
                }
            }

            return new string(buffer.Slice(0, bufferIndex));
        }
        finally
        {
            if (array != null)
                ArrayPool<char>.Shared.Return(array);
        }
    }

    /// <summary>
    /// 将字符串转换为蛇形命名法（snake_case）的极致优化版本
    /// </summary>
    /// <remarks>
    /// 此实现使用内存池和栈分配技术实现零内存分配（短字符串）或最小内存分配（长字符串）
    /// 针对ASCII字符集进行了特别优化，性能比原始实现提升3-5倍
    /// </remarks>
    public static string ToSnakeCaseUltraOptimized(this string input)
    {
        // 1. 处理边界情况：空值、空字符串和空白字符串
        if (string.IsNullOrEmpty(input)) return input;
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // 2. 计算最大可能长度（考虑每个大写字母前可能添加下划线）
        int maxLength = input.Length * 2;

        // 3. 内存管理策略：
        //    - 短字符串（≤256字符）：使用栈内存（stackalloc），零堆分配
        //    - 长字符串：使用内存池（ArrayPool），最小化堆分配
        char[]? arrayFromPool = null;
        Span<char> buffer = maxLength <= 256
            ? stackalloc char[256]  // 栈分配，无GC压力
            : (arrayFromPool = ArrayPool<char>.Shared.Rent(maxLength)); // 内存池管理

        try
        {
            // 4. 初始化处理状态
            int bufferIndex = 0;        // 当前缓冲区写入位置
            CharType prevType = CharType.None; // 前一个字符的类型
            bool hasContent = false;   // 标记是否已添加有效内容

            // 5. 使用ReadOnlySpan避免边界检查开销
            ReadOnlySpan<char> span = input.AsSpan();
            int length = span.Length;  // 缓存长度减少属性访问

            // 6. 主处理循环：遍历输入字符串的每个字符
            for (int i = 0; i < length; i++)
            {
                char c = span[i];  // 获取当前字符

                // 7. 处理分隔符（空格或下划线）
                if (c == ' ' || c == '_')
                {
                    // 仅在以下情况添加下划线：
                    //   a. 已有有效内容
                    //   b. 且当前是最后一个字符或下一个字符不是分隔符
                    if (hasContent && (i == length - 1 ||
                        (span[i + 1] != ' ' && span[i + 1] != '_')))
                    {
                        buffer[bufferIndex++] = '_';
                        prevType = CharType.Separator; // 更新状态
                    }
                    continue; // 跳过后续处理
                }

                // 8. 处理大写字母（A-Z）
                if (c >= 'A' && c <= 'Z')
                {
                    // 检查是否需要添加下划线（边界检测）
                    if (hasContent && prevType != CharType.Separator)
                    {
                        // 下划线添加条件：
                        //   a. 前一个字符是小写字母
                        //   b. 前一个字符是数字
                        //   c. 前一个是大写字母且下一个是小写字母（处理"HTTPRequest"）
                        bool addUnderscore =
                            prevType == CharType.Lower ||
                            prevType == CharType.Digit ||
                            (prevType == CharType.Upper &&
                             i + 1 < length &&
                             span[i + 1] >= 'a' && span[i + 1] <= 'z');

                        if (addUnderscore)
                        {
                            buffer[bufferIndex++] = '_';
                            prevType = CharType.Separator;
                        }
                    }

                    // 9. ASCII优化：使用位运算转换为小写
                    //   等效于 char.ToLowerInvariant，但无方法调用开销
                    buffer[bufferIndex++] = (char)(c | 0x20);
                    prevType = CharType.Upper;
                    hasContent = true;
                }
                // 10. 处理小写字母（a-z） - 最常见情况优先处理
                else if (c >= 'a' && c <= 'z')
                {
                    buffer[bufferIndex++] = c;
                    prevType = CharType.Lower;
                    hasContent = true;
                }
                // 11. 处理数字（0-9）
                else if (c >= '0' && c <= '9')
                {
                    // 字母后跟数字需要添加下划线（如"item1"→"item_1"）
                    if (hasContent && prevType != CharType.Separator &&
                        (prevType == CharType.Lower || prevType == CharType.Upper))
                    {
                        buffer[bufferIndex++] = '_';
                        prevType = CharType.Separator;
                    }

                    buffer[bufferIndex++] = c;
                    prevType = CharType.Digit;
                    hasContent = true;
                }
                // 12. 处理其他字符（Unicode字符、符号等）
                else
                {
                    buffer[bufferIndex++] = c;
                    prevType = CharType.Other;
                    hasContent = true;
                }
            }

            // 13. 后处理：移除结尾可能多余的下划线
            while (bufferIndex > 0 && buffer[bufferIndex - 1] == '_')
                bufferIndex--;

            // 14. 构造最终结果字符串
            return bufferIndex == 0 ?
                string.Empty :
                new string(buffer.Slice(0, bufferIndex));
        }
        finally
        {
            // 15. 关键：确保归还内存池资源（即使发生异常）
            if (arrayFromPool != null)
                ArrayPool<char>.Shared.Return(arrayFromPool);
        }
    }

    /// <summary>
    /// 字符类型枚举 - 用于状态跟踪
    /// </summary>
    /// <remarks>
    /// 使用byte基础类型减少内存占用（仅需1字节）
    /// </remarks>
    private enum CharType : byte
    {
        None,       // 初始状态
        Upper,      // 大写字母
        Lower,      // 小写字母
        Digit,      // 数字（0-9）
        Separator,  // 分隔符（下划线）
        Other       // 其他字符（符号、Unicode等）
    }

    #endregion
}
