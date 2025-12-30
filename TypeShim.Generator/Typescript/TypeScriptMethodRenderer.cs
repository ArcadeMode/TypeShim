using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptMethodRenderer(TypescriptSymbolNameProvider symbolNameProvider, RenderContext ctx)
{
    internal void RenderProxyConstructor(ConstructorInfo? constructorInfo)
    {
        if (constructorInfo == null)
        {
            ctx.AppendLine("private constructor() {}");
        }
        else
        {
            IEnumerable<MethodParameterInfo> parameterInfos = constructorInfo.GetParametersIncludingInitializerObject();
            RenderConstructorSignature(parameterInfos);
            RenderConstructorBody(parameterInfos);
        }

        void RenderConstructorSignature(IEnumerable<MethodParameterInfo> parameterInfos)
        {
            ctx.Append("constructor(");
            RenderParameterList(parameterInfos);
            ctx.Append(")");
        }

        void RenderConstructorBody(IEnumerable<MethodParameterInfo> parameterInfos)
        {
            ctx.AppendLine(" {");
            using (ctx.Indent())
            {
                RenderHandleExtractionToConsts(parameterInfos);
                if (symbolNameProvider.GetUserClassSymbolNameIfExists(constructorInfo.Type, SymbolNameFlags.Proxy | SymbolNameFlags.Isolated) is string proxyClassName)
                {
                    ctx.Append("super(");
                    RenderInteropInvocation(constructorInfo.Name, constructorInfo.Parameters);
                    ctx.AppendLine(");");
                }
                else
                {
                    throw new InvalidOperationException("Constructor must have a user class return type.");
                }
            }
            ctx.AppendLine("}");
        }
    }

    internal void RenderProxyMethod(MethodInfo methodInfo)
    {
        RenderProxyMethodSignature(methodInfo.WithoutInstanceParameter());
        RenderMethodBody(methodInfo);

        void RenderProxyMethodSignature(MethodInfo methodInfo)
        {
            ctx.Append("public ");
            if (methodInfo.IsStatic) ctx.Append("static ");
            if (methodInfo.ReturnType.IsTaskType) ctx.Append("async ");
            ctx.Append(methodInfo.Name)
               .Append('(');
            RenderParameterList(methodInfo.Parameters);
            ctx.Append("): ");
            string returnType = symbolNameProvider.GetUserClassSymbolNameIfExists(methodInfo.ReturnType, SymbolNameFlags.Proxy) ?? symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);
            ctx.Append(returnType);
        }
    }

    internal void RenderProxyProperty(PropertyInfo propertyInfo)
    {
        MethodInfo getterInfo = propertyInfo.GetMethod;
        RenderProxyPropertyGetterSignature(getterInfo.WithoutInstanceParameter());
        RenderMethodBody(getterInfo);

        if (propertyInfo.SetMethod is MethodInfo setterInfo)
        {
            ctx.AppendLine();
            RenderProxyPropertySetterSignature(setterInfo.WithoutInstanceParameter());
            RenderMethodBody(setterInfo);
        }

        void RenderProxyPropertyGetterSignature(MethodInfo methodInfo)
        {
            ctx.Append($"public ");
            if (methodInfo.IsStatic) ctx.Append("static ");
            string returnType = symbolNameProvider.GetUserClassSymbolNameIfExists(methodInfo.ReturnType, SymbolNameFlags.Proxy) ?? symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType);
            ctx.Append("get ").Append(propertyInfo.Name).Append("(): ").Append(returnType);
        }

        void RenderProxyPropertySetterSignature(MethodInfo methodInfo)
        {
            ctx.Append($"public ");
            if (methodInfo.IsStatic) ctx.Append("static ");
            ctx.Append("set ").Append(propertyInfo.Name).Append('(');
            RenderParameterList(methodInfo.Parameters);
            ctx.Append(')');
        }
    }

    private void RenderParameterList(IEnumerable<MethodParameterInfo> parameterInfos)
    {
        bool isFirst = true;
        foreach (MethodParameterInfo parameterInfo in parameterInfos)
        {
            SymbolNameFlags flags = ContainsSnapshotCompatibleType(parameterInfo.Type) ? SymbolNameFlags.ProxySnapshotUnion : SymbolNameFlags.Proxy;
            string returnType = symbolNameProvider.GetUserClassSymbolNameIfExists(parameterInfo.Type, flags) ?? symbolNameProvider.GetNakedSymbolReference(parameterInfo.Type);
            
            if (!isFirst) ctx.Append(", ");

            ctx.Append(parameterInfo.Name).Append(": ").Append(returnType);
            isFirst = false;
        }

        static bool ContainsSnapshotCompatibleType(InteropTypeInfo typeInfo) // TODO: rename snapshot to 'initializer constructable' or similar
        {
            if (typeInfo.IsSnapshotCompatible)
                return true;
            if (typeInfo.TypeArgument is InteropTypeInfo innerTypeInfo)
                return ContainsSnapshotCompatibleType(innerTypeInfo);
            return false;
        }
    }

    private void RenderMethodBody(MethodInfo methodInfo)
    {
        ctx.AppendLine(" {");
        using (ctx.Indent())
        {
            RenderHandleExtractionToConsts(methodInfo.Parameters);
            if (symbolNameProvider.GetUserClassSymbolNameIfExists(methodInfo.ReturnType, SymbolNameFlags.Proxy | SymbolNameFlags.Isolated) is string proxyClassName)
            {
                // user class return type, wrap in proxy
                ctx.Append("const res = ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters);
                ctx.AppendLine(";");
                ctx.Append($"return ");
                RenderInlineProxyConstruction(methodInfo.ReturnType, proxyClassName, "res");
                ctx.AppendLine(";");
            }
            else // primitive return type or void
            {
                ctx.Append(methodInfo.ReturnType.ManagedType == KnownManagedType.Void ? string.Empty : "return ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters);
                ctx.AppendLine(";");
            }
        }
        ctx.AppendLine("}");

        void RenderInlineProxyConstruction(InteropTypeInfo typeInfo, string proxyClassName, string sourceVarName)
        {
            if (typeInfo is { IsNullableType: true })
            {
                ctx.Append(sourceVarName).Append(" ? ");
                RenderInlineProxyConstruction(typeInfo.TypeArgument!, proxyClassName, sourceVarName);
                ctx.Append(" : null");
            }
            else if (typeInfo is { IsArrayType: true } or { IsTaskType: true })
            {
                string transformFunction = typeInfo.IsArrayType ? "map" : "then";
                ctx.Append(sourceVarName).Append('.').Append(transformFunction).Append("(e => ");
                RenderInlineProxyConstruction(typeInfo.TypeArgument!, proxyClassName, "e");
                ctx.Append(')');
            }
            else
            {
                ctx.Append($"ProxyBase.fromHandle({proxyClassName}, {sourceVarName})");
            }
        }
    }

    private void RenderHandleExtractionToConsts(IEnumerable<MethodParameterInfo> parameterInfos)
    {
        foreach (MethodParameterInfo parameterInfo in parameterInfos)
        {
            if (parameterInfo.IsInjectedInstanceParameter || !parameterInfo.Type.RequiresCLRTypeConversion || !parameterInfo.Type.ContainsExportedType())
                continue;

            if (symbolNameProvider.GetUserClassSymbolNameIfExists(parameterInfo.Type, SymbolNameFlags.Proxy | SymbolNameFlags.Isolated) is not string proxyClassName)
            {
                throw new ArgumentException($"Invalid conversion-requiring type '{parameterInfo.Type.CLRTypeSyntax}' failed to resolve associated TypeScript proxy name.");
            }

            ctx.Append($"const {GetInteropInvocationVariable(parameterInfo)} = ");
            RenderInlineHandleExtraction(parameterInfo.Type, proxyClassName, parameterInfo.Name);
            ctx.AppendLine(";");
        }

        void RenderInlineHandleExtraction(InteropTypeInfo typeInfo, string proxyClassName, string sourceVarName)
        {
            if (typeInfo is { IsNullableType: true })
            {
                ctx.Append(sourceVarName).Append(" ? ");
                RenderInlineHandleExtraction(typeInfo.TypeArgument!, proxyClassName, sourceVarName);
                ctx.Append(" : null");
            }
            else if (typeInfo is { IsArrayType: true } or { IsTaskType: true })
            {
                string transformFunction = typeInfo.IsArrayType ? "map" : "then";
                ctx.Append(sourceVarName).Append('.').Append(transformFunction).Append("(e => ");
                RenderInlineHandleExtraction(typeInfo.TypeArgument!, proxyClassName, "e");
                ctx.Append(')');
            }
            else
            {
                ctx.Append(sourceVarName).Append(" instanceof ").Append(proxyClassName).Append(" ? ").Append(sourceVarName).Append(".instance : ").Append(sourceVarName);
            }
        }
    }

    private void RenderInteropInvocation(string methodName, IEnumerable<MethodParameterInfo> methodParameters)
    {
        ctx.Append("TypeShimConfig.exports.");
        RenderInteropMethodAccessor(methodName);
        ctx.Append("(");
        RenderMethodInvocationParameters(methodParameters, "this.instance");
        ctx.Append(")");

        void RenderMethodInvocationParameters(IEnumerable<MethodParameterInfo> parameters, string instanceParameterExpression)
        {
            bool isFirst = true;
            foreach (MethodParameterInfo parameter in parameters)
            {
                if (!isFirst) ctx.Append(", ");
                
                ctx.Append(parameter.IsInjectedInstanceParameter ? instanceParameterExpression : GetInteropInvocationVariable(parameter));
                isFirst = false;
            }
        }

        void RenderInteropMethodAccessor(string methodName)
        {
            ctx.Append(ctx.Class.Namespace).Append('.').Append(RenderConstants.InteropClassName(ctx.Class)).Append('.').Append(methodName);
        }
    }

    private static string GetInteropInvocationVariable(MethodParameterInfo param) // TODO: get from ctx localscope (check param.Name call sites!)
    {
        return param.Type.RequiresCLRTypeConversion && param.Type.ContainsExportedType() ? $"{param.Name}Instance" : param.Name;
    }
}