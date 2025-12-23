using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TSJSObjectMappingAnalyzer : DiagnosticAnalyzer
{
    public const string RequiredNonPublicSetterRuleId = "TSHIM009";
    public const string NonRequiredNonPublicSetterRuleId = "TSHIM010";

    private static readonly DiagnosticDescriptor RequiredNonPublicSetterRule = new(
        id: RequiredNonPublicSetterRuleId,
        title: "Required property must have a public setter/init to support JSObject mapping",
        messageFormat: "Required property '{0}' on class '{1}' must have a public setter/init accessor for JSObject mapping",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TypeShim generates a JSObject mapper for each TSExport class. Required properties must be assignable via a public setter/init accessor.");

    private static readonly DiagnosticDescriptor NonRequiredNonPublicSetterRule = new(
        id: NonRequiredNonPublicSetterRuleId,
        title: "Non-public setter/init will not be set during JSObject mapping",
        messageFormat: "Property '{0}' on class '{1}' has a non-public setter/init accessor and will not be set when mapping from a JSObject",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "TypeShim generates a JSObject mapper for each TSExport class. Properties with non-public set/init accessors are excluded from the generated JSObject mapper.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RequiredNonPublicSetterRule, NonRequiredNonPublicSetterRule];

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
                DiagnosticDescriptor rule = p.IsRequired ? RequiredNonPublicSetterRule : NonRequiredNonPublicSetterRule;
                context.ReportDiagnostic(Diagnostic.Create(rule, location, p.Name, type.Name));
            }

            _ = IsSnapshotCompatibleType(p);
        }

        static ImmutableArray<IPropertySymbol> GetInstanceProperties(INamedTypeSymbol t)
            => [.. t.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsIndexer)];
    }

    // Placeholder for future type checks.
    // For now: treat required properties as not snapshot compatible.
    private static bool IsSnapshotCompatibleType(IPropertySymbol property)
        => !property.IsRequired; // TODO: the rest.
}
