using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        TypeShimDiagnostics.NoOverloadsRule,
        TypeShimDiagnostics.UnsupportedTypeRule,
        TypeShimDiagnostics.NonExportedTypeInInteropApiRule,
        TypeShimDiagnostics.UnderDevelopmentTypeRule,
        TypeShimDiagnostics.NoGenericsTSExportRule,
        TypeShimDiagnostics.NoGenericsPublicMethodRule
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
        //Debugger.Launch();
        if (TryGetTypeDiagnostic(type) is DiagnosticDescriptor descriptor)
        {
            context.ReportDiagnostic(Diagnostic.Create(descriptor, LocationFinder.GetDefaultLocation(type), type.Name));
        }

        AnalyzeClassAccessibility(context, type);
        AnalyzeMembers(context, type);
    }

    private static void AnalyzeClassAccessibility(SymbolAnalysisContext context, INamedTypeSymbol classSymbol)
    {
        if (SymbolFacts.IsPublicClass(classSymbol)) return;

        context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.AttributeOnPublicClassOnlyRule, LocationFinder.GetDefaultLocation(classSymbol), classSymbol.Name));
    }

    private static void AnalyzeMembers(SymbolAnalysisContext context, INamedTypeSymbol type)
    {
        HashSet<string> seenMethodNames = [];
        foreach (ISymbol member in type.GetMembers())
        {
            if (member.DeclaredAccessibility != Accessibility.Public)
                continue;
            
            switch (member)
            {
                case IMethodSymbol method when method.MethodKind is MethodKind.Ordinary or MethodKind.Constructor:
                    CheckForOverloads(context, seenMethodNames, method);
                    CheckMethodReturnType(context, method);
                    CheckNoGenericsInMethod(context, method);
                    foreach (IParameterSymbol parameter in method.Parameters)
                        CheckMethodParameterType(context, method, parameter);
                    break;
                case IPropertySymbol prop:
                    CheckInstancePropertySetterAccessibility(context, prop);
                    CheckPropertyType(context, prop);
                    break;
                case IFieldSymbol field:
                    CheckInstanceFieldRequiredness(context, field);
                    break;
            }
        }
    }

    private static void CheckForOverloads(SymbolAnalysisContext context, HashSet<string> seenMethodNames, IMethodSymbol member)
    {
        if (seenMethodNames.Contains(member.Name))
        {
            context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.NoOverloadsRule, LocationFinder.GetDefaultLocation(member), member.Name));
        }
        else
        {
            seenMethodNames.Add(member.Name);
        }
    }
    private static void CheckNoGenericsInMethod(SymbolAnalysisContext context, IMethodSymbol member)
    {
        if (member.Arity == 0) return;

        context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.NoGenericsPublicMethodRule, LocationFinder.GetDefaultLocation(member), member.Name));
    }

    private static void CheckMethodReturnType(SymbolAnalysisContext context, IMethodSymbol method)
    {
        if (TryGetTypeDiagnostic(method.ReturnType) is DiagnosticDescriptor descriptor)
        {
            Location location = LocationFinder.GetMethodReturnTypeLocation(method, context.CancellationToken);
            string typeName = method.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, typeName));
        }
    }

    private static void CheckMethodParameterType(SymbolAnalysisContext context, IMethodSymbol method, IParameterSymbol parameter)
    {
        if (TryGetTypeDiagnostic(parameter.Type) is DiagnosticDescriptor descriptor)
        {
            Location location = LocationFinder.GetMethodParameterLocation(method, parameter, context.CancellationToken);
            string typeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, typeName));
        }
    }

    private static void CheckPropertyType(SymbolAnalysisContext context, IPropertySymbol property)
    {
        if (TryGetTypeDiagnostic(property.Type) is DiagnosticDescriptor descriptor)
        {
            Location location = LocationFinder.GetPropertyTypeLocation(property, context.CancellationToken);
            string typeName = property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, typeName));
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

        string propertyName = property.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.NonPublicSetterRule, LocationFinder.GetDefaultLocation(property), propertyName));
    }

    private static void CheckInstanceFieldRequiredness(SymbolAnalysisContext context, IFieldSymbol field)
    {
        if (field.IsStatic || field.IsConst || field.IsImplicitlyDeclared)
            return;

        if (field.DeclaredAccessibility == Accessibility.Public && field.IsRequired)
        {
            string fieldName = field.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.NoRequiredFieldsRule, LocationFinder.GetDefaultLocation(field), fieldName));
        }
    }

    private static DiagnosticDescriptor? TryGetTypeDiagnostic(ITypeSymbol type)
    {
        try
        {
            InteropTypeInfoBuilder builder = new(type, new InteropTypeInfoCache());
            InteropTypeInfo info = builder.Build();
            if (info.RequiresTypeConversion && !info.SupportsTypeConversion)
            {
                return TypeShimDiagnostics.NonExportedTypeInInteropApiRule;
            }
        }
        catch (NotSupportedTypeException)
        {
            return TypeShimDiagnostics.UnsupportedTypeRule;
        }
        catch (NotImplementedException)
        {
            return TypeShimDiagnostics.UnderDevelopmentTypeRule;
        }
        catch (NotSupportedGenericClassException)
        {
            return TypeShimDiagnostics.NoGenericsTSExportRule;
        }
        return null;
    }
}
