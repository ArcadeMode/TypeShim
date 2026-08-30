namespace TypeShim.Generator.Parsing;

internal sealed class ClassInfo : NamedTypeInfo
{
    internal required bool IsTSExport { get; init; }
    internal required bool IsStatic { get; init; }
    internal required ConstructorInfo? Constructor { get; init; }
    internal required IEnumerable<MethodInfo> Methods { get; init; }
    internal required IEnumerable<PropertyInfo> Properties { get; init; }
}
