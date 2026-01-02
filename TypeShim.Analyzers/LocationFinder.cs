using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace TypeShim.Analyzers;

internal static class LocationFinder
{

    internal static Location GetDefaultLocation(ISymbol symbol)
            => symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;

    internal static Location GetMethodParameterLocation(IMethodSymbol method, IParameterSymbol parameter, CancellationToken t)
    {
        foreach (SyntaxReference syntaxRef in parameter.DeclaringSyntaxReferences)
        {
            SyntaxNode node = syntaxRef.GetSyntax(t);

            if (node is ParameterSyntax { Type: TypeSyntax ts })
                return ts.GetLocation();
        }
        return GetDefaultLocation(method); // TODO; try pass parameter
    }

    internal static Location GetMethodReturnTypeLocation(IMethodSymbol method, CancellationToken t)
    {
        foreach (SyntaxReference syntaxRef in method.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax(t) is MethodDeclarationSyntax m)
                return m.ReturnType.GetLocation();
        }
        return GetDefaultLocation(method);
    }

    internal static Location GetPropertyTypeLocation(IPropertySymbol property, CancellationToken t)
    {
        foreach (SyntaxReference syntaxRef in property.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax(t) is PropertyDeclarationSyntax propertyDeclaration)
                return propertyDeclaration.Type.GetLocation();
        }
        return GetDefaultLocation(property);
    }
}