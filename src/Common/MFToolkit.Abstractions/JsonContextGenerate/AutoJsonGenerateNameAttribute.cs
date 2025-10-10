namespace MFToolkit.Abstractions.JsonContextGenerate;
/// <summary>
/// Json Context 上下文自动生成名称特性
/// </summary>
/// <param name="autoGenerateName">生成名称</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class AutoJsonGenerateNameAttribute(string autoGenerateName = "GenerateJsonContext") : Attribute
{

    /// <summary>
    /// 生成名称
    /// </summary>
    public string AutoGenerateName { get; } = autoGenerateName;
}