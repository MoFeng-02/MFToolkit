namespace MFToolkit.AutoAttribute.JsonContextGenerate;
/// <summary>
/// Json Context 上下文自动生成
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AutoJsonGenerateAttribute : Attribute
{
    /// <summary>
    /// 生成 Type
    /// </summary>
    public Type? GenerateType { get; set; }
    /// <summary>
    /// 默认构造函数，不给任何数据就生成自身
    /// </summary>
    public AutoJsonGenerateAttribute() { }
    /// <summary>
    /// 主要构造函数
    /// </summary>
    /// <param name="generateType"></param>
    public AutoJsonGenerateAttribute(Type generateType)
    {
        GenerateType = generateType;
    }
}

/// <summary>
/// 泛型 Json Context 上下文自动生成
/// </summary>
/// <typeparam name="T"></typeparam>
public class AutoJsonGenerateAttribute<T> : AutoJsonGenerateAttribute
    where T : class
{
    /// <summary>
    /// 主要构造函数
    /// </summary>
    public AutoJsonGenerateAttribute() : base(typeof(T)) { }
}