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

    /// <summary>
    /// Determines whether <paramref name="type"/> is actually part of the generated TypeShim export
    /// surface, mirroring what the generator projects: a top-level type is exported only when it carries
    /// [TSExport], and a nested type is exported only when it (and every containing type up to a
    /// [TSExport] ancestor) is public. A nested type's own [TSExport] does not, by itself, export it.
    /// </summary>
    internal static bool IsExportSurfaceType(INamedTypeSymbol type)
    {
        if (type.ContainingType is null)
        {
            // Top-level: exported iff annotated.
            return HasTSExportAttribute(type);
        }

        // Nested: exported iff a containing type is [TSExport] and the chain down to this type is public.
        if (type.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        for (INamedTypeSymbol? container = type.ContainingType; container is not null; container = container.ContainingType)
        {
            if (HasTSExportAttribute(container))
            {
                return true;
            }
            // A non-public intermediate container prevents export; the top-level container's own
            // accessibility is handled separately (TSHIM008) so it is not treated as a blocker here.
            if (container.ContainingType is not null && container.DeclaredAccessibility != Accessibility.Public)
            {
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// True when any strictly-containing type of <paramref name="type"/> carries [TSExport].
    /// </summary>
    internal static bool AnyContainerIsTSExport(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? container = type.ContainingType; container is not null; container = container.ContainingType)
        {
            if (HasTSExportAttribute(container))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// True when <paramref name="type"/> is a Span or ArraySegment (unwrapping a single-level Nullable),
    /// whose values must be constructed on the C# side and therefore cannot be optional parameters.
    /// </summary>
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

    /// <summary>
    /// Matches the generated member-initializer set: a public property with a public set/init accessor
    /// whose type is non-nullable, so it must be provided through the initializer object.
    /// </summary>
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
