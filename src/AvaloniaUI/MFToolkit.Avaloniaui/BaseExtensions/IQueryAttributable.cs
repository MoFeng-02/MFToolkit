namespace MFToolkit.Avaloniaui.BaseExtensions;

/// <summary>
/// 用于接收传递参数
/// </summary>
public interface IQueryAttributable
{
    /// <summary>
    /// 用于获取传输的值
    /// </summary>
    /// <example>
    /// Route: home?title=你好
    /// <para>query: [{"title","你好"}]</para>
    /// </example>
    /// <param name="query">传输值集合</param>
    void ApplyQueryAttributes(IDictionary<string, object?> query);
}