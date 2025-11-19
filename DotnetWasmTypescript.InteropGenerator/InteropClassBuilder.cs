using Microsoft.CodeAnalysis;
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
