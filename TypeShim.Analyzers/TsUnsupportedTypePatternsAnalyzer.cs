using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

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

        bool hasTSExport = SymbolFacts.HasAttribute(type, "TypeShim.TSExportAttribute");
        bool hasTSModule = SymbolFacts.HasAttribute(type, "TypeShim.TSModuleAttribute");
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
                    CheckType(context, prop, prop.Type);
                    break;
            }
        }
    }

    private static void CheckType(SymbolAnalysisContext context, ISymbol method, ITypeSymbol type)
    {
        if (IsClassWithoutTSExport(type))
        {
            ReportNonTSExported(context, type, method);
        }
        else if (IsUnderDevelopment(type))
        {
            ReportUnderDevelopment(context, type, method);
        }
        else if (IsUnsupported(type))
        {
            ReportUnsupported(context, type, method);
        }
    }

    private static bool IsClassWithoutTSExport(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { TypeKind: TypeKind.Class } namedClass)
        {
            return !SymbolFacts.HasAttribute(namedClass, "TypeShim.TSExportAttribute")
                && !IsClassTypeWithoutTSExportRequirement(namedClass);
        }
        return false; // not a class

        static bool IsClassTypeWithoutTSExportRequirement(INamedTypeSymbol type)
        {
            var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (fullName is "string" or "global::System.String"
                or "global::System.Exception"
                or "global::System.Runtime.InteropServices.JavaScript.JSObject"
                || fullName.StartsWith("global::System.Threading.Tasks.Task"))
            {
                return true;
            }
            return false;
        }
    }

    private static bool IsUnderDevelopment(ITypeSymbol type) // i.e. implementation planned but not done yet
    {
        if (type is INamedTypeSymbol named)
        {
            if (named.ConstructedFrom?.SpecialType == SpecialType.System_Nullable_T)
            {
                return IsUnderDevelopment(named.TypeArguments[0]);
            }

            if (SymbolFacts.IsConstructedFrom(named, "global::System.Span<T>", out ITypeSymbol? spanArg))
            {
                if (IsAllowedSpanOrArraySegmentElement(spanArg))
                    return true; // under development
                return false; // not supported by .net
            }

            if (SymbolFacts.IsConstructedFrom(named, "global::System.ArraySegment<T>", out ITypeSymbol? segArg))
            {
                if (IsAllowedSpanOrArraySegmentElement(segArg))
                    return true;
                return false; // not supported by .net
            }

            if (SymbolFacts.IsAction(named))
            {
                int arity = named.TypeArguments.Length;
                // TODO: recurse on type args, also for other diagnostics
                if (arity <= 3)
                    return true;
                return false; // not supported by .net
            }
            if (SymbolFacts.IsFunc(named))
            {
                int arity = named.TypeArguments.Length;
                if (arity <= 4)
                    return true;
                return false; // not supported by .net
            }
        }

        if (type is IArrayTypeSymbol arr)
        {
            return IsUnderDevelopment(arr.ElementType);
        }

        if (SymbolFacts.IsConstructedFrom(type, "global::System.Threading.Tasks.Task<TResult>", out var taskArg))
        {
            return taskArg != null && IsUnderDevelopment(taskArg);
        }

        return false;
    }

    private static bool IsUnsupported(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
        {
            var elem = arrayType.ElementType;
            if (SymbolFacts.IsNullable(elem))
            {
                return true; // nullable element type unsupported
            }
            return IsUnsupported(elem);
        }

        if (type is INamedTypeSymbol named)
        {
            if (SymbolFacts.IsConstructedFrom(named, "global::System.Span<T>", out var spanArg))
            {
                if (!IsAllowedSpanOrArraySegmentElement(spanArg))
                    return true;
                return false;
            }
            if (SymbolFacts.IsConstructedFrom(named, "global::System.ArraySegment<T>", out var segArg))
            {
                if (!IsAllowedSpanOrArraySegmentElement(segArg))
                    return true;
                return false;
            }
            foreach (string constructedFrom in new[]
            {
                "global::System.Threading.Tasks.ValueTask<TResult>",
                "global::System.ReadOnlySpan<T>",
                "global::System.ReadOnlyMemory<T>",
                "global::System.Memory<T>",
                "global::System.Collections.Generic.IEnumerable<T>",
                "global::System.Collections.Generic.IList<T>",
                "global::System.Collections.Generic.IReadOnlyList<T>",
                "global::System.Collections.Generic.IDictionary<TKey, TValue>",
                "global::System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>",
            })
            {
                if (SymbolFacts.IsConstructedFrom(named, constructedFrom, out _))
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
                return IsUnsupported(inner);
            }

            if (SymbolFacts.IsConstructedFrom(type, "global::System.Threading.Tasks.Task<TResult>", out ITypeSymbol? taskArg) && taskArg != null)
            {
                if (taskArg is IArrayTypeSymbol || SymbolFacts.IsNullable(taskArg) || IsUnsupported(taskArg))
                {
                    return true;
                }
                return false;
            }

            if (SymbolFacts.IsAction(named))
            {
                if (named.TypeArguments.Length > 3)
                    return true;
                return false;
            }
            if (SymbolFacts.IsFunc(named))
            {
                if (named.TypeArguments.Length > 4)
                    return true;
                return false;
            }
        }

        return false;
    }

    private static bool IsAllowedSpanOrArraySegmentElement(ITypeSymbol? elem)
    {
        if (elem == null) return false;
        return elem.SpecialType is SpecialType.System_Byte or SpecialType.System_Int32 or SpecialType.System_Double;
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

    private static void ReportNonTSExported(SymbolAnalysisContext context, ITypeSymbol type, ISymbol symbol)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        var typeText = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(NonExportedTypeRule, location, typeText));
    }
}
