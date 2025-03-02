namespace MFToolkit.Avaloniaui.Routes;

/// <summary>
/// 路由段类型枚举
/// </summary>
public enum SegmentType
{
    /// <summary>静态固定路由段（如/home）</summary>
    Static,

    /// <summary>动态参数路由段（如{id}）</summary>
    Parameter
}

/// <summary>
/// 路由段数据结构（不可变设计）
/// </summary>
/// <param name="Type">路由段类型</param>
/// <param name="Value">路由段值（静态段为固定值，参数段为参数名）</param>
public sealed record RouteSegment(SegmentType Type, string Value);