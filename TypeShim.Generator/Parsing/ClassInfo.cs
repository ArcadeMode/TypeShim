namespace TypeShim.Generator.Parsing;

internal sealed class ClassInfo
{
    /// <summary>
    /// Whether the class is marked as a TSModule, else it is a TSExport class.
    /// </summary>
    internal required bool IsModule { get; init; }
    internal required string Namespace { get; init; }
    internal required string Name { get; init; }
    internal required InteropTypeInfo Type { get; init; }
    internal required IEnumerable<MethodInfo> Methods { get; init; }
    internal required IEnumerable<PropertyInfo> Properties { get; init; }

    internal bool IsSnapshotCompatible() => !IsModule && Properties.Any(p => p.Type.IsSnapshotCompatible);
}
