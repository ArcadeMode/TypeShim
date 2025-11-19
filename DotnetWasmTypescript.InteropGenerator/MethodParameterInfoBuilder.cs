using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

internal class MethodParameterInfoBuilder(INamedTypeSymbol classSymbol, IMethodSymbol memberMethod)
{
    internal IEnumerable<MethodParameterInfo> Build()
    {
        if (!memberMethod.IsStatic)
        {
            yield return new MethodParameterInfo
            {
                ParameterName = "instance",
                KnownType = KnownManagedType.Object,
                InteropTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                CLRTypeSyntax = SyntaxFactory.IdentifierName(classSymbol.Name)
            };
        }

        foreach (IParameterSymbol parameterSymbol in memberMethod.Parameters)
        {
            JSTypeInfo parameterMarshallingTypeInfo = JSTypeInfo.CreateJSTypeInfoForTypeSymbol(parameterSymbol.Type); // type info needed for jsexport to know how to marshal param type

            TypeSyntax? parameterTypeSyntax = parameterMarshallingTypeInfo switch 
            {
                JSSimpleTypeInfo { Syntax: TypeSyntax typeSyntax } => typeSyntax,
                JSArrayTypeInfo arrayTypeInfo => SyntaxFactory.ArrayType(arrayTypeInfo.ElementTypeInfo.Syntax),
                _ => throw new InvalidOperationException($"Unsupported type info found in parameter type {parameterSymbol.Type} of method {memberMethod} of {classSymbol}")
            };

            yield return new MethodParameterInfo
            {
                ParameterName = parameterSymbol.Name,
                KnownType = parameterMarshallingTypeInfo.KnownType,
                InteropTypeSyntax = parameterTypeSyntax,
                CLRTypeSyntax = SyntaxFactory.ParseTypeName(parameterSymbol.Type.Name)
            };
        }
    }
}
