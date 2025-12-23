using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TSAttributesAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "TSHIM001";
    public const string ModuleStaticRuleId = "TSHIM002";
    public const string ExportNonStaticRuleId = "TSHIM003";
    public const string PublicClassOnlyRuleId = "TSHIM008";

    private static readonly DiagnosticDescriptor ExclusivityRule = new(
        id: DiagnosticId,
        title: "TSModule and TSExport cannot be applied together",
        messageFormat: "Class '{0}' has both [TSModule] and [TSExport]; these attributes are mutually exclusive",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TSModule classes are entry points; TSExport marks user classes. They cannot be combined.");

    private static readonly DiagnosticDescriptor ModuleMustBeStaticRule = new(
        id: ModuleStaticRuleId,
        title: "TSModule can only be applied to static classes",
        messageFormat: "Classes with [TSModule] must be static",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[TSModule] must be applied to static classes.");

    private static readonly DiagnosticDescriptor ExportMustBeNonStaticRule = new(
        id: ExportNonStaticRuleId,
        title: "TSExport can only be applied to non-static classes",
        messageFormat: "Classes with [TSExport] must be non-static",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[TSExport] must be applied to non-static classes.");

    private static readonly DiagnosticDescriptor PublicClassOnlyRule = new(
        id: PublicClassOnlyRuleId,
        title: "TSModule and TSExport can only be applied to classes with public accessibility",
        messageFormat: "'{0}' has either [TSModule] or [TSExport] and is not a class or has non-public accessibility",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TSModule and TSExport are only supported on classes with public accessibility.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ExclusivityRule, ModuleMustBeStaticRule, ExportMustBeNonStaticRule, PublicClassOnlyRule];

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

        bool hasTSModule = HasAttribute(type, "TypeShim.TSModuleAttribute");
        bool hasTSExport = HasAttribute(type, "TypeShim.TSExportAttribute");
        bool isStaticClass = type.IsStatic;

        if ((hasTSExport || hasTSModule) && !IsPublicClass(type))
        {
            var location = type.Locations.Length > 0 ? type.Locations[0] : Location.None;
            context.ReportDiagnostic(Diagnostic.Create(PublicClassOnlyRule, location, type.Name));
            return;
        }

        if (hasTSModule && hasTSExport)
        {
            var location = type.Locations.Length > 0 ? type.Locations[0] : Location.None;
            context.ReportDiagnostic(Diagnostic.Create(ExclusivityRule, location, type.Name));
            return;
        }

        if (hasTSModule && !isStaticClass)
        {
            var location = type.Locations.Length > 0 ? type.Locations[0] : Location.None;
            context.ReportDiagnostic(Diagnostic.Create(ModuleMustBeStaticRule, location, type.Name));
            return;
        }

        if (hasTSExport && isStaticClass)
        {
            var location = type.Locations.Length > 0 ? type.Locations[0] : Location.None;
            context.ReportDiagnostic(Diagnostic.Create(ExportMustBeNonStaticRule, location, type.Name));
            return;
        }

        if (hasTSExport) 
        {
            // TODO: add parameterless constructor check
            // TODO: add check for 'no required members that cannot be snapshotted'
        }
        // Add
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
