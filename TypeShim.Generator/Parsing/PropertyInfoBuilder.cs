using Microsoft.CodeAnalysis;
using System.Reflection;
using TypeShim.Generator;
using TypeShim.Generator.Parsing;

internal sealed class PropertyInfoBuilder(INamedTypeSymbol classSymbol, IPropertySymbol propertySymbol)
{
    private readonly InteropTypeInfoBuilder typeInfoBuilder = new(propertySymbol.Type);

    internal PropertyInfo Build()
    {
        if (propertySymbol.DeclaredAccessibility != Accessibility.Public)
        {
            throw new UnsupportedPropertyException($"Property {classSymbol}.{propertySymbol} must have accessibility 'Public'.");
        }

        InteropTypeInfo propertyTypeInfo = typeInfoBuilder.Build();
        if (propertySymbol.GetMethod is not IMethodSymbol methodSymbol)
        {
            throw new UnsupportedPropertyException("Properties without get are not supported");
        }
        
        MethodInfoBuilder methodInfoBuilder = new(classSymbol, methodSymbol);
        MethodInfo getMethod = methodInfoBuilder.Build();

        MethodInfo? setMethod = null;
        if (propertySymbol.SetMethod is IMethodSymbol setMethodSymbol && !setMethodSymbol.IsInitOnly && setMethodSymbol.DeclaredAccessibility == Accessibility.Public)
        {
            MethodInfoBuilder setMethodInfoBuilder = new(classSymbol, setMethodSymbol);
            setMethod = setMethodInfoBuilder.Build();
        }
        
        return new PropertyInfo
        {
            Name = propertySymbol.Name,
            IsStatic = propertySymbol.IsStatic,
            Type = typeInfoBuilder.Build(),
            GetMethod = getMethod,
            SetMethod = setMethod,
        };
    }
}
