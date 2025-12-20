using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptMethodRenderer(ClassInfo classInfo, TypescriptSymbolNameProvider symbolNameProvider)
{
    private readonly StringBuilder sb = new();

    internal string GetRenderedContent()
    {
        return sb.ToString();
    }

    internal void RenderProxyMethod(int depth, MethodInfo methodInfo)
    {
        string indent = new(' ', depth * 2);
        sb.AppendLine($"{indent}public {RenderProxyMethodSignature(methodInfo.WithoutInstanceParameter())} {{");
        RenderMethodBodyContent(depth + 1, methodInfo);
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();

        string RenderProxyMethodSignature(MethodInfo methodInfo)
        {
            string returnType = symbolNameProvider.GetProxyReferenceNameIfExists(methodInfo.ReturnType) ?? symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);

            string optionalAsync = methodInfo.ReturnType.IsTaskType ? "async " : string.Empty;
            return $"{optionalAsync}{methodInfo.Name}({RenderProxyMethodParameters(methodInfo)}): {returnType}";
        }
    }

    internal void RenderProxyProperty(int depth, PropertyInfo propertyInfo)
    {
        string indent = new(' ', depth * 2);
        MethodInfo getterInfo = propertyInfo.GetMethod;
        sb.AppendLine($"{indent}public {RenderProxyPropertyGetterSignature(getterInfo.WithoutInstanceParameter())} {{");
        RenderMethodBodyContent(depth + 1, getterInfo);
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();

        if (propertyInfo.SetMethod is MethodInfo setterInfo)
        {
            sb.AppendLine($"{indent}public {RenderProxyPropertySetterSignature(setterInfo.WithoutInstanceParameter())} {{");
            RenderMethodBodyContent(depth + 1, setterInfo);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }

        string RenderProxyPropertyGetterSignature(MethodInfo methodInfo)
        {
            string returnType = symbolNameProvider.GetProxyReferenceNameIfExists(methodInfo.ReturnType) ?? symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);
            return $"get {propertyInfo.Name}({RenderProxyMethodParameters(methodInfo)}): {returnType}";
        }

        string RenderProxyPropertySetterSignature(MethodInfo methodInfo)
        {
            return $"set {propertyInfo.Name}({RenderProxyMethodParameters(methodInfo)})";
        }
    }

    private string RenderProxyMethodParameters(MethodInfo methodInfo)
    {
        return string.Join(", ", methodInfo.MethodParameters.Select(p =>
        {
            string returnType = symbolNameProvider.GetProxySnapshotUnionIfExists(p.Type) ?? symbolNameProvider.GetNakedSymbolReference(p.Type);
            return $"{p.Name}: {returnType}";
        }));
    }

    internal void RenderMethodBodyContent(int depth, MethodInfo methodInfo)
    {
        string indent = new(' ', depth * 2);
        if (!methodInfo.IsSnapshotOverloaded())
        {
            RenderManagedObjectConstAssignments(depth, methodInfo, null);
            RenderInteropInvocation(depth, methodInfo, null);
            return;
        }

        sb.Append($"{indent}if (");
        RenderOverloadTypeCheckBooleanExpression(methodInfo, overloadInfo: null);
        sb.AppendLine($") {{");

        RenderManagedObjectConstAssignments(depth + 1, methodInfo, null);
        RenderInteropInvocation(depth + 1, methodInfo, null);

        sb.Append($"{indent}}}");
        foreach (MethodOverloadInfo overloadInfo in methodInfo.Overloads)
        {
            sb.Append($" else if (");
            RenderOverloadTypeCheckBooleanExpression(methodInfo, overloadInfo);
            sb.AppendLine($") {{");
            RenderManagedObjectConstAssignments(depth + 1, methodInfo, overloadInfo);
            RenderInteropInvocation(depth + 1, methodInfo, overloadInfo);
            sb.Append($"{indent}}}");
        }
        sb.AppendLine($" else {{");
        sb.AppendLine($"{indent}  throw new Error(\"No overload for interop method '{methodInfo.Name}' matches the provided arguments.\");");
        sb.AppendLine($"{indent}}}");

        void RenderOverloadTypeCheckBooleanExpression(MethodInfo methodInfo, MethodOverloadInfo? overloadInfo = null)
        {
            int i = 0;
            IEnumerable<MethodParameterInfo?> overloadParams = overloadInfo?.MethodParameters ?? Enumerable.Repeat<MethodParameterInfo?>(null, methodInfo.MethodParameters.Count);
            foreach ((MethodParameterInfo param, MethodParameterInfo? overloadParam) in methodInfo.MethodParameters.Zip(overloadParams))
            {
                if (param.IsInjectedInstanceParameter || !param.Type.RequiresCLRTypeConversion)
                    continue;

                InteropTypeInfo paramTargetType = param.Type.TypeArgument ?? param.Type; // we are concerned with the element type for arrays/tasks or inner type of nullables
                string targetClassName = overloadParam != null && overloadParam.Type.ContainsTypeOf(KnownManagedType.JSObject)
                    ? symbolNameProvider.GetSnapshotReferenceNameIfExists(paramTargetType) ?? throw new ArgumentException("All type conversion-requiring types should be user classes.")
                    : symbolNameProvider.GetProxyReferenceNameIfExists(paramTargetType) ?? throw new ArgumentException("All type conversion-requiring types should be user classes.");

                sb.Append(i > 0 ? " && " : string.Empty);
                if (param.Type.IsArrayType)
                {
                    sb.Append($"{param.Name}.every(e => e instanceof {targetClassName})");
                }
                else if (param.Type.IsTaskType)
                {
                    // option A: make TS method async, await + instanceof check on resolved value
                    // option B: fire and forget, then() + instanceof check inside (wont be able to deal with non-void return types)
                    // for now, we will throw not implemented, think about it..

                    throw new NotImplementedException("TODO: make TS method async to await task input and call appropriate interop when applicable?");
                    //sb.Append($"{param.Name} instanceof Promise");
                }
                else // simple or nullable types
                {
                    sb.Append($"{param.Name} instanceof {targetClassName}");
                }
                i++;
            }
        }

        void RenderManagedObjectConstAssignments(int depth, MethodInfo methodInfo, MethodOverloadInfo? overloadInfo)
        {
            string indent = new(' ', depth * 2);
            IEnumerable<MethodParameterInfo?> overloadParameters = overloadInfo?.MethodParameters ?? Enumerable.Repeat<MethodParameterInfo?>(null, methodInfo.MethodParameters.Count);
            foreach ((MethodParameterInfo originalParam, MethodParameterInfo? overloadParam) in methodInfo.MethodParameters.Zip(overloadParameters))
            {
                if (originalParam.IsInjectedInstanceParameter || !originalParam.Type.RequiresCLRTypeConversion)
                    continue;

                if (overloadParam != null && overloadParam.Type.ContainsTypeOf(KnownManagedType.JSObject))
                    continue; // if the original was also JSObject, it wouldnt RequiresCLRTypeConversion

                InteropTypeInfo paramTargetType = originalParam.Type.TypeArgument ?? originalParam.Type; // we are concerned with the element type for arrays/tasks or inner type of nullables
                if (symbolNameProvider.GetProxyReferenceNameIfExists(paramTargetType) is not string proxyClassName)
                {
                    throw new ArgumentException("All type conversion-requiring types should be user class proxies.");
                }

                InteropTypeInfo paramType = originalParam.Type;
                if (paramType.IsArrayType || paramType.IsTaskType)
                {
                    string instancePropertyAccessor = paramTargetType.IsNullableType ? "?.instance" : ".instance";
                    string transformFunction = paramType.IsArrayType ? "map" : "then";
                    sb.AppendLine($"{indent}const {GetInteropInvocationVariable(originalParam, overloadParam)} = {originalParam.Name}.{transformFunction}(e => e{instancePropertyAccessor});");
                }
                else // simple or nullable proxy types
                {
                    string instancePropertyAccessor = paramTargetType.IsNullableType ? "?.instance" : ".instance";
                    sb.AppendLine($"{indent}const {GetInteropInvocationVariable(originalParam, overloadParam)} = {originalParam.Name}{instancePropertyAccessor};");
                }
            }
        }

        void RenderInteropInvocation(int depth, MethodInfo methodInfo, MethodOverloadInfo? overloadInfo = null)
        {
            string indent = new(' ', depth * 2);
            string interopInvoke = RenderMethodCallParametersWithInstanceParameterExpression(methodInfo, overloadInfo, "this.instance"); // note: instance parameter will be unused for static methods

            InteropTypeInfo returnType = methodInfo.ReturnType;
            InteropTypeInfo returnTargetType = methodInfo.ReturnType.TypeArgument ?? methodInfo.ReturnType; // we are concerned with the element type for arrays/tasks or inner type of nullables

            if (symbolNameProvider.GetProxyReferenceNameIfExists(returnTargetType) is string proxyClassName)
            {
                // user class return type, wrap in proxy
                string optionalAwait = returnType.IsTaskType ? "await " : string.Empty;
                sb.AppendLine($"{indent}const res = {optionalAwait}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo, overloadInfo)}({interopInvoke});");

                if (returnType.IsArrayType)
                {
                    sb.AppendLine($"{indent}return res.map(e => {GetNewProxyExpression(returnType, proxyClassName, "e")});");
                }
                else
                {
                    sb.AppendLine($"{indent}return {GetNewProxyExpression(returnType, proxyClassName, "res")};");
                }
            }
            else // primitive return type or void
            {
                string optionalReturn = returnType.ManagedType == KnownManagedType.Void ? string.Empty : "return ";
                sb.AppendLine($"{indent}{optionalReturn}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo, overloadInfo)}({interopInvoke});");
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
        string RenderMethodCallParametersWithInstanceParameterExpression(MethodInfo methodInfo, MethodOverloadInfo? overloadInfo, string instanceParameterExpression)
        {
            IEnumerable<MethodParameterInfo?> overloadParameters = overloadInfo?.MethodParameters ?? Enumerable.Repeat<MethodParameterInfo?>(null, methodInfo.MethodParameters.Count);
            IEnumerable<(MethodParameterInfo Param, MethodParameterInfo? Overload)> methodInfoWithOverload = methodInfo.MethodParameters.Zip(overloadParameters);
            return string.Join(", ", methodInfoWithOverload.Select((p) => p.Param.IsInjectedInstanceParameter ? instanceParameterExpression : GetInteropInvocationVariable(p.Param, p.Overload)));
        }

        string GetInteropInvocationVariable(MethodParameterInfo param, MethodParameterInfo? overload)
        {
            return param.Type.RequiresCLRTypeConversion && (overload == null || !overload.Type.ContainsTypeOf(KnownManagedType.JSObject))
                ? $"{param.Name}Instance"
                : param.Name;
        }

        string ResolveInteropMethodAccessor(ClassInfo classInfo, MethodInfo methodInfo, MethodOverloadInfo? overloadInfo)
        {
            return $"{classInfo.Namespace}.{symbolNameProvider.GetInteropInterfaceName(classInfo)}.{overloadInfo?.Name ?? methodInfo.Name}";
        }
    }
}