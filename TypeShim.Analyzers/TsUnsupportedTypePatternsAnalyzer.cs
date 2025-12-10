using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TsUnsupportedTypePatternsAnalyzer : DiagnosticAnalyzer
{
    public const string UnsupportedTypeId = "TSHIM005";
    public const string NonExportedTypeId = "TSHIM006";
    public const string UnderDevelopmentTypeId = "TSHIM007";

    private static readonly DiagnosticDescriptor UnsupportedTypeRule = new(
        id: UnsupportedTypeId,
        title: "Unsupported type pattern",
        messageFormat: "Type '{0}' is not supported by TypeShim nor .NET-JS type marshalling",
        category: "TypeChecking",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The provided type is currently unsupported by .NET Type Marshalling and/or TypeShim interop.");

    private static readonly DiagnosticDescriptor NonExportedTypeRule = new(
        id: NonExportedTypeId,
        title: "Non-TSExport type on the interop API",
        messageFormat: "Class '{0}' is not annotated with [TSExport], it will be exported to TypeScript as 'object'",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Interop APIs should use TSExport-annotated classes or supported primitives for returns and parameters.");

    private static readonly DiagnosticDescriptor UnderDevelopmentTypeRule = new(
        id: UnderDevelopmentTypeId,
        title: "Type under development",
        messageFormat: "Type '{0}' is not yet implemented by TypeShim",
        category: "TypeChecking",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "This type is under development and not yet supported by TypeShim interop.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [UnsupportedTypeRule, NonExportedTypeRule, UnderDevelopmentTypeRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeClass, SymbolKind.NamedType);
    }

    private static void AnalyzeClass(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol type || type.TypeKind != TypeKind.Class)
            return;

        bool hasTSExport = HasAttribute(type, "TypeShim.TSExportAttribute");
        bool hasTSModule = HasAttribute(type, "TypeShim.TSModuleAttribute");
        if (!hasTSExport && !hasTSModule)
            return;

        //Debugger.Launch();
        foreach (var member in type.GetMembers())
        {
            if (member.DeclaredAccessibility != Accessibility.Public)
                continue;

            switch (member)
            {
                case IMethodSymbol method when method.MethodKind == MethodKind.Ordinary:
                    CheckType(context, method, method.ReturnType);
                    foreach (var p in method.Parameters)
                    {
                        CheckType(context, method, p.Type);
                    }
                    break;
                case IPropertySymbol prop:
                    CheckPropertyType(context, prop);
                    break;
            }
        }
    }

    private static void CheckType(SymbolAnalysisContext context, IMethodSymbol method, ITypeSymbol type)
    {
        if (ContainsNonTSExportedClass(type))
        {
            ReportNonTSExported(context, method.Name, type, method);
        }
        else if (ContainsUnderDevelopmentType(type))
        {
            ReportUnderDevelopment(context, type, method);
        }
        else if (ContainsUnsupportedPattern(type))
        {
            ReportUnsupported(context, type, method);
        }
        else if (type is INamedTypeSymbol named && named.TypeKind == TypeKind.Class)
        {
            if (!IsClassTypeWithoutTSExportRequirement(named) && !HasAttribute(named, "TypeShim.TSExportAttribute"))
            {
                ReportNonTSExported(context, method.Name, type, method);
            }
        }
    }

    private static bool ContainsNonTSExportedClass(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named && named.TypeKind == TypeKind.Class)
        {
            if (IsClassTypeWithoutTSExportRequirement(named))
                return false;

            if (HasAttribute(named, "TypeShim.TSExportAttribute"))
                return false;
            
            return true;
        }
        return false; // not a class
    }

    private static void CheckPropertyType(SymbolAnalysisContext context, IPropertySymbol prop)
    {
        ITypeSymbol type = prop.Type;
        if (ContainsNonTSExportedClass(type))
        {
            ReportNonTSExported(context, prop.Name, type, prop);
        }
        else if(ContainsUnderDevelopmentType(type))
        {
            ReportUnderDevelopment(context, type, prop);
        }
        else if (ContainsUnsupportedPattern(type))
        {
            ReportUnsupported(context, type, prop);
        }
        else if (type is INamedTypeSymbol named && named.TypeKind == TypeKind.Class)
        {
            if (!IsClassTypeWithoutTSExportRequirement(named) && !HasAttribute(named, "TypeShim.TSExportAttribute"))
            {
                ReportNonTSExported(context, prop.Name, type, prop);
            }
        }
    }

    private static bool ContainsUnderDevelopmentType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named)
        {
            if (named.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T)
            {
                return ContainsUnderDevelopmentType(named.TypeArguments[0]);
            }

            if (IsConstructedFrom(named, "global::System.Span<T>", out ITypeSymbol? spanArg))
            {
                if (IsAllowedSpanOrArraySegmentElement(spanArg))
                    return true; // under development
                return false; // not supported by .net
            }

            if (IsConstructedFrom(named, "global::System.ArraySegment<T>", out ITypeSymbol? segArg))
            {
                if (IsAllowedSpanOrArraySegmentElement(segArg))
                    return true;
                return false; // not supported by .net
            }

            if (IsAction(named))
            {
                int arity = named.TypeArguments.Length;
                // TODO: recurse on type args, also for other diagnostics
                if (arity <= 3)
                    return true;
                return false; // not supported by .net
            }
            if (IsFunc(named))
            {
                int arity = named.TypeArguments.Length;
                if (arity <= 4)
                    return true;
                return false; // not supported by .net
            }
        }

        if (type is IArrayTypeSymbol arr)
        {
            return ContainsUnderDevelopmentType(arr.ElementType);
        }

        if (IsConstructedFrom(type, "global::System.Threading.Tasks.Task<TResult>", out var taskArg))
        {
            return taskArg != null && ContainsUnderDevelopmentType(taskArg);
        }

        return false;
    }

    private static bool ContainsUnsupportedPattern(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
        {
            var elem = arrayType.ElementType;
            if (IsNullable(elem))
            {
                return true; // nullable element type unsupported
            }
            return ContainsUnsupportedPattern(elem);
        }

        if (type is INamedTypeSymbol named)
        {
            if (IsConstructedFrom(named, "global::System.Span<T>", out var spanArg))
            {
                if (!IsAllowedSpanOrArraySegmentElement(spanArg))
                    return true;
                return false;
            }
            if (IsConstructedFrom(named, "global::System.ArraySegment<T>", out var segArg))
            {
                if (!IsAllowedSpanOrArraySegmentElement(segArg))
                    return true;
                return false;
            }
            foreach (string constructedFrom in new[]
            {
                "global::System.ReadOnlySpan<T>",
                "global::System.ReadOnlyMemory<T>",
                "global::System.Memory<T>",
                "global::System.Collections.Generic.IEnumerable<T>",
            })
            {
                if (IsConstructedFrom(named, constructedFrom, out _))
                {
                    return true;
                }
            }

            if (named.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T)
            {
                var inner = named.TypeArguments[0];
                if (inner is IArrayTypeSymbol)
                {
                    return true;
                }
                return ContainsUnsupportedPattern(inner);
            }

            if (IsConstructedFrom(type, "global::System.Threading.Tasks.ValueTask<TResult>", out _))
            {
                return true;
            }

            if (IsConstructedFrom(type, "global::System.Threading.Tasks.Task<TResult>", out ITypeSymbol? taskArg))
            {
                if (taskArg is IArrayTypeSymbol || IsNullable(taskArg) || ContainsUnsupportedPattern(taskArg))
                {
                    return true;
                }
                return false;
            }

            // Action / Func exceeding allowed arity
            if (IsAction(named))
            {
                if (named.TypeArguments.Length > 3)
                    return true;
                return false;
            }
            if (IsFunc(named))
            {
                if (named.TypeArguments.Length > 4)
                    return true;
                return false;
            }
        }

        return false;
    }

    private static bool IsClassTypeWithoutTSExportRequirement(INamedTypeSymbol type)
    {
        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (fullName is "global::System.String" 
            or "global::System.Exception" 
            or "global::System.Runtime.InteropServices.JavaScript.JSObject"
            || fullName.StartsWith("global::System.Threading.Tasks.Task"))
        {
            return true;
        }
        return false;
    }

    private static bool IsConstructedFrom(ITypeSymbol type, string constructedFromFullName, out ITypeSymbol? typeArg)
    {
        typeArg = null;
        if (type is INamedTypeSymbol named && named.TypeArguments.Length == 1)
        {
            var constructed = named.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if ($"global::{constructedFromFullName}" == constructed || constructed == constructedFromFullName)
            {
                typeArg = named.TypeArguments[0];
                return true;
            }
        }
        return false;
    }

    private static bool IsNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named && named.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T)
        {
            return true;
        }
        return false;
    }

    private static bool IsAllowedSpanOrArraySegmentElement(ITypeSymbol? elem)
    {
        if (elem == null) return false;
        return elem.SpecialType is SpecialType.System_Byte or SpecialType.System_Int32 or SpecialType.System_Double;
    }

    private static bool IsAction(INamedTypeSymbol type)
    {
        var full = type.ConstructedFrom?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
        return full.StartsWith("global::System.Action", StringComparison.Ordinal);
    }

    private static bool IsFunc(INamedTypeSymbol type)
    {
        var full = type.ConstructedFrom?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty;
        return full.StartsWith("global::System.Func", StringComparison.Ordinal);
    }

    private static void ReportUnsupported(SymbolAnalysisContext context, ITypeSymbol providedType, ISymbol symbol)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        var typeText = providedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(UnsupportedTypeRule, location, typeText));
    }

    private static void ReportUnderDevelopment(SymbolAnalysisContext context, ITypeSymbol providedType, ISymbol symbol)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        var typeText = providedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(UnderDevelopmentTypeRule, location, typeText));
    }

    private static void ReportNonTSExported(SymbolAnalysisContext context, string methodName, ITypeSymbol type, ISymbol symbol)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        var typeText = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(NonExportedTypeRule, location, typeText));
    }

    private static bool HasAttribute(INamedTypeSymbol type, string fullName)
    {
        foreach (var attr in type.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == $"global::{fullName}")
                return true;
        }
        return false;
    }
}
