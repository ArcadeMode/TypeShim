using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypescriptUserClassProxyRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypescriptClassNameBuilder classNameBuilder)
{
    // PURPOSE:
    // - glue between interop interface for a single class instance, enabling dynamic method invocation

    private readonly StringBuilder sb = new();

    internal string Render()
    {
        string proxyClassName = classNameBuilder.GetUserClassProxyName(classInfo);
        string interopInterfaceName = classNameBuilder.GetModuleInteropClassName();
        RenderProxyClass(proxyClassName, interopInterfaceName);

        //BEEPBOOP
        //string staticsClassName = classNameBuilder.GetUserClassStaticsName(classInfo);
        //RenderStaticsClass(staticsClassName, interopInterfaceName);
        return sb.ToString();
    }

    private void RenderProxyClass(string className, string interopInterfaceName)
    {
        string indent = "  ";
        sb.AppendLine($"// Auto-generated TypeScript proxy class. Source class: {classInfo.Namespace}.{classInfo.Name}");

        sb.AppendLine($"class {className} implements {classInfo.Name} {{");
        sb.AppendLine($"{indent}interop: {interopInterfaceName};");
        sb.AppendLine($"{indent}instance: object;");
        sb.AppendLine();
        sb.AppendLine($"{indent}constructor(instance: object, interop: {interopInterfaceName}) {{");
        sb.AppendLine($"{indent}{indent}this.interop = interop;");
        sb.AppendLine($"{indent}{indent}this.instance = instance;");
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();

        foreach (MethodInfo methodInfo in classInfo.Methods.Where(m => !m.IsStatic))
        {
            sb.AppendLine($"{indent}public {methodRenderer.RenderMethodSignatureForClass(methodInfo.WithoutInstanceParameter())} {{");
            RenderProxyInstanceExtraction(indent, methodInfo);
            RenderInteropInvocation(indent, methodInfo);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => !p.IsStatic))
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
            sb.AppendLine($"{indent}{indent}const res = {optionalAwait}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({interopInvoke});");

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
            sb.AppendLine($"{indent}{indent}{optionalReturn}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({interopInvoke});");
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
