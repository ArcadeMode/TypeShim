using Microsoft.CodeAnalysis;
using TypeShim.Shared;

namespace TypeShim.Generator.Parsing;

internal sealed class ClassInfoBuilder(INamedTypeSymbol classSymbol, InteropTypeInfoCache typeInfoCache)
{
    internal ClassInfoBuilder(INamedTypeSymbol classSymbol) : this(classSymbol, new InteropTypeInfoCache())
    {
    }

    internal ClassInfo Build()
    {
        return new ClassInfo
        {
            Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Name = classSymbol.Name,
            IsStatic = classSymbol.IsStatic,
            Type = new InteropTypeInfoBuilder(classSymbol, typeInfoCache).Build(),
            Constructor = BuildConstructor(),
            Methods = BuildMethods(),
            Properties = BuildProperties(),
        };
    }

    private MethodInfo? BuildConstructor()
    {
        IMethodSymbol[] constructorMethods = [.. classSymbol.Constructors.Where(m => m.DeclaredAccessibility == Accessibility.Public && !m.IsStatic)];
        return constructorMethods switch
        {
            { Length: 0 } => null,
            { Length: > 1 } => throw new UnsupportedConstructorOverloadException("Overloaded constructors are not supported."),
            [ IMethodSymbol constructor ] => new MethodInfoBuilder(classSymbol, constructor, typeInfoCache).Build(),
        };
    }

    private List<PropertyInfo> BuildProperties()
    {
        List<PropertyInfo> propertyInfoBuilders = new();
        IEnumerable<IPropertySymbol> propertySymbols = classSymbol.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public);
        foreach (IPropertySymbol propertySymbol in propertySymbols)
        {
            PropertyInfoBuilder propertyInfoBuilder = new(classSymbol, propertySymbol, typeInfoCache);
            propertyInfoBuilders.Add(propertyInfoBuilder.Build());
        }

        return propertyInfoBuilders;
    }

    private List<MethodInfo> BuildMethods()
    {
        Dictionary<string, MethodInfo> methodInfos = [];
        IEnumerable<IMethodSymbol> methodSymbols = classSymbol.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary && m.DeclaredAccessibility == Accessibility.Public);
        foreach (IMethodSymbol methodSymbol in methodSymbols)
        {
            if (methodInfos.ContainsKey(methodSymbol.Name))
            {
                throw new UnsupportedMethodOverloadException($"Class {classSymbol.Name} contains overloaded method '{methodSymbol.Name}'. Overloading is not supported.");
            }
            MethodInfoBuilder methodInfoBuilder = new(classSymbol, methodSymbol, typeInfoCache);
            methodInfos.Add(methodSymbol.Name, methodInfoBuilder.Build());
        }

        return [.. methodInfos.Values];
    }
}
