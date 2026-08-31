using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        TypeShimDiagnostics.NoRequiredFieldsRule,
        TypeShimDiagnostics.NoOverloadsRule,
        TypeShimDiagnostics.UnsupportedTypeRule,
        TypeShimDiagnostics.NonExportedTypeInInteropApiRule,
        TypeShimDiagnostics.UnderDevelopmentTypeRule,
        TypeShimDiagnostics.NoGenericsTSExportRule,
        TypeShimDiagnostics.NoGenericsPublicMethodRule,
        TypeShimDiagnostics.MixedExportRule,
        TypeShimDiagnostics.UnresolvableDefaultConstRule,
        TypeShimDiagnostics.NoOptionalMemoryViewRule,
        TypeShimDiagnostics.NoOptionalCtorParamWithRequiredInitializerRule
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeClass, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeMethodForMixedExport, SymbolKind.Method);
        context.RegisterSyntaxNodeAction(CheckOptionalParameterDefault, SyntaxKind.Parameter);
    }

    private static void AnalyzeMethodForMixedExport(SymbolAnalysisContext context)
    {
        IMethodSymbol methodSymbol = (IMethodSymbol)context.Symbol;
        bool hasJSExport = SymbolFacts.HasJSExportAttribute(methodSymbol);
        if (!hasJSExport) return;

        bool classHasTSExport = SymbolFacts.HasTSExportAttribute(methodSymbol.ContainingType);
        if (classHasTSExport)
        {
            Diagnostic diagnostic = Diagnostic.Create(TypeShimDiagnostics.MixedExportRule, methodSymbol.Locations[0], methodSymbol.Name, methodSymbol.ContainingType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeClass(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol type || type.TypeKind != TypeKind.Class)
            return;

        bool hasTSExport = SymbolFacts.HasTSExportAttribute(type);
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
                    if (method.MethodKind is MethodKind.Constructor)
                        CheckOptionalConstructorParameter(context, type, method);
                    break;
                case IPropertySymbol prop:
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

    private static void CheckOptionalParameterDefault(SyntaxNodeAnalysisContext context)
    {
        var parameterNode = (ParameterSyntax)context.Node;
        if (parameterNode.Default is not { Value: ExpressionSyntax defaultExpr })
            return;

        if (context.SemanticModel.GetDeclaredSymbol(parameterNode, context.CancellationToken) is not IParameterSymbol parameter)
            return;

        if (parameter.ContainingSymbol is not IMethodSymbol method
            || method.DeclaredAccessibility != Accessibility.Public
            || method.MethodKind is not (MethodKind.Ordinary or MethodKind.Constructor)
            || !SymbolFacts.HasTSExportAttribute(method.ContainingType))
        {
            return;
        }

        Location location = parameterNode.Type?.GetLocation() ?? parameterNode.GetLocation();

        if (IsSpanOrArraySegment(parameter.Type))
        {
            string typeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.NoOptionalMemoryViewRule, location, parameter.Name, typeName));
            return;
        }

        // The generator resolves defaults against a compilation that only preserves [TSExport] classes whole.
        // A default referencing a user-declared constant outside that surface cannot be resolved. Enum members
        // are excluded because enums cross as their underlying value, which is always resolvable.
        foreach (SyntaxNode node in defaultExpr.DescendantNodesAndSelf())
        {
            if (node is not SimpleNameSyntax)
                continue;

            if (context.SemanticModel.GetSymbolInfo(node, context.CancellationToken).Symbol is IFieldSymbol { IsConst: true } field
                && field.ContainingType.TypeKind != TypeKind.Enum
                && field.Locations.Any(l => l.IsInSource)
                && !SymbolFacts.HasTSExportAttribute(field.ContainingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(TypeShimDiagnostics.UnresolvableDefaultConstRule, location, parameter.Name, field.Name));
                return;
            }
        }
    }

    private static bool IsSpanOrArraySegment(ITypeSymbol type)
    {
        ITypeSymbol effective = type;
        if (SymbolFacts.IsNullable(type) && type is INamedTypeSymbol { TypeArguments.Length: 1 } nullable)
            effective = nullable.TypeArguments[0];

        string fullName = effective.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        return fullName.StartsWith(Constants.SpanGlobal, StringComparison.Ordinal)
            || fullName.StartsWith(Constants.ArraySegmentGlobal, StringComparison.Ordinal);
    }

    private static void CheckOptionalConstructorParameter(SymbolAnalysisContext context, INamedTypeSymbol type, IMethodSymbol constructor)
    {
        IParameterSymbol? optionalParameter = constructor.Parameters.FirstOrDefault(p => p.HasExplicitDefaultValue);
        if (optionalParameter is null)
            return;

        bool hasNonOmittableInitializerMember = type.GetMembers().OfType<IPropertySymbol>().Any(IsNonOmittableInitializerMember);
        if (!hasNonOmittableInitializerMember)
            return;

        Location location = LocationFinder.GetMethodParameterLocation(constructor, optionalParameter, context.CancellationToken);
        context.ReportDiagnostic(Diagnostic.Create(
            TypeShimDiagnostics.NoOptionalCtorParamWithRequiredInitializerRule, location, optionalParameter.Name, type.Name));
    }

    private static bool IsNonOmittableInitializerMember(IPropertySymbol property)
    {
        // Matches the member-initializer set: public property with a public set/init accessor.
        if (property.DeclaredAccessibility != Accessibility.Public
            || property.SetMethod is not { DeclaredAccessibility: Accessibility.Public })
        {
            return false;
        }

        // Non-nullable members are mandatory in the generated interop; nullable ones can be omitted.
        return property.Type.NullableAnnotation != NullableAnnotation.Annotated;
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

            // A non-[TSExport] enum on the boundary is stripped from codegen and fails there, so flag it
            // like a non-exported class. (Enums otherwise 'support' conversion since they cross as a number.)
            if (info.GetInnermostType() is { IsEnum: true, IsTSExport: false })
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
