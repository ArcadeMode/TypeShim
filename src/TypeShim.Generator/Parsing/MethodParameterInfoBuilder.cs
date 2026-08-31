using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Shared;

internal class MethodParameterInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod, InteropTypeInfoCache typeInfoCache)
{
    internal MethodParameterInfo? BuildInstanceParameter()
    {
        if (!memberMethod.IsStatic && memberMethod.MethodKind is not MethodKind.Constructor)
        {
            return new MethodParameterInfo
            {
                Name = "instance",
                Type = new InteropTypeInfoBuilder(classSymbol, typeInfoCache).Build()
            };
        }

        return null;
    }

    internal IEnumerable<MethodParameterInfo> Build()
    {
        foreach (IParameterSymbol parameterSymbol in memberMethod.Parameters)
        {
            InteropTypeInfo type = new InteropTypeInfoBuilder(parameterSymbol.Type, typeInfoCache).Build();
            yield return new MethodParameterInfo
            {
                Name = parameterSymbol.Name,
                Type = type,
                Default = ResolveDefault(parameterSymbol, type),
            };
        }
    }

    private static ParameterDefaultInfo? ResolveDefault(IParameterSymbol parameterSymbol, InteropTypeInfo type)
    {
        if (!parameterSymbol.HasExplicitDefaultValue)
        {
            return null;
        }

        // Span/ArraySegment values must be constructed on the C# side; they cannot cross the interop boundary as defaults.
        if (IsSpanOrArraySegment(type))
        {
            throw new NotSupportedDefaultValueException(
                $"Parameter '{parameterSymbol.Name}' of type '{parameterSymbol.Type}' cannot be optional because Span/ArraySegment values must be constructed on the C# side.");
        }

        ExpressionSyntax? defaultExpr = GetDefaultExpression(parameterSymbol);
        bool isDefaultLiteral = defaultExpr is DefaultExpressionSyntax
            || (defaultExpr?.IsKind(SyntaxKind.DefaultLiteralExpression) ?? false);

        if (parameterSymbol.ExplicitDefaultValue is null
            && defaultExpr is not null
            && !isDefaultLiteral
            && !defaultExpr.IsKind(SyntaxKind.NullLiteralExpression))
        {
            throw new NotSupportedDefaultValueException(
                $"Optional parameter '{parameterSymbol.Name}' has a default referencing a constant that TypeShim cannot resolve. " +
                "Only constants declared within [TSExport] classes are supported.");
        }

        return new ParameterDefaultInfo(parameterSymbol.ExplicitDefaultValue, isDefaultLiteral);
    }

    private static bool IsSpanOrArraySegment(InteropTypeInfo type)
    {
        InteropTypeInfo effective = type.IsNullableType && type.TypeArgument is not null ? type.TypeArgument : type;
        return effective.ManagedType is KnownManagedType.Span or KnownManagedType.ArraySegment;
    }

    private static ExpressionSyntax? GetDefaultExpression(IParameterSymbol parameterSymbol)
    {
        foreach (SyntaxReference syntaxRef in parameterSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is ParameterSyntax { Default.Value: ExpressionSyntax defaultExpr })
            {
                return defaultExpr;
            }
        }

        return null;
    }
}
