using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using TypeShim.Core;

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

    private static void CheckType(SymbolAnalysisContext context, ISymbol method, ITypeSymbol type)
    {
        try
        {
            InteropTypeInfoBuilder builder = new(type);
            InteropTypeInfo info = builder.Build();
            InteropTypeInfo innermostTypeInfo = info.TypeArgument ?? info; // array, task, nullable -> inner type
            if (info.RequiresCLRTypeConversion && !innermostTypeInfo.IsTSExport)
            {
                // needs to traverse interop boundary as object or JSObject and be converted in CLR
                // however type is not TSExport, no conversion possible.
                ReportNonTSExported(context, type, method);
            }
        }
        catch (TypeNotSupportedException)
        {
            ReportUnsupported(context, type, method);
        }
        catch (NotImplementedException)
        {
            ReportUnderDevelopment(context, type, method);
        }
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
