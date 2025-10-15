using System.Reflection;
using Microsoft.CodeAnalysis; // 需要添加对Microsoft.CodeAnalysis的引用

namespace MFToolkit.AutoGenerator.Common;
/// <summary>
/// 类型到代码字面量的转换工具类
/// 支持常规类型和Roslyn编译器类型符号
/// </summary>
public static class CodeLiteralConverter
{
    /// <summary>
    /// 将对象转换为对应的代码字面量
    /// </summary>
    public static string ConvertToLiteral(object? value)
    {
        if (value == null)
            return "null";

        var type = value.GetType();

        // 处理Roslyn类型符号
        if (value is ITypeSymbol typeSymbol)
            return GetTypeSymbolLiteral(typeSymbol);

        // 处理枚举类型
        if (type.IsEnum)
            return ConvertEnumToLiteral(value, type);

        // 处理Type类型（运行时类型）
        if (type == typeof(Type))
            return ConvertTypeToLiteral((Type)value);

        // 处理值类型
        if (type.IsValueType)
            return ConvertValueTypeToLiteral(value, type);

        // 处理字符串
        if (type == typeof(string))
            return ConvertStringToLiteral((string)value);

        // 处理GUID
        if (type == typeof(Guid))
            return ConvertGuidToLiteral((Guid)value);

        // 处理DateTime
        if (type == typeof(DateTime))
            return ConvertDateTimeToLiteral((DateTime)value);

        // 处理DateTimeOffset
        if (type == typeof(DateTimeOffset))
            return ConvertDateTimeOffsetToLiteral((DateTimeOffset)value);

        // 处理TimeSpan
        if (type == typeof(TimeSpan))
            return ConvertTimeSpanToLiteral((TimeSpan)value);

        // 其他引用类型（默认处理）
        return HandleOtherTypes(value, type);
    }

    #region 非递归处理类型符号（核心修改）

    /// <summary>
    /// 非递归方式获取类型符号的字面量
    /// </summary>
    private static string GetTypeSymbolLiteral(ITypeSymbol symbol)
    {
        // 构建基础类型名称（处理嵌套类型、泛型等）
        var baseTypeName = GetBaseTypeName(symbol);

        // 处理数组修饰符（[]、[,]等）
        var arraySuffix = GetArraySuffix(symbol);

        // 处理指针修饰符（*）
        var pointerSuffix = GetPointerSuffix(symbol);

        // 组合结果：typeof(基础类型+数组/指针修饰符)
        var group = $"{baseTypeName}{arraySuffix}{pointerSuffix}";
        var globalName = group.Contains("global::") ? group : $"global::{group}";
        return $"typeof({globalName})";
    }

    /// <summary>
    /// 获取基础类型名称（不含数组/指针修饰符）
    /// </summary>
    private static string GetBaseTypeName(ITypeSymbol symbol)
    {
        // 剥离数组和指针修饰符，找到最内层的基础类型
        var current = symbol;
        while (current is IArrayTypeSymbol || current is IPointerTypeSymbol)
        {
            current = current is IArrayTypeSymbol array
                ? array.ElementType
                : (current as IPointerTypeSymbol)!.PointedAtType;
        }

        // 处理命名类型（类、接口、枚举等）
        if (current is INamedTypeSymbol namedType)
        {
            // 处理泛型参数
            var genericArgs = namedType.IsGenericType
                ? $"<{string.Join(", ", namedType.TypeArguments.Select(GetBaseTypeName))}>"
                : "";

            // 处理命名空间和嵌套类型
            return namedType.ContainingType != null
                ? $"{GetBaseTypeName(namedType.ContainingType)}.{namedType.Name}{genericArgs}"
                : (string.IsNullOrEmpty(namedType.ContainingNamespace?.ToString()) || namedType.ContainingNamespace.ToString() == "global"
                    ? $"{namedType.Name}{genericArgs}"
                    : $"{namedType.ContainingNamespace}.{namedType.Name}{genericArgs}");
        }

        // 基础类型（如int、string等）
        return current.Name;
    }

    /// <summary>
    /// 获取数组修饰符（如[]、[,]）
    /// </summary>
    private static string GetArraySuffix(ITypeSymbol symbol)
    {
        var arraySuffix = "";
        var current = symbol;

        // 循环处理数组类型（支持多维数组和数组嵌套）
        while (current is IArrayTypeSymbol arrayType)
        {
            // 多维数组：rank=2 → [,]
            arraySuffix = arrayType.Rank > 1
                ? $"[{new string(',', arrayType.Rank - 1)}]{arraySuffix}"
                : $"[]{arraySuffix}";

            // 继续处理元素类型（可能还是数组）
            current = arrayType.ElementType;
        }

        return arraySuffix;
    }

    /// <summary>
    /// 获取指针修饰符（如*）
    /// </summary>
    private static string GetPointerSuffix(ITypeSymbol symbol)
    {
        var pointerSuffix = "";
        var current = symbol;

        // 循环处理指针类型（支持多级指针）
        while (current is IPointerTypeSymbol pointerType)
        {
            pointerSuffix += "*";
            current = pointerType.PointedAtType;
        }

        return pointerSuffix;
    }

    #endregion

    #region 常规类型处理

    /// <summary>
    /// 转换枚举类型到代码字面量
    /// </summary>
    private static string ConvertEnumToLiteral(object value, Type enumType)
    {
        if (enumType.IsDefined(typeof(FlagsAttribute), false))
        {
            var underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(enumType));
            return $"{GetFullTypeName(enumType)}.{value} /* 数值: {underlyingValue} */";
        }

        return $"{GetFullTypeName(enumType)}.{value}";
    }

    /// <summary>
    /// 转换Type类型到代码字面量
    /// </summary>
    private static string ConvertTypeToLiteral(Type type)
    {
        if (type == typeof(void))
            return "typeof(void)";

        return $"typeof(global::{GetFullTypeName(type)})";
    }

    /// <summary>
    /// 转换值类型到代码字面量
    /// </summary>
    private static string ConvertValueTypeToLiteral(object value, Type type)
    {
        if (type == typeof(bool))
            return (bool)value ? "true" : "false";

        if (type == typeof(char))
        {
            var charValue = (char)value;
            return charValue == '"' ? "'\\\"'" :
                   charValue == '\'' ? "'\\''" :
                   char.IsControl(charValue) ? $"'\\u{((int)charValue):X4}'" :
                   $"'{charValue}'";
        }

        // 有符号整数类型
        if (type == typeof(sbyte)) return $"{value}s";
        if (type == typeof(short)) return $"{value}";
        if (type == typeof(int)) return $"{value}";
        if (type == typeof(long)) return $"{value}L";

        // 无符号整数类型
        if (type == typeof(byte)) return $"{value}";
        if (type == typeof(ushort)) return $"{value}U";
        if (type == typeof(uint)) return $"{value}U";
        if (type == typeof(ulong)) return $"{value}UL";

        // 浮点类型
        if (type == typeof(float))
        {
            var floatValue = (float)value;
            return float.IsInfinity(floatValue) ? (floatValue > 0 ? "float.PositiveInfinity" : "float.NegativeInfinity") :
                   float.IsNaN(floatValue) ? "float.NaN" :
                   $"{value}f";
        }

        if (type == typeof(double))
        {
            var doubleValue = (double)value;
            return double.IsInfinity(doubleValue) ? (doubleValue > 0 ? "double.PositiveInfinity" : "double.NegativeInfinity") :
                   double.IsNaN(doubleValue) ? "double.NaN" :
                   $"{value}d";
        }

        if (type == typeof(decimal))
            return $"{value}m";

        // 其他值类型
        return $@"new {GetFullTypeName(type)}({string.Join(", ",
            type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0]
                .GetParameters()
                .Select(p => ConvertToLiteral(p.DefaultValue))
                .ToArray())})";
    }

    /// <summary>
    /// 转换字符串到代码字面量
    /// </summary>
    private static string ConvertStringToLiteral(string value)
    {
        // 转义双引号和特殊字符
        return $"\"{value.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\a", "\\a")
                        .Replace("\b", "\\b")
                        .Replace("\f", "\\f")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r")
                        .Replace("\t", "\\t")
                        .Replace("\v", "\\v")}\"";
    }

    /// <summary>
    /// 转换Guid到代码字面量
    /// </summary>
    private static string ConvertGuidToLiteral(Guid value)
    {
        return value == Guid.Empty ? "Guid.Empty" : $"new Guid(\"{value}\")";
    }

    /// <summary>
    /// 转换DateTime到代码字面量
    /// </summary>
    private static string ConvertDateTimeToLiteral(DateTime value)
    {
        if (value == DateTime.MinValue)
            return "DateTime.MinValue";
        if (value == DateTime.MaxValue)
            return "DateTime.MaxValue";

        return value.Kind == DateTimeKind.Utc
            ? $"new DateTime({value.Ticks}, DateTimeKind.Utc)"
            : $"new DateTime({value.Ticks})";
    }

    /// <summary>
    /// 转换DateTimeOffset到代码字面量
    /// </summary>
    private static string ConvertDateTimeOffsetToLiteral(DateTimeOffset value)
    {
        return $"new DateTimeOffset({value.Ticks}, new TimeSpan({value.Offset.Ticks}))";
    }

    /// <summary>
    /// 转换TimeSpan到代码字面量
    /// </summary>
    private static string ConvertTimeSpanToLiteral(TimeSpan value)
    {
        if (value == TimeSpan.Zero)
            return "TimeSpan.Zero";
        if (value == TimeSpan.MaxValue)
            return "TimeSpan.MaxValue";
        if (value == TimeSpan.MinValue)
            return "TimeSpan.MinValue";

        return $"new TimeSpan({value.Ticks})";
    }

    /// <summary>
    /// 处理其他未明确指定的类型
    /// </summary>
    private static string HandleOtherTypes(object value, Type type)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType()!;
            var array = (Array)value;
            if (array.Length == 0)
                return $"Array.Empty<{GetFullTypeName(elementType)}>()";

            return $"new {GetFullTypeName(elementType)}[] {{ {string.Join(", ", array.Cast<object?>().Select(ConvertToLiteral))} }}";
        }

        return $"/* 未完全支持的类型 {GetFullTypeName(type)} */ {ConvertToLiteral(value.ToString())}";
    }

    /// <summary>
    /// 获取类型的完整名称（包含命名空间）
    /// </summary>
    private static string GetFullTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var genericTypeName = type.Name.Split('`')[0];
            var genericArguments = string.Join(", ", type.GetGenericArguments().Select(GetFullTypeName));
            return $"{type.Namespace}.{genericTypeName}<{genericArguments}>";
        }

        if (type.IsNullableType())
        {
            var underlyingType = Nullable.GetUnderlyingType(type)!;
            return $"{GetFullTypeName(underlyingType)}?";
        }

        return type.Namespace == null ? type.Name : $"{type.Namespace}.{type.Name}";
    }

    /// <summary>
    /// 检查类型是否为可空类型
    /// </summary>
    private static bool IsNullableType(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    #endregion
}
