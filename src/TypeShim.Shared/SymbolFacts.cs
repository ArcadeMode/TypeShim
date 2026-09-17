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

    internal static bool IsTSExportOrNested(ITypeSymbol type)
    {
        for (ITypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (HasTSExportAttribute(current))
            {
                return true;
            }
        }
        return false;
    }

    internal static Accessibility GetEffectiveAccessibility(ITypeSymbol type)
    {
        Accessibility lowest = Accessibility.Public;
        for (ITypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility < lowest)
            {
                lowest = current.DeclaredAccessibility;
            }
        }
        return lowest;
    }

    internal static bool IsSpanOrArraySegment(ITypeSymbol type)
    {
        ITypeSymbol effective = type;
        if (IsNullable(type) && type is INamedTypeSymbol { TypeArguments.Length: 1 } nullable)
        {
            effective = nullable.TypeArguments[0];
        }

        string fullName = effective.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return fullName.StartsWith(Constants.SpanGlobal, StringComparison.Ordinal)
            || fullName.StartsWith(Constants.ArraySegmentGlobal, StringComparison.Ordinal);
    }

    internal static bool IsNonOmittableInitializerMember(IPropertySymbol property)
    {
        if (property.DeclaredAccessibility != Accessibility.Public
            || property.SetMethod is not { DeclaredAccessibility: Accessibility.Public })
        {
            return false;
        }

        return property.Type.NullableAnnotation != NullableAnnotation.Annotated;
    }

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
