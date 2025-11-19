using DotnetWasmTypescript.InteropGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
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
            sb.AppendLine("using TypeScriptExport;");


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

            foreach (IMethodSymbol staticMethod in classSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary && m.IsStatic))
            {
                sb.AppendLine("    [JSExport]");

                if (IsCustomClass(staticMethod.ReturnType))
                {
                    sb.AppendLine("    [return: JSMarshalAs<JSType.Any>]");
                }

                string parameters = string.Join(", ", staticMethod.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
                sb.AppendLine($"    public static {staticMethod.ReturnType.ToDisplayString()} {staticMethod.Name}({parameters}) => {classSymbol.Name}.{staticMethod.Name}({string.Join(", ", staticMethod.Parameters.Select(p => p.Name))});");
            }

            foreach (IMethodSymbol memberMethod in classSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary && !m.IsStatic)) // todo dynamic members exported through interop type as static with instance parameter
            {
                MethodParameterInfoBuilder parameterInfoBuilder = new(classSymbol, memberMethod);
                var parameterInfos = parameterInfoBuilder.Build();
                string parameters = parameterInfoBuilder.Render(parameterInfos);

                //StringBuilder methodParameterCodeBuilder = new();
                //foreach (IParameterSymbol parameterSymbol in memberMethod.Parameters)
                //{
                //    JSTypeInfo parameterMarshallingTypeInfo = JSTypeInfo.CreateJSTypeInfoForTypeSymbol(parameterSymbol.Type); // type info needed for jsexport to know how to marshal param type

                //    if (parameterMarshallingTypeInfo is not JSSimpleTypeInfo { Syntax: TypeSyntax parameterTypeSyntax })
                //    {
                //        throw new InvalidOperationException($"Unsupported type info found in parameter type {parameterSymbol.Type} of method {memberMethod} of {classSymbol}");
                //    }
                //    if (parameterMarshallingTypeInfo.KnownType == KnownManagedType.Object)
                //    {
                //        methodParameterCodeBuilder.Append("[JSMarshalAs<JSType.Any>] ");
                //    }
                //    methodParameterCodeBuilder.Append($"{parameterTypeSyntax} {parameterSymbol.Name},");
                //}
                //string parameters = methodParameterCodeBuilder.ToString().TrimEnd(',');
                //string instanceParameter = $"[JSMarshalAs<JSType.Any>, TsExportAs<{classSymbol.Name}>] object instance";

                //if (!string.IsNullOrEmpty(parameters))
                //{
                //    parameters = $"{instanceParameter}, {parameters}";
                //}
                //else
                //{
                //    parameters = instanceParameter;
                //}


                sb.AppendLine("    [JSExport]");
                // type info needed for jsexport to know how to marshal return type
                if (JSTypeInfo.CreateJSTypeInfoForTypeSymbol(memberMethod.ReturnType) is not JSSimpleTypeInfo { Syntax: TypeSyntax returnTypeSyntax, KnownType: KnownManagedType knownReturnType })
                {
                    throw new InvalidOperationException($"Unsupported type info found in return type {memberMethod.ReturnType} of method {memberMethod} of {classSymbol}");
                }

                if (knownReturnType == KnownManagedType.Object)
                {
                    sb.AppendLine("    [return: JSMarshalAs<JSType.Any>]");
                }

                sb.AppendLine($"    public static {returnTypeSyntax} {memberMethod.Name}({parameters})");
                sb.AppendLine("    {");
                sb.AppendLine($"        {classSymbol.Name} typedInstance = ({classSymbol.Name})instance;");

                string memberInvocation = $"typedInstance.{memberMethod.Name}({string.Join(", ", memberMethod.Parameters.Select(p => p.Name))})";
                if (knownReturnType == KnownManagedType.Void)
                {
                    sb.AppendLine($"        return {memberInvocation};");
                    sb.AppendLine("    }");
                    continue;
                }
                else
                {
                    sb.AppendLine($"        {memberInvocation};");
                    sb.AppendLine("    }");
                }


            }

            sb.AppendLine("}");




            // property diagnostics??
            foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility == Accessibility.Public)
                {
                    
                }
            }


            //sb.AppendLine("}");
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Filter out classes that are not user-defined classes.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsCustomClass(ITypeSymbol type)
        {
            // Exclude primitives
            if (type.SpecialType != SpecialType.None)
                return false;

            // Exclude strings (not considered primitive in Roslyn)
            if (type is INamedTypeSymbol nts && nts.ToDisplayString() == "string")
                return false;

            // Exclude enums, delegates, interfaces, structs, arrays
            if (type.TypeKind != TypeKind.Class)
                return false;

            // Exclude common framework types by namespace (System.*, Microsoft.*)
            var ns = type.ContainingNamespace?.ToDisplayString();
            if (ns != null &&
                (ns.StartsWith("System.") || ns == "System" || ns.StartsWith("Microsoft.")))
                return false;

            // Optionally, exclude specific types (Task, Span, ReadOnlySpan)
            var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (fullName.StartsWith("global::System.Threading.Tasks.Task") ||
                fullName.StartsWith("global::System.Span") ||
                fullName.StartsWith("global::System.ReadOnlySpan"))
                return false;

            // Optionally, restrict to only types defined in the user's assembly
            // if (type.ContainingAssembly?.Name == "YourProjectAssemblyName")
            //     return true;

            // If it passed all filters, treat as custom
            return true;
        }
    }
}


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
                CSSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))
            };
        }

        foreach (IParameterSymbol parameterSymbol in memberMethod.Parameters)
        {
            JSTypeInfo parameterMarshallingTypeInfo = JSTypeInfo.CreateJSTypeInfoForTypeSymbol(parameterSymbol.Type); // type info needed for jsexport to know how to marshal param type

            if (parameterMarshallingTypeInfo is not JSSimpleTypeInfo { Syntax: TypeSyntax parameterTypeSyntax })
            {
                throw new InvalidOperationException($"Unsupported type info found in parameter type {parameterSymbol.Type} of method {memberMethod} of {classSymbol}");
            }

            yield return new MethodParameterInfo
            {
                ParameterName = parameterSymbol.Name,
                KnownType = parameterMarshallingTypeInfo.KnownType,
                CSSyntax = parameterTypeSyntax
            };
        }
    }

    internal string Render(IEnumerable<MethodParameterInfo> parameterInfos)
    {
        StringBuilder codeBuilder = new();
        foreach (MethodParameterInfo parameterInfo in parameterInfos)
        {
            if (parameterInfo.KnownType == KnownManagedType.Object)
            {
                codeBuilder.Append("[JSMarshalAs<JSType.Any>] ");
            }
            codeBuilder.Append($"{parameterInfo.CSSyntax} {parameterInfo.ParameterName},");
        }
        return codeBuilder.ToString().TrimEnd(',');
    }
}

internal class MethodParameterInfo
{ 
    internal string ParameterName { get; init; }
    internal KnownManagedType KnownType { get; init; }
    internal TypeSyntax CSSyntax { get; init; }
    internal Type TSExportType { get; init; }

}
