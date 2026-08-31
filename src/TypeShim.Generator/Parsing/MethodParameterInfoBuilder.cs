using Microsoft.CodeAnalysis;
using TypeShim.Shared;

internal class MethodParameterInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod, InteropTypeInfoCache typeInfoCache)
{
    internal MethodParameterInfo? BuildInstanceParameter()
    {
        if (!memberMethod.IsStatic && memberMethod.MethodKind is not MethodKind.Constructor)
        {
            return new MethodParameterInfo
            {
                Name = "instance",
                Type = new InteropTypeInfoBuilder(classSymbol, typeInfoCache).Build()
            };
        }

        return null;
    }

    internal IEnumerable<MethodParameterInfo> Build()
    {
        foreach (IParameterSymbol parameterSymbol in memberMethod.Parameters)
        {
            yield return new MethodParameterInfo
            {
                Name = parameterSymbol.Name,
                Type = new InteropTypeInfoBuilder(parameterSymbol.Type, typeInfoCache).Build()
            };
        }
    }
}
