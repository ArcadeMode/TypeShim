using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace TypeShim.Shared;

internal static class SymbolFacts
{
    private const string JSExportFqn = "global::System.Runtime.InteropServices.JavaScript.JSExportAttribute";
    private const string TSExportFqn = "global::TypeShim.TSExportAttribute";

    internal static bool IsPublicClass(INamedTypeSymbol type)
        => type.TypeKind == TypeKind.Class && type.DeclaredAccessibility == Accessibility.Public;

    internal static bool HasJSExportAttribute(ISymbol symbol)
        => symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == JSExportFqn);

    internal static bool HasTSExportAttribute(ISymbol symbol)
    {
        foreach (AttributeData attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass is null) continue;
            if (attr.AttributeClass.Kind == SymbolKind.ErrorType)
            {
                if (attr.AttributeClass.Name is "TSExportAttribute" or "TSExport")
                    return true;
            }
            else if (attr.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == TSExportFqn)
                return true;
        }
        return false;
    }

    internal static bool IsNullable(ITypeSymbol type)
    {
        return type is INamedTypeSymbol named
            && named.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T;
    }

    internal static bool IsAction(INamedTypeSymbol type)
    {
        var full = type.ConstructedFrom?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
        return full.StartsWith("global::System.Action", StringComparison.Ordinal);
    }

    internal static bool IsFunc(INamedTypeSymbol type)
    {
        var full = type.ConstructedFrom?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
        return full.StartsWith("global::System.Func", StringComparison.Ordinal);
    }

    internal static bool IsConstructedFrom(ITypeSymbol type, string constructedFromFullName, out ITypeSymbol? typeArg)
    {
        if (type is INamedTypeSymbol named && named.TypeArguments.Length == 1)
        {
            var constructed = named.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if ($"global::{constructedFromFullName}" == constructed || constructed == constructedFromFullName)
            {
                typeArg = named.TypeArguments[0];
                return true;
            }
        }
        typeArg = null;
        return false;
    }
}
