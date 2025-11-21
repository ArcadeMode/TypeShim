using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Parsing;

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required string Name { get; init; }
    internal required IEnumerable<MethodParameterInfo> MethodParameters { get; init; }
    internal required KnownManagedType ReturnKnownType { get; init; }
    internal required TypeSyntax ReturnInteropTypeSyntax { get; init; }
    internal required TypeSyntax ReturnCLRTypeSyntax { get; init; }

    public MethodInfo WithoutInstanceParameter()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            MethodParameters = this.MethodParameters.Where(p => !p.IsInjectedInstanceParameter),
            ReturnKnownType = this.ReturnKnownType,
            ReturnInteropTypeSyntax = this.ReturnInteropTypeSyntax,
            ReturnCLRTypeSyntax = this.ReturnCLRTypeSyntax
        };
    }

    public MethodInfo WithoutInstanceParameterTypeInfo()
    {
        return new MethodInfo
        {
            IsStatic = this.IsStatic,
            Name = this.Name,
            MethodParameters = this.MethodParameters.Select(p => !p.IsInjectedInstanceParameter ? p : p.WithoutTypeInfo()),
            ReturnKnownType = this.ReturnKnownType,
            ReturnInteropTypeSyntax = this.ReturnInteropTypeSyntax,
            ReturnCLRTypeSyntax = this.ReturnCLRTypeSyntax
        };
    }
}
