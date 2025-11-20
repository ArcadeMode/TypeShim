using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal class MethodParameterInfo
{ 
    internal required string ParameterName { get; init; }
    internal required bool IsInjectedInstanceParameter { get; init; }
    internal required KnownManagedType KnownType { get; init; }
    internal required TypeSyntax InteropTypeSyntax { get; init; }
    internal required TypeSyntax CLRTypeSyntax { get; init; }

    internal string GetTypedParameterName() => KnownType == KnownManagedType.Object ? $"typed_{ParameterName}" : ParameterName;
}
