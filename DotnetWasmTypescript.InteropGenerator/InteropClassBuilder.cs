using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DotnetWasmTypescript.InteropGenerator
{
    internal class InteropClassBuilder
    {
        private StringBuilder sb = new();

        internal SourceText? Build(INamedTypeSymbol classSymbol)
        {
            sb.AppendLine("// Auto-generated TypeScript interop definitions");
            sb.AppendLine("using System.Runtime.InteropServices.JavaScript;");
            //sb.AppendLine("using TypeScriptExport;");


            if (classSymbol.ContainingNamespace?.ToDisplayString() is not string nsName)
            {
                
                return null;
            }
            sb.AppendLine($@"namespace {nsName};");

            sb.AppendLine($"public partial class {classSymbol.Name}Interop");
            sb.AppendLine("{");

            // TODO:
            // - PARAMETER TYPE MARSHALING FOR CUSTOMCLASSES
            // - 

            foreach (IMethodSymbol staticMethod in classSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary))
            {
                MethodInfoBuilder methodInfoBuilder = new(classSymbol, staticMethod);
                MethodInfo methodInfo = methodInfoBuilder.Build();
                sb.Append(methodInfoBuilder.Render(methodInfo));
            }

            //foreach (IMethodSymbol memberMethod in classSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary && !m.IsStatic)) // todo dynamic members exported through interop type as static with instance parameter
            //{
            //    sb.AppendLine("    [JSExport]");
                
            //    // type info needed for jsexport to know how to marshal return type
            //    if (JSTypeInfo.CreateJSTypeInfoForTypeSymbol(memberMethod.ReturnType) is not JSSimpleTypeInfo { Syntax: TypeSyntax returnTypeSyntax, KnownType: KnownManagedType knownReturnType })
            //    {
            //        throw new InvalidOperationException($"Unsupported type info found in return type {memberMethod.ReturnType} of method {memberMethod} of {classSymbol}");
            //    }

            //    if (knownReturnType == KnownManagedType.Object)
            //    {
            //        sb.AppendLine("    [return: JSMarshalAs<JSType.Any>]");
            //    }

            //    MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod);
            //    List<MethodParameterInfo> parameterInfos = [.. parameterInfoBuilder.Build()];

            //    if (parameterInfos is not [MethodParameterInfo instanceParam, ..IEnumerable<MethodParameterInfo> memberParams])
            //    {
            //        throw new InvalidOperationException("Expected at least instance parameter and zero or more member parameters.");
            //    }
                
            //    sb.AppendLine($"    public static {returnTypeSyntax} {memberMethod.Name}({parameterInfoBuilder.Render(parameterInfos)})");
            //    sb.AppendLine("    {");

            //    // Cast object-typed parameters to their original types
            //    foreach (MethodParameterInfo paramInfo in parameterInfos)
            //    {
            //        if (paramInfo.KnownType == KnownManagedType.Object)
            //        {
            //            sb.AppendLine($"        {paramInfo.CLRTypeSyntax} {paramInfo.GetTypedParameterName()} = ({paramInfo.CLRTypeSyntax}){paramInfo.ParameterName};");
            //        }
            //    }
                
            //    string memberInvocation = $"{instanceParam.GetTypedParameterName()}.{memberMethod.Name}({string.Join(", ", memberParams.Select(p => p.GetTypedParameterName()))})";
            //    if (knownReturnType != KnownManagedType.Void)
            //    {
            //        sb.AppendLine($"        return {memberInvocation};");
            //        sb.AppendLine("    }");
            //        continue;
            //    }
            //    else
            //    {
            //        sb.AppendLine($"        {memberInvocation};");
            //        sb.AppendLine("    }");
            //    }
            //}





            // FEATURE: generate property accessors (mostly needs correct receivers on TS end)
            foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
            {

            }

            sb.AppendLine("}");

            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }
    }
}

internal sealed class MethodInfo
{
    internal required bool IsStatic { get; init; }
    internal required string Name { get; init; }
    internal required IEnumerable<MethodParameterInfo> MethodParameters { get; init; }
    internal required KnownManagedType ReturnKnownType { get; init; }
    internal required TypeSyntax ReturnInteropTypeSyntax { get; init; }
    internal required TypeSyntax ReturnCLRTypeSyntax { get; init; }
}

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