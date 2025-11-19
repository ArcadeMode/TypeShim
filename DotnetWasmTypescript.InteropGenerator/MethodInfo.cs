using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required string Name { get; init; }
    internal required IEnumerable<MethodParameterInfo> MethodParameters { get; init; }
    internal required KnownManagedType ReturnKnownType { get; init; }
    internal required TypeSyntax ReturnInteropTypeSyntax { get; init; }
    internal required TypeSyntax ReturnCLRTypeSyntax { get; init; }
}
