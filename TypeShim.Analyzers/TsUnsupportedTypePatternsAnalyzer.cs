using System;
using System.Collections.Immutable;
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
        messageFormat: "Member '{0}' uses unsupported type '{1}' in {2}",
        category: "TypeChecking",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The provided type is currently unsupported by .NET Type Marshalling and/or TypeShim interop.");

    private static readonly DiagnosticDescriptor NonExportedTypeRule = new(
        id: NonExportedTypeId,
        title: "Non-TSExport type on the interop API",
        messageFormat: "Member '{0}' uses '{1}' in {2}, which is not annotated with [TSExport], the type will be exported as 'object'",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Interop APIs should use TSExport-annotated classes or supported primitives for returns and parameters.");

    private static readonly DiagnosticDescriptor UnderDevelopmentTypeRule = new(
        id: UnderDevelopmentTypeId,
        title: "Type under development",
        messageFormat: "Member '{0}' uses type '{1}' in {2}, which is currently not supported",
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

        foreach (var member in type.GetMembers())
        {
            if (member.DeclaredAccessibility != Accessibility.Public)
                continue;

            switch (member)
            {
                case IMethodSymbol method when method.MethodKind == MethodKind.Ordinary:
                    // Check return type
                    CheckType(context, method, method.ReturnType, isReturn: true);
                    // Check parameters
                    foreach (var p in method.Parameters)
                    {
                        CheckType(context, method, p.Type, isReturn: false);
                    }
                    // Additional rule: warning when returning non-TSExport class
                    CheckReturnNonExportedClass(context, method);
                    break;
                case IPropertySymbol prop:
                    CheckPropertyType(context, prop);
                    break;
            }
        }
    }

    private static void CheckType(SymbolAnalysisContext context, IMethodSymbol method, ITypeSymbol type, bool isReturn)
    {
        string pos = isReturn ? "return type" : "parameter";
        if (ContainsUnderDevelopmentType(type))
        {
            ReportUnderDevelopment(context, method.Name, pos, type, method);
        } 
        else if (ContainsUnsupportedPattern(type))
        {
            ReportUnsupported(context, method.Name, pos, type, method);
        }
        else if (type is INamedTypeSymbol named && named.TypeKind == TypeKind.Class)
        {
            if (!IsAllowedClassType(named) && !HasAttribute(named, "TypeShim.TSExportAttribute"))
            {
                var location = method.Locations.Length > 0 ? method.Locations[0] : Location.None;
                var typeText = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                context.ReportDiagnostic(Diagnostic.Create(NonExportedTypeRule, location, method.Name, typeText, pos));
            }
        }
    }

    private static void CheckReturnNonExportedClass(SymbolAnalysisContext context, IMethodSymbol method)
    {
        ITypeSymbol returnType = method.ReturnType;
        // Only consider class types; structs like Span<T> are ignored by virtue of not being classes
        if (returnType is INamedTypeSymbol named && named.TypeKind == TypeKind.Class)
        {
            // Skip allowed framework/simple types
            if (IsAllowedClassType(named))
                return;

            // If the class itself is TSExport, it's fine
            if (HasAttribute(named, "TypeShim.TSExportAttribute"))
                return;

            // Otherwise warn
            var location = method.Locations.Length > 0 ? method.Locations[0] : Location.None;
            var typeText = returnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            context.ReportDiagnostic(Diagnostic.Create(NonExportedTypeRule, location, method.Name, typeText, "return type"));
        }
    }

    private static void CheckPropertyType(SymbolAnalysisContext context, IPropertySymbol prop)
    {
        var type = prop.Type;
        const string pos = "property type";
        if (ContainsUnderDevelopmentType(type))
        {
            ReportUnderDevelopment(context, prop.Name, pos, type, prop);
        }
        else if (ContainsUnsupportedPattern(type))
        {
            ReportUnsupported(context, prop.Name, pos, type, prop);
        }
        else if (type is INamedTypeSymbol named && named.TypeKind == TypeKind.Class)
        {
            if (!IsAllowedClassType(named) && !HasAttribute(named, "TypeShim.TSExportAttribute"))
            {
                var location = prop.Locations.Length > 0 ? prop.Locations[0] : Location.None;
                var typeText = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                context.ReportDiagnostic(Diagnostic.Create(NonExportedTypeRule, location, prop.Name, typeText, pos));
            }
        }
    }

    private static bool ContainsUnderDevelopmentType(ITypeSymbol type)
    {
        // Check direct
        if (type is INamedTypeSymbol named)
        {
            // Nullable<T>
            if (named.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T)
            {
                return ContainsUnderDevelopmentType(named.TypeArguments[0]);
            }

            // Span<T>
            if (IsConstructedFrom(named, "global::System.Span", out var spanArg))
            {
                if (IsAllowedSpanOrArraySegmentElement(spanArg))
                    return true; 
                return false; // not supported by .net
            }

            // ArraySegment<T>
            if (IsConstructedFrom(named, "global::System.ArraySegment", out var segArg))
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

        // Array element
        if (type is IArrayTypeSymbol arr)
        {
            return ContainsUnderDevelopmentType(arr.ElementType);
        }

        // Task<T>
        if (IsConstructedFrom(type, "global::System.Threading.Tasks.Task", out var taskArg))
        {
            return taskArg != null && ContainsUnderDevelopmentType(taskArg);
        }

        return false;
    }

    private static bool ContainsUnsupportedPattern(ITypeSymbol type)
    {
        // Unsupported if any array has a nullable element at any depth,
        // or Task<T> where T contains such a pattern or is Nullable<T> or is an Array
        if (type is IArrayTypeSymbol arrayType)
        {
            var elem = arrayType.ElementType;
            if (IsNullable(elem))
            {
                return true;
            }
            return ContainsUnsupportedPattern(elem);
        }

        if (type is INamedTypeSymbol named)
        {
            // Span<T> with unsupported element types
            if (IsConstructedFrom(named, "global::System.Span", out var spanArg))
            {
                if (!IsAllowedSpanOrArraySegmentElement(spanArg))
                    return true;
                return false;
            }
            // ArraySegment<T> with unsupported element types
            if (IsConstructedFrom(named, "global::System.ArraySegment", out var segArg))
            {
                if (!IsAllowedSpanOrArraySegmentElement(segArg))
                    return true;
                return false;
            }

            // Nullable<T>
            if (named.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T)
            {
                var inner = named.TypeArguments[0];
                // Nullable of array or of another unsupported nested pattern
                if (inner is IArrayTypeSymbol)
                {
                    return true;
                }
                return ContainsUnsupportedPattern(inner);
            }

            // Task<T>
            if (IsConstructedFrom(type, "global::System.Threading.Tasks.Task", out var taskArg))
            {
                // Task of array OR Task of nullable OR Task of type that contains unsupported nested pattern
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

    private static bool IsAllowedClassType(INamedTypeSymbol type)
    {
        // Allow string and common date types; Uri should warn; JSObject etc. could be allowed but keeping minimal per request
        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (fullName is "global::System.String" or "global::System.Exception" or "global::System.Runtime.InteropServices.JavaScript.JSObject")
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

    private static void ReportUnsupported(SymbolAnalysisContext context, string memberName, string pos, ITypeSymbol providedType, ISymbol symbol)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        // Use minimally qualified format to resemble user code; include generics like Task<int[]>
        var typeText = providedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(UnsupportedTypeRule, location, memberName, typeText, pos));
    }

    private static void ReportUnderDevelopment(SymbolAnalysisContext context, string memberName, string pos, ITypeSymbol providedType, ISymbol symbol)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        var typeText = providedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(UnderDevelopmentTypeRule, location, memberName, typeText, pos));
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
