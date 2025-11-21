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
        // type info needed for jsexport to know how to marshal return type
        if (JSTypeInfo.CreateJSTypeInfoForTypeSymbol(memberMethod.ReturnType) is not JSSimpleTypeInfo { Syntax: TypeSyntax returnTypeSyntax, KnownType: KnownManagedType knownReturnType })
        {
            throw new InvalidOperationException($"Unsupported type info found in return type {memberMethod.ReturnType} of method {memberMethod} of {classSymbol}");
        }

        MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod);

        return new MethodInfo
        {
            IsStatic = memberMethod.IsStatic,
            Name = memberMethod.Name,
            MethodParameters = parameterInfoBuilder.Build(),
            ReturnKnownType = knownReturnType,
            ReturnInteropTypeSyntax = returnTypeSyntax,
            ReturnCLRTypeSyntax = SyntaxFactory.ParseTypeName(memberMethod.ReturnType.Name)
        };
    }
}

