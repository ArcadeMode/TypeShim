using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders the 'TSModule' marked by the user. Can be constructed with the 'AssemblyExports', from here on out the user can interact with their C# code
/// </summary>
internal class TypescriptUserModuleClassRenderer(ClassInfo moduleClassInfo, TypeScriptMethodRenderer methodRenderer, TypescriptClassNameBuilder classNameBuilder)
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        RenderModuleClass(moduleClassInfo.Name, classNameBuilder.GetModuleInteropClassName()); 
        return sb.ToString();
    }

    private void RenderModuleClass(string className, string interopInterfaceName)
    {
        string indent = "  ";
        sb.AppendLine($"// Auto-generated TypeScript statics class. Source class: {moduleClassInfo.Namespace}.{moduleClassInfo.Name}");

        sb.AppendLine($"export class {className} {{");
        sb.AppendLine($"{indent}private interop: {interopInterfaceName};");
        sb.AppendLine();
        sb.AppendLine($"{indent}constructor(interop: {interopInterfaceName}) {{");
        sb.AppendLine($"{indent}{indent}this.interop = interop;");
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();
        foreach (MethodInfo methodInfo in moduleClassInfo.Methods.Where(m => m.IsStatic))
        {
            sb.AppendLine($"{indent}public {methodRenderer.RenderMethodSignatureForClass(methodInfo)} {{");
            RenderProxyInstanceExtraction(indent, methodInfo);
            RenderInteropInvocation(indent, methodInfo);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        foreach (PropertyInfo propertyInfo in moduleClassInfo.Properties.Where(p => p.IsStatic))
        {
            MethodInfo? getter = propertyInfo.GetMethod;
            sb.AppendLine($"{indent}public {methodRenderer.RenderPropertyGetterSignatureForClass(getter.WithoutInstanceParameter())} {{");
            RenderInteropInvocation(indent, getter);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();

            if (propertyInfo.SetMethod is MethodInfo setter)
            {
                sb.AppendLine($"{indent}public {methodRenderer.RenderPropertySetterSignatureForClass(setter.WithoutInstanceParameter())} {{");
                RenderProxyInstanceExtraction(indent, setter);
                RenderInteropInvocation(indent, setter);
                sb.AppendLine($"{indent}}}");
                sb.AppendLine();
            }
        }
        sb.AppendLine($"}}");
    }

    private void RenderProxyInstanceExtraction(string indent, MethodInfo methodInfo)
    {
        foreach (MethodParameterInfo param in methodInfo.MethodParameters.Where(p => p.Type.RequiresCLRTypeConversion && !p.IsInjectedInstanceParameter))
        {
            if (classNameBuilder.GetUserClassProxyName(param.Type) is not string proxyClassName)
            {
                throw new ArgumentException("All type conversion-requiring types should be user class proxies.");
            }

            if (param.Type.IsArrayType || param.Type.IsTaskType)
            {
                string transformFunction = param.Type.IsArrayType ? "map" : "then";
                sb.AppendLine($"{indent}{indent}const {GetInteropInvocationVariable(param)} = {param.Name}.{transformFunction}(item => item instanceof {proxyClassName} ? item.instance : item);");
            }
            else
            {
                // simple or nullable types
                sb.AppendLine($"{indent}{indent}const {GetInteropInvocationVariable(param)} = {param.Name} instanceof {proxyClassName} ? {param.Name}.instance : {param.Name};");
            }

        }
    }

    private void RenderInteropInvocation(string indent, MethodInfo methodInfo)
    {
        string interopInvoke = RenderMethodCallParametersWithInstanceParameterExpression(methodInfo, "this.instance"); // note: instance parameter will be unused for static methods
        if (classNameBuilder.GetUserClassProxyName(methodInfo.ReturnType) is string proxyClassName) // user class return type, wrap in proxy
        {
            string optionalAwait = methodInfo.ReturnType.IsTaskType ? "await " : string.Empty;
            sb.AppendLine($"{indent}{indent}const res = {optionalAwait}this.interop.{ResolveInteropMethodAccessor(moduleClassInfo, methodInfo)}({interopInvoke});");

            if (methodInfo.ReturnType.IsArrayType)
            {
                sb.AppendLine($"{indent}{indent}return res.map(item => {GetNewProxyExpression(methodInfo.ReturnType, proxyClassName, "item")});");
            }
            else
            {
                sb.AppendLine($"{indent}{indent}return {GetNewProxyExpression(methodInfo.ReturnType, proxyClassName, "res")};");
            }
        }
        else // primitive return type or void
        {
            string optionalReturn = methodInfo.ReturnType.ManagedType == KnownManagedType.Void ? string.Empty : "return ";
            sb.AppendLine($"{indent}{indent}{optionalReturn}this.interop.{ResolveInteropMethodAccessor(moduleClassInfo, methodInfo)}({interopInvoke});");
        }
    }

    /// <summary>
    /// Renders only the parameter names for method call, with the same names as defined in the method info.
    /// Excludes the injected instance parameter if present.
    /// </summary>
    /// <param name="methodInfo"></param>
    /// <returns></returns>
    internal string RenderMethodCallParametersWithInstanceParameterExpression(MethodInfo methodInfo, string instanceParameterExpression)
    {
        return string.Join(", ", methodInfo.MethodParameters.Select(p => p.IsInjectedInstanceParameter ? instanceParameterExpression : GetInteropInvocationVariable(p)));
    }

    private string GetNewProxyExpression(InteropTypeInfo returnTypeInfo, string proxyClassName, string instanceName)
    {
        if (returnTypeInfo.IsNullableType)
        {
            return $"{instanceName} ? new {proxyClassName}({instanceName}, this.interop) : null";
        }
        else
        {
            return $"new {proxyClassName}({instanceName}, this.interop)";
        }
    }

    private string GetInteropInvocationVariable(MethodParameterInfo param)
    {
        return param.Type.RequiresCLRTypeConversion ? $"{param.Name}Instance" : param.Name;
    }

    private string ResolveInteropMethodAccessor(ClassInfo classInfo, MethodInfo methodInfo)
    {
        return $"{classInfo.Namespace}.{classNameBuilder.GetInteropInterfaceName(classInfo)}.{methodInfo.Name}";
    }
}
