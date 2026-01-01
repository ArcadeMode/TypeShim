using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TypeShim.Shared;

namespace TypeShim.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal sealed class TypeShimAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        TypeShimDiagnostics.AttributeOnPublicClassOnlyRule,
        TypeShimDiagnostics.NonPublicSetterRule,
        TypeShimDiagnostics.NoRequiredFieldsRule,
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
        if (!hasTSExport)
            return;


        AnalyzeClassAccessibility(context, type);
        AnalyzeTypesUsedInInteropApi(context, type);
        AnalyzeClassPropertiesForConstructionCompatibility(context, type);
    }

    private static void AnalyzeClassAccessibility(SymbolAnalysisContext context, INamedTypeSymbol type)
    {
        if (SymbolFacts.IsPublicClass(type)) return;
        Report(context, TypeShimDiagnostics.AttributeOnPublicClassOnlyRule, type, type.Name);
    }

    private static void AnalyzeTypesUsedInInteropApi(SymbolAnalysisContext context, INamedTypeSymbol type)
    {
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
                    CheckInstancePropertySetterAccessibility(context, prop);
                    CheckType(context, prop, prop.Type);
                    break;
                case IFieldSymbol field:
                    CheckInstanceFieldRequiredness(context, field);
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
            if (info.RequiresTypeConversion && !info.SupportsTypeConversion)
            {
                ReportNonTSExported(context, type, symbol);
            }
        }
        catch (NotSupportedTypeException)
        {
            ReportUnsupported(context, type, symbol);
        }
        catch (NotImplementedException)
        {
            ReportUnderDevelopment(context, type, symbol);
        }
    }

    private static void CheckInstancePropertySetterAccessibility(SymbolAnalysisContext context, IPropertySymbol property)
    {
        if (property.IsStatic || property.IsIndexer || property.SetMethod is not IMethodSymbol setter) 
            return;

        Accessibility propertyAccessibility = property.DeclaredAccessibility is Accessibility.NotApplicable ? Accessibility.Private : property.DeclaredAccessibility;
        Accessibility setterAccessibility = setter.DeclaredAccessibility is Accessibility.NotApplicable ? propertyAccessibility : setter.DeclaredAccessibility;
        if (setterAccessibility is Accessibility.Public) 
            return;
        
        Report(context, TypeShimDiagnostics.NonPublicSetterRule, property, property.Type);
    }

    private static void CheckInstanceFieldRequiredness(SymbolAnalysisContext context, IFieldSymbol field)
    {
        if (field.IsStatic || field.IsConst || field.IsImplicitlyDeclared)
            return;
        
        if (field.DeclaredAccessibility == Accessibility.Public && field.IsRequired)
            Report(context, TypeShimDiagnostics.NoRequiredFieldsRule, field, field.Name);
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

    private static void Report(SymbolAnalysisContext context, DiagnosticDescriptor descriptor, ISymbol symbol, ITypeSymbol type)
    {
        string propTypeName = type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        Report(context, descriptor, symbol, propTypeName);
    }

    private static void Report(SymbolAnalysisContext context, DiagnosticDescriptor descriptor, ISymbol symbol, params object[] messageParams)
    {
        Location location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
        context.ReportDiagnostic(Diagnostic.Create(descriptor, location, symbol.Name, messageParams));
    }
}
