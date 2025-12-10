using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TsExportStaticMembersAnalyzer : DiagnosticAnalyzer
{
    public const string Id = "TSHIM004";

    private static readonly DiagnosticDescriptor Rule = new(
        id: Id,
        title: "Static members on TSExport classes are not exported",
        messageFormat: "Public static member '{0}' will not be TSExported, consider making it non-static, lowering its visibility or moving it to a [TSModule]",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "TSExport classes expose instance members in TypeScript; static members are not exported.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

        foreach (var member in type.GetMembers())
        {
            switch (member)
            {
                case IMethodSymbol method when method.MethodKind == MethodKind.Ordinary && method.DeclaredAccessibility == Accessibility.Public && method.IsStatic:
                    Report(context, method, method.Name);
                    break;
                case IPropertySymbol prop when prop.DeclaredAccessibility == Accessibility.Public && prop.IsStatic:
                    Report(context, prop, prop.Name);
                    break;
            }
        }
    }

    private static void Report(SymbolAnalysisContext context, ISymbol symbol, string name)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, name));
    }
}
