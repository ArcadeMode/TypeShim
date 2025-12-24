using Microsoft.CodeAnalysis;
using TypeShim.Shared;

internal sealed class PropertyInfoBuilder(INamedTypeSymbol classSymbol, IPropertySymbol propertySymbol, InteropTypeInfoCache typeInfoCache)
{
    private readonly InteropTypeInfoBuilder typeInfoBuilder = new(propertySymbol.Type, typeInfoCache);

    internal PropertyInfo Build()
    {
        if (propertySymbol.DeclaredAccessibility != Accessibility.Public)
        {
            throw new UnsupportedPropertyException($"Property {classSymbol}.{propertySymbol} must have accessibility 'Public'.");
        }

        if (propertySymbol.GetMethod is not IMethodSymbol methodSymbol)
        {
            throw new UnsupportedPropertyException("Properties without get are not supported");
        }

        MethodInfoBuilder methodInfoBuilder = new(classSymbol, methodSymbol, typeInfoCache);
        MethodInfo getMethod = methodInfoBuilder.Build();

        MethodInfo? setMethod = null;
        if (propertySymbol.SetMethod is IMethodSymbol setMethodSymbol && !setMethodSymbol.IsInitOnly && setMethodSymbol.DeclaredAccessibility == Accessibility.Public)
        {
            MethodInfoBuilder setMethodInfoBuilder = new(classSymbol, setMethodSymbol, typeInfoCache);
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
