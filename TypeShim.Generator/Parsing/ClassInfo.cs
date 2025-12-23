namespace TypeShim.Generator.Parsing;

internal sealed class ClassInfo
{
    internal required string Namespace { get; init; }
    internal required string Name { get; init; }
    internal required InteropTypeInfo Type { get; init; }
    internal required IEnumerable<MethodInfo> Methods { get; init; }
    internal required IEnumerable<PropertyInfo> Properties { get; init; }

    internal bool IsSnapshotCompatible() => !Type.IsTSModule && Properties.Any(p => p.Type.IsSnapshotCompatible);
}
