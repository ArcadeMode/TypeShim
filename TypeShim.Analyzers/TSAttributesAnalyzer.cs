using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TSAttributesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            TypeShimDiagnostics.AttributeOnPublicClassOnlyRule,
        ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeClass, SymbolKind.NamedType);
    }

    private static void AnalyzeClass(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol type)
            return;

        bool hasTSExport = HasAttribute(type, "TypeShim.TSExportAttribute");

        if (hasTSExport && !IsPublicClass(type))
        {
            var location = type.Locations.Length > 0 ? type.Locations[0] : Location.None;
            context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.AttributeOnPublicClassOnlyRule, location, type.Name));
            return;
        }

        if (hasTSExport) 
        {
            // TODO: add parameterless constructor check >> BROADER: check if can be constructed (parameters are interopable/properties are etc)
            // TODO: add check for 'no required members that cannot be snapshotted'
        }
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

    private static bool IsPublicClass(INamedTypeSymbol type)
        => type.TypeKind == TypeKind.Class && type.DeclaredAccessibility == Accessibility.Public;
}
