namespace TypeShim.Generator.Parsing;

internal sealed class ClassInfo
{
    internal required string Namespace { get; init; }
    internal required string Name { get; init; }
    internal required InteropTypeInfo Type { get; init; }
    internal required IEnumerable<MethodInfo> Methods { get; init; }
    internal required IEnumerable<PropertyInfo> Properties { get; init; }

    internal bool IsSnapshotCompatible() => !Type.IsTSModule // not a module (are static) TODO: add isstatic field to classinfo?
        && Properties.Any() // has properties at all
        && !Properties.Any(p => !p.Type.IsSnapshotCompatible); // all properties snapshot compatible
}
