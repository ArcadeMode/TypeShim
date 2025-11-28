using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using TypeShim.Generator.Parsing;

internal sealed class MethodInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod)
{
    private MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod);

    internal MethodInfo Build()
    {
        
        MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod);

        return new MethodInfo
        {
            IsStatic = memberMethod.IsStatic,
            Name = memberMethod.Name,
            MethodParameters = parameterInfoBuilder.Build(),
            ReturnType = InteropTypeInfo.FromTypeSymbol(memberMethod.ReturnType) ?? throw new InvalidOperationException($"Could not create InteropTypeInfo for return type {memberMethod.ReturnType} of method {memberMethod} of {classSymbol}")
        };
    }
}

