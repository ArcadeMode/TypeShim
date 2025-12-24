using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using TypeShim.Core;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TSJSObjectMappingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            TypeShimDiagnostics.NonPublicSetterRule,
        ];

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
        if (!hasTSExport)
            return;

        AnalyzeClassPropertiesForSnapshotCompatibility(context, type);
    }

    private static void AnalyzeClassPropertiesForSnapshotCompatibility(SymbolAnalysisContext context, INamedTypeSymbol type)
    {
        foreach (IPropertySymbol p in GetInstanceProperties(type))
        {
            if (p.SetMethod is not IMethodSymbol setter)
            {
                continue;
            }

            Accessibility propertyAccessibility = p.DeclaredAccessibility is Accessibility.NotApplicable ? Accessibility.Private : p.DeclaredAccessibility;
            Accessibility setterAccessibility = setter.DeclaredAccessibility is Accessibility.NotApplicable ? propertyAccessibility : setter.DeclaredAccessibility;

            if (setterAccessibility is not Accessibility.Public)
            {
                Report(context, TypeShimDiagnostics.NonPublicSetterRule, p, p.Type);
            }
        }

        static ImmutableArray<IPropertySymbol> GetInstanceProperties(INamedTypeSymbol t)
            => [.. t.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer)];
    }

    private static void Report(SymbolAnalysisContext context, DiagnosticDescriptor descriptor, ISymbol symbol, ITypeSymbol type)
    {
        Location location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        string propTypeName = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        context.ReportDiagnostic(Diagnostic.Create(descriptor, location, symbol.Name, propTypeName));
    }
}
