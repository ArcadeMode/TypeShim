using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal class MethodParameterInfo
{ 
    internal required string ParameterName { get; init; }
    internal required bool IsInjectedInstanceParameter { get; init; }
    internal required KnownManagedType KnownType { get; init; }
    internal required TypeSyntax InteropTypeSyntax { get; init; }
    internal required TypeSyntax CLRTypeSyntax { get; init; }

    internal string GetTypedParameterName() => KnownType == KnownManagedType.Object ? $"typed_{ParameterName}" : ParameterName;

    internal MethodParameterInfo WithoutTypeInfo()
    {
        return new MethodParameterInfo
        {
            ParameterName = this.ParameterName,
            IsInjectedInstanceParameter = this.IsInjectedInstanceParameter,
            KnownType = this.KnownType,
            InteropTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
            CLRTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))
        };
    }
}
