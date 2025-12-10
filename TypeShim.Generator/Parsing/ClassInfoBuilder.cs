using Microsoft.CodeAnalysis;

namespace TypeShim.Generator.Parsing;

internal sealed class ClassInfoBuilder(INamedTypeSymbol classSymbol)
{
    internal ClassInfo Build()
    {
        List<MethodInfoBuilder> methodInfoBuilders = new();
        IEnumerable<IMethodSymbol> methodSymbols = classSymbol.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary && m.DeclaredAccessibility == Accessibility.Public);
        foreach (IMethodSymbol methodSymbol in methodSymbols)
        {
            MethodInfoBuilder methodInfoBuilder = new(classSymbol, methodSymbol);
            methodInfoBuilders.Add(methodInfoBuilder);
        }

        List<PropertyInfoBuilder> propertyInfoBuilders = new();
        IEnumerable<IPropertySymbol> propertySymbols = classSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public);
        foreach (IPropertySymbol propertySymbol in propertySymbols)
        {
            PropertyInfoBuilder propertyInfoBuilder = new(classSymbol, propertySymbol);
            propertyInfoBuilders.Add(propertyInfoBuilder);
        }

        return new ClassInfo
        {
            IsModule = classSymbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name is "TSModuleAttribute" or "TSModule"),
            Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Name = classSymbol.Name,
            Type = new InteropTypeInfoBuilder(classSymbol).Build(),
            Methods = [.. methodInfoBuilders.Select(b => b.Build())],
            Properties = [.. propertyInfoBuilders.Select(b => b.Build())],
        };
    }
}
