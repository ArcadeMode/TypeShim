using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using TypeShim.Core;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TSJSObjectMappingAnalyzer : DiagnosticAnalyzer
{
    public const string NonPublicSetterRuleId = "TSHIM009";
    public const string NonCompatiblePropertyRuleId = "TSHIM010";

    private static readonly DiagnosticDescriptor NonPublicSetterRule = new(
        id: NonPublicSetterRuleId,
        title: "Non-public setter/init will not be set during JSObject mapping",
        messageFormat: "Property '{0}' has a non-public setter/init accessor and will not be set when mapping from a JSObject",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "TypeShim generates a JSObject mapper for each TSExport class. Properties with non-public set/init accessors are excluded from the generated JSObject mapper.");

    private static readonly DiagnosticDescriptor NonCompatiblePropertyRule = new(
        id: NonCompatiblePropertyRuleId,
        title: "Property must have a TypeShim/.NET Interop-supported type to opt-in to generated JSObject mapping",
        messageFormat: "Property '{0}' has unsupported type '{1}' and will not be set when mapping from a JSObject",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "TypeShim generates a JSObject mapper for each TSExport class. Required properties must be of TypeShim/.NET Interop-supported types.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [NonCompatiblePropertyRule, NonPublicSetterRule];

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
                Location location = p.Locations.Length > 0 ? p.Locations[0] : Location.None;
                context.ReportDiagnostic(Diagnostic.Create(NonPublicSetterRule, location, p.Name));
            }
            else if (!IsSnapshotCompatibleProperty(p))
            {
                //Debugger.Launch();
                Location location = p.Locations.Length > 0 ? p.Locations[0] : Location.None;
                DiagnosticDescriptor rule = NonCompatiblePropertyRule;
                DiagnosticSeverity severity = p.IsRequired ? DiagnosticSeverity.Error : rule.DefaultSeverity;
                context.ReportDiagnostic(Diagnostic.Create(rule, location, effectiveSeverity: severity, additionalLocations: null, properties: null, p.Name, p.Type));
            }
        }

        static ImmutableArray<IPropertySymbol> GetInstanceProperties(INamedTypeSymbol t)
            => [.. t.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer)];
    }

    private static bool IsSnapshotCompatibleProperty(IPropertySymbol property)
    {
        try
        {
            InteropTypeInfo typeInfo = new InteropTypeInfoBuilder(property.Type).Build();
            return typeInfo.IsSnapshotCompatible;
        } 
        catch (TypeShimException)
        {
            return false; // info builder throws for unsupported types (or not yet supported types)
        }
    }
}
