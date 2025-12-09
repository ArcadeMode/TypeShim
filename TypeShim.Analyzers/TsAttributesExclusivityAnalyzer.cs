using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TsAttributesExclusivityAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TSHIM001";
    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "TSModule and TSExport cannot be applied together",
        messageFormat: "Class '{0}' has both [TSModule] and [TSExport]; these attributes are mutually exclusive",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TSModule classes are entry points; TSExport marks user classes. They cannot be combined.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        bool hasTSModule = HasAttribute(type, "TypeShim.TSModuleAttribute");
        bool hasTSExport = HasAttribute(type, "TypeShim.TSExportAttribute");

        if (hasTSModule && hasTSExport)
        {
            var location = type.Locations.Length > 0 ? type.Locations[0] : Location.None;
            var diagnostic = Diagnostic.Create(Rule, location, type.Name);
            context.ReportDiagnostic(diagnostic);
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
}
