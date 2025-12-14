using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypescriptUserClassProxyRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();

    internal string Render(int depth)
    {
        string interopInterfaceName = symbolNameProvider.GetModuleInteropClassName();
        RenderProxyClass(interopInterfaceName, depth);
        return sb.ToString();
    }

    private void RenderProxyClass(string interopInterfaceName, int depth)
    {
        string indent = new(' ', depth * 2);
        string indent2 = new(' ', (depth + 1) * 2);
        string indent3 = new(' ', (depth + 2) * 2);

        sb.AppendLine($"{indent}export class {symbolNameProvider.GetProxyDefinitionName()} {{");
        sb.AppendLine($"{indent2}interop: {interopInterfaceName};");
        sb.AppendLine($"{indent2}instance: object;");
        sb.AppendLine();
        sb.AppendLine($"{indent2}constructor(instance: object, interop: {interopInterfaceName}) {{");
        sb.AppendLine($"{indent3}this.interop = interop;");
        sb.AppendLine($"{indent3}this.instance = instance;");
        sb.AppendLine($"{indent2}}}");
        sb.AppendLine();

        foreach (MethodInfo methodInfo in classInfo.Methods.Where(m => !m.IsStatic))
        {
            sb.AppendLine($"{indent2}public {methodRenderer.RenderProxyMethodSignature(methodInfo.WithoutInstanceParameter())} {{");
            RenderProxyInstanceExtraction(indent3, methodInfo);
            RenderInteropInvocation(indent3, methodInfo);
            sb.AppendLine($"{indent2}}}");
            sb.AppendLine();
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => !p.IsStatic))
        {
            MethodInfo? getter = propertyInfo.GetMethod;
            sb.AppendLine($"{indent2}public {methodRenderer.RenderProxyPropertyGetterSignature(getter.WithoutInstanceParameter())} {{");
            RenderInteropInvocation(indent3, getter);
            sb.AppendLine($"{indent2}}}");
            sb.AppendLine();

            if (propertyInfo.SetMethod is MethodInfo setter)
            {
                sb.AppendLine($"{indent2}public {methodRenderer.RenderProxyPropertySetterSignature(setter.WithoutInstanceParameter())} {{");
                RenderProxyInstanceExtraction(indent3, setter);
                RenderInteropInvocation(indent3, setter);
                sb.AppendLine($"{indent2}}}");
                sb.AppendLine();
            }
        }
        sb.AppendLine($"{indent}}}");
    }

    private void RenderProxyInstanceExtraction(string indent, MethodInfo methodInfo)
    {
        foreach (MethodParameterInfo param in methodInfo.MethodParameters.Where(p => p.Type.RequiresCLRTypeConversion && !p.IsInjectedInstanceParameter))
        {
            InteropTypeInfo paramType = param.Type;
            InteropTypeInfo paramTargetType = param.Type.TypeArgument ?? param.Type; // we are concerned with the element type for arrays/tasks or inner type of nullables
            if (symbolNameProvider.GetProxyReferenceNameIfExists(paramTargetType) is not string proxyClassName)
            {
                throw new ArgumentException("All type conversion-requiring types should be user class proxies.");
            }

            if (paramType.IsArrayType || paramType.IsTaskType)
            {
                string transformFunction = paramType.IsArrayType ? "map" : "then";
                sb.AppendLine($"{indent}const {GetInteropInvocationVariable(param)} = {param.Name}.{transformFunction}(item => item instanceof {proxyClassName} ? item.instance : item);");
            }
            else
            {
                // simple or nullable types
                sb.AppendLine($"{indent}const {GetInteropInvocationVariable(param)} = {param.Name} instanceof {proxyClassName} ? {param.Name}.instance : {param.Name};");
            }
        }
    }

    private void RenderInteropInvocation(string indent, MethodInfo methodInfo)
    {
        string interopInvoke = RenderMethodCallParametersWithInstanceParameterExpression(methodInfo, "this.instance"); // note: instance parameter will be unused for static methods

        InteropTypeInfo returnType = methodInfo.ReturnType;
        InteropTypeInfo returnTargetType = methodInfo.ReturnType.TypeArgument ?? methodInfo.ReturnType; // we are concerned with the element type for arrays/tasks or inner type of nullables

        if (symbolNameProvider.GetProxyReferenceNameIfExists(returnTargetType) is string proxyClassName) 
        {
            // user class return type, wrap in proxy
            string optionalAwait = returnType.IsTaskType ? "await " : string.Empty;
            sb.AppendLine($"{indent}const res = {optionalAwait}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({interopInvoke});");

            if (returnType.IsArrayType)
            {
                sb.AppendLine($"{indent}return res.map(item => {GetNewProxyExpression(returnType, proxyClassName, "item")});");
            }
            else
            {
                sb.AppendLine($"{indent}return {GetNewProxyExpression(returnType, proxyClassName, "res")};");
            }
        }
        else // primitive return type or void
        {
            string optionalReturn = returnType.ManagedType == KnownManagedType.Void ? string.Empty : "return ";
            sb.AppendLine($"{indent}{optionalReturn}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({interopInvoke});");
        }

        static string GetNewProxyExpression(InteropTypeInfo returnTypeInfo, string proxyClassName, string instanceName)
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

    private string GetInteropInvocationVariable(MethodParameterInfo param)
    {
        return param.Type.RequiresCLRTypeConversion ? $"{param.Name}Instance" : param.Name;
    }

    private string ResolveInteropMethodAccessor(ClassInfo classInfo, MethodInfo methodInfo)
    {
        return $"{classInfo.Namespace}.{symbolNameProvider.GetInteropInterfaceName(classInfo)}.{methodInfo.Name}";
    }
}
