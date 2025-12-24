using Microsoft.CodeAnalysis;
using TypeShim.Core;

internal class MethodParameterInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod)
{
    internal IEnumerable<MethodParameterInfo> Build()
    {
        if (!memberMethod.IsStatic)
        {
            yield return new MethodParameterInfo
            {
                Name = "instance",
                IsInjectedInstanceParameter = true,
                Type = new InteropTypeInfoBuilder(classSymbol).Build()
            };
        }

        foreach (IParameterSymbol parameterSymbol in memberMethod.Parameters)
        {
            yield return new MethodParameterInfo
            {
                Name = parameterSymbol.Name,
                IsInjectedInstanceParameter = false,
                Type = new InteropTypeInfoBuilder(parameterSymbol.Type).Build()
            };
        }
    }
}
