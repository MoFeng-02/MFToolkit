namespace MFToolkit.Avaloniaui.Routes.Extensions;

public static class RoutingStringExtensions
{
    /// <summary>
    /// 尝试将字符范围按指定分隔符分割为两部分
    /// </summary>
    /// <param name="source">输入的字符范围（需分割的原始数据）</param>
    /// <param name="separator">分隔符</param>
    /// <param name="segment">输出参数，返回分隔符前的第一个段（若未找到分隔符，则为完整输入）</param>
    /// <param name="remaining">输出参数，返回分隔符后的剩余字符范围（若未找到分隔符，则为空）</param>
    /// <returns>
    ///   <c>true</c> - 成功找到分隔符并分割；
    ///   <c>false</c> - 未找到分隔符，segment为完整输入，remaining为空
    /// </returns>
    /// <example>
    /// 示例：
    /// <code>
    /// ReadOnlySpan&lt;char&gt; span = "a,b,c".AsSpan();
    /// if (span.TrySplit(',', out var segment, out var remaining)) 
    /// {
    ///     // segment = "a", remaining = "b,c"
    /// }
    /// </code>
    /// </example>
    public static bool TrySplit(
        this ReadOnlySpan<char> source,
        char separator,
        out ReadOnlySpan<char> segment,
        out ReadOnlySpan<char> remaining
    )
    {
        // 查找分隔符位置
        var index = source.IndexOf(separator);
        if (index == -1)
        {
            // 未找到分隔符：返回完整输入，剩余部分为空
            segment = source;
            remaining = ReadOnlySpan<char>.Empty;
            return false;
        }

        // 分割为两部分：分隔符前和分隔符后
        segment = source[..index]; // 分隔符前的段
        remaining = source[(index + 1)..]; // 分隔符后的剩余部分
        return true;
    }
}