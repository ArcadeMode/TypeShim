using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using TypeShim.Core;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TsUnsupportedTypePatternsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            TypeShimDiagnostics.UnsupportedTypeRule,
            TypeShimDiagnostics.NonExportedTypeInMethodRule,
            TypeShimDiagnostics.NonExportedTypeInPropertyRule,   
            TypeShimDiagnostics.UnderDevelopmentTypeRule,
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
        bool hasTSModule = SymbolFacts.HasAttribute(type, "TypeShim.TSModuleAttribute");
        if (!hasTSExport && !hasTSModule)
            return;

        //Debugger.Launch();
        foreach (ISymbol member in type.GetMembers())
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
                case IFieldSymbol field:
                    // TODO: report unsupported, do not public fields, especially not required fields.
                    break;
            }
        }
    }

    private static void CheckType(SymbolAnalysisContext context, ISymbol symbol, ITypeSymbol type)
    {
        try
        {
            InteropTypeInfoBuilder builder = new(type, new InteropTypeInfoCache());
            InteropTypeInfo info = builder.Build();
            InteropTypeInfo innermostTypeInfo = info.TypeArgument ?? info; // array, task, nullable -> inner type
            if (info.RequiresCLRTypeConversion && !innermostTypeInfo.IsTSExport)
            {
                // needs to traverse interop boundary as object or JSObject and be converted in CLR
                // however type is not TSExport, no conversion possible.
                ReportNonTSExported(context, type, symbol);
            }
        }
        catch (TypeNotSupportedException)
        {
            ReportUnsupported(context, type, symbol);
        }
        catch (NotImplementedException)
        {
            ReportUnderDevelopment(context, type, symbol);
        }
    }

    private static void ReportUnsupported(SymbolAnalysisContext context, ITypeSymbol providedType, ISymbol symbol)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        var typeText = providedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.UnsupportedTypeRule, location, typeText));
    }

    private static void ReportUnderDevelopment(SymbolAnalysisContext context, ITypeSymbol providedType, ISymbol symbol)
    {
        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        var typeText = providedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.UnderDevelopmentTypeRule, location, typeText));
    }

    private static void ReportNonTSExported(SymbolAnalysisContext context, ITypeSymbol type, ISymbol symbol)
    {
        DiagnosticDescriptor descriptor = symbol is IPropertySymbol
            ? TypeShimDiagnostics.NonExportedTypeInPropertyRule
            : TypeShimDiagnostics.NonExportedTypeInMethodRule;

        var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        var typeText = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(descriptor, location, typeText));
    }
}
