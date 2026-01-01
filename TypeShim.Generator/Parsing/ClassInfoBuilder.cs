using Microsoft.CodeAnalysis;
using TypeShim.Shared;

namespace TypeShim.Generator.Parsing;

internal sealed class ClassInfoBuilder(INamedTypeSymbol classSymbol, InteropTypeInfoCache typeInfoCache)
{
    internal ClassInfo Build()
    {
        ThrowIfContainsRequiredFields();

        List<PropertyInfo> properties = BuildProperties();
        return new ClassInfo
        {
            Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
            Name = classSymbol.Name,
            IsStatic = classSymbol.IsStatic,
            Type = new InteropTypeInfoBuilder(classSymbol, typeInfoCache).Build(),
            Constructor = BuildConstructor(properties),
            Methods = BuildMethods(),
            Properties = properties,
        };
    }

    private ConstructorInfo? BuildConstructor(List<PropertyInfo> properties)
    {
        IMethodSymbol[] constructorMethods = [.. classSymbol.Constructors.Where(m => m.DeclaredAccessibility == Accessibility.Public && !m.IsStatic)];
        return constructorMethods switch
        {
            { Length: 0 } => null,
            { Length: > 1 } => throw new NotSupportedConstructorOverloadException("Overloaded constructors are not supported."),
            [ IMethodSymbol constructor ] => new ConstructorInfoBuilder(classSymbol, constructor, typeInfoCache)
                .Build(properties),
        };
    }

    private List<PropertyInfo> BuildProperties()
    {
        List<PropertyInfo> propertyInfoBuilders = [];
        foreach (IPropertySymbol propertySymbol in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (propertySymbol.DeclaredAccessibility != Accessibility.Public)
            {
                if (propertySymbol.IsRequired) throw new NotSupportedPropertyException($"Required property '{propertySymbol.Name}' is less visible than '{classSymbol.Name}'. This is invalid syntax.");
                continue;
            }
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
                throw new NotSupportedMethodOverloadException($"Method '{classSymbol.Name}.{methodSymbol.Name}' is overloaded, which is not supported.");
            }
            MethodInfoBuilder methodInfoBuilder = new(classSymbol, methodSymbol, typeInfoCache);
            methodInfos.Add(methodSymbol.Name, methodInfoBuilder.Build());
        }

        return [.. methodInfos.Values];
    }

    private void ThrowIfContainsRequiredFields()
    {
        foreach (IFieldSymbol fieldSymbol in classSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (fieldSymbol.DeclaredAccessibility != Accessibility.Public)
                continue;
            if (fieldSymbol.IsRequired)
            {
                throw new NotSupportedFieldException($"Field '{classSymbol.Name}.{fieldSymbol.Name}' is required. Required fields are currently not supported.");
            }
        }
    }
}
