using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using TypeShim.Generator.Parsing;

internal class MethodParameterInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod)
{
    internal IEnumerable<MethodParameterInfo> Build()
    {
        if (!memberMethod.IsStatic)
        {
            yield return new MethodParameterInfo
            {
                ParameterName = "instance",
                IsInjectedInstanceParameter = true,
                Type = new InteropTypeInfoBuilder(classSymbol).Build() // TODO: TEST THIS
            };
        }

        foreach (IParameterSymbol parameterSymbol in memberMethod.Parameters)
        {
            yield return new MethodParameterInfo
            {
                ParameterName = parameterSymbol.Name,
                IsInjectedInstanceParameter = false,
                Type = new InteropTypeInfoBuilder(parameterSymbol.Type).Build()
            };
        }
    }
}
