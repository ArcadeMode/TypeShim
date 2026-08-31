using TypeShim.Shared;

namespace TypeShim.Generator.Parsing;

internal abstract class NamedTypeInfo
{
    internal required string Namespace { get; init; }
    internal required string Name { get; init; }
    internal required InteropTypeInfo Type { get; init; }
    internal required CommentInfo? Comment { get; init; }
    internal IReadOnlyList<NamedTypeInfo> NestedTypes { get; init; } = [];
}
