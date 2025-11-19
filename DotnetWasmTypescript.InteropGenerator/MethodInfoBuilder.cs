using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

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

    internal string Render(MethodInfo methodInfo)
    {
        StringBuilder sb = new();
        sb.AppendLine("    [JSExport]");
        if (methodInfo.ReturnKnownType == KnownManagedType.Object)
        {
            sb.AppendLine("    [return: JSMarshalAs<JSType.Any>]");
        }

        List<MethodParameterInfo> methodParameters = methodInfo.MethodParameters.ToList();

        sb.AppendLine($"    public static {methodInfo.ReturnInteropTypeSyntax} {memberMethod.Name}({parameterInfoBuilder.Render(methodParameters)})");
        sb.AppendLine("    {");

        // Cast object-typed parameters to their original types
        foreach (MethodParameterInfo paramInfo in methodParameters)
        {
            if (paramInfo.KnownType is KnownManagedType.Object or KnownManagedType.Array)
            {
                sb.AppendLine($"        {paramInfo.CLRTypeSyntax} {paramInfo.GetTypedParameterName()} = ({paramInfo.CLRTypeSyntax}){paramInfo.ParameterName};");
            }
        }

        string methodInvocation;
        if (!methodInfo.IsStatic)
        {
            MethodParameterInfo instanceParam = methodParameters[0];
            List<MethodParameterInfo> memberParams = methodParameters[1..];
            methodInvocation = $"{instanceParam.GetTypedParameterName()}.{memberMethod.Name}({string.Join(", ", memberParams.Select(p => p.GetTypedParameterName()))})";
        }
        else
        {
            List<MethodParameterInfo> memberParams = methodParameters; // for static methods, all parameters are member parameters
            methodInvocation = $"{classSymbol.Name}.{memberMethod.Name}({string.Join(", ", memberParams.Select(p => p.GetTypedParameterName()))})";
        } 

        if (methodInfo.ReturnKnownType != KnownManagedType.Void)
        {
            sb.AppendLine($"        return {methodInvocation};");
        }
        else
        {
            sb.AppendLine($"        {methodInvocation};");
        }
        sb.AppendLine("    }");
        return sb.ToString();
    }
}