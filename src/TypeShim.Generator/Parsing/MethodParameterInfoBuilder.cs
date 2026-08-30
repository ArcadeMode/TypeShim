using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Shared;

internal class MethodParameterInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod, InteropTypeInfoCache typeInfoCache)
{
    internal IEnumerable<MethodParameterInfo> Build()
    {
        if (!memberMethod.IsStatic && memberMethod.MethodKind is not MethodKind.Constructor)
        {
            yield return new MethodParameterInfo
            {
                Name = "instance",
                IsInjectedInstanceParameter = true,
                Type = new InteropTypeInfoBuilder(classSymbol, typeInfoCache).Build()
            };
        }

        foreach (IParameterSymbol parameterSymbol in memberMethod.Parameters)
        {
            yield return new MethodParameterInfo
            {
                Name = parameterSymbol.Name,
                IsInjectedInstanceParameter = false,
                Type = new InteropTypeInfoBuilder(parameterSymbol.Type, typeInfoCache).Build(),
                Default = ResolveDefault(parameterSymbol),
            };
        }
    }

    private static ParameterDefaultInfo? ResolveDefault(IParameterSymbol parameterSymbol)
    {
        if (!parameterSymbol.HasExplicitDefaultValue)
        {
            return null;
        }

        return new ParameterDefaultInfo(parameterSymbol.ExplicitDefaultValue, IsDefaultLiteral(parameterSymbol));
    }

    private static bool IsDefaultLiteral(IParameterSymbol parameterSymbol)
    {
        foreach (SyntaxReference syntaxRef in parameterSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not ParameterSyntax { Default.Value: ExpressionSyntax defaultExpr })
            {
                continue;
            }

            // 'default(T)' or a bare 'default' literal, as opposed to an explicit '= null'.
            return defaultExpr is DefaultExpressionSyntax
                || defaultExpr.IsKind(SyntaxKind.DefaultLiteralExpression);
        }

        return false;
    }
}
