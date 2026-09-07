using Microsoft.CodeAnalysis;

namespace TypeShim.Shared;

internal static class InheritanceFacts
{
    internal static ISymbol? GetUnsupportedBaseOrInterface(INamedTypeSymbol type)
    {
        if (type.BaseType is INamedTypeSymbol baseType && baseType.SpecialType != SpecialType.System_Object)
        {
            return baseType;
        }

        foreach (INamedTypeSymbol interfaceType in type.Interfaces)
        {
            if (interfaceType.SpecialType != SpecialType.System_IDisposable)
            {
                return interfaceType;
            }
        }

        return null;
    }
}
