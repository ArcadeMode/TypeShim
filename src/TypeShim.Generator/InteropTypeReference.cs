namespace TypeShim.Generator;

internal sealed class InteropTypeReference
{
    internal required string TypeSyntax { get; init; }
    internal string InteropClassTypeSyntax => $"{TypeSyntax}Interop";
}
