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
            ctx.Append("private constructor() {");
            if (!ctx.Class.IsStatic)
            {
                ctx.Append(" super(undefined!); "); // so TS compiles
            }
            ctx.AppendLine("}");
        }
        else if (constructorInfo == null)
        {
            ctx.AppendLine("private constructor() { super(undefined!); }");
        }
        else
        {
            RenderConstructorSignature();
            RenderConstructorBody();
        }

        void RenderConstructorSignature()
        {
            ctx.Append("constructor(");
            RenderParameterList(constructorInfo.Parameters);
            if (constructorInfo.InitializerObject != null)
            {
                if (constructorInfo.Parameters.Length != 0) ctx.Append(", ");
                
                string returnType = symbolNameProvider.GetUserClassSymbolName(ctx.Class, SymbolNameFlags.Initializer);
                ctx.Append(constructorInfo.InitializerObject.Name).Append(": ").Append(returnType);
            }
            ctx.Append(")");
        }

        void RenderConstructorBody()
        {
            ctx.AppendLine(" {");
            using (ctx.Indent())
            {
                RenderHandleExtractionToConsts(constructorInfo.Parameters);
                string proxyClassName = symbolNameProvider.GetUserClassSymbolName(ctx.Class, SymbolNameFlags.Proxy);
                ctx.Append("super(");
                RenderInteropInvocation(constructorInfo.Name, constructorInfo.Parameters, constructorInfo.InitializerObject);
                ctx.AppendLine(");");
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

            if (methodInfo.ReturnType is { RequiresTypeConversion: true, SupportsTypeConversion: true })
            {
                ClassInfo classInfo = ctx.GetClassInfo(methodInfo.ReturnType.GetInnermostType());
                ctx.Append(symbolNameProvider.GetUserClassSymbolName(classInfo, methodInfo.ReturnType, SymbolNameFlags.Proxy));
            }
            else
            {
                ctx.Append(symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType));
            }
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
            ctx.Append("get ").Append(propertyInfo.Name).Append("(): ");

            if (methodInfo.ReturnType is { RequiresTypeConversion: true, SupportsTypeConversion: true })
            {
                ClassInfo classInfo = ctx.GetClassInfo(methodInfo.ReturnType.GetInnermostType());
                ctx.Append(symbolNameProvider.GetUserClassSymbolName(classInfo, methodInfo.ReturnType, SymbolNameFlags.Proxy));
            }
            else
            {
                ctx.Append(symbolNameProvider.GetNakedSymbolReference(methodInfo.ReturnType));
            }
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
            if (!isFirst) ctx.Append(", ");

            ctx.Append(parameterInfo.Name).Append(": ").Append(ResolveReturnType(parameterInfo.Type));
            isFirst = false;
        }

        string ResolveReturnType(InteropTypeInfo typeInfo)
        {
            if (!typeInfo.RequiresTypeConversion || !typeInfo.SupportsTypeConversion)
            {
                return symbolNameProvider.GetNakedSymbolReference(typeInfo);
            }

            ClassInfo classInfo = ctx.GetClassInfo(typeInfo.GetInnermostType());
            if (classInfo is { Constructor: { IsParameterless: true, AcceptsInitializer: true } })
            {
                return symbolNameProvider.GetUserClassSymbolName(classInfo, typeInfo, SymbolNameFlags.ProxyInitializerUnion);
            }

            return symbolNameProvider.GetUserClassSymbolName(classInfo, typeInfo, SymbolNameFlags.Proxy);
        }
    }

    private void RenderMethodBody(MethodInfo methodInfo)
    {
        ctx.AppendLine(" {");
        using (ctx.Indent())
        {
            RenderHandleExtractionToConsts(methodInfo.Parameters);

            if (methodInfo.ReturnType is { RequiresTypeConversion: true, SupportsTypeConversion: true })
            {
                ClassInfo classInfo = ctx.GetClassInfo(methodInfo.ReturnType.GetInnermostType());
                string proxyClassName = symbolNameProvider.GetUserClassSymbolName(classInfo, SymbolNameFlags.Proxy);
                // user class return type, wrap in proxy
                ctx.Append("const res = ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters);
                ctx.AppendLine(";");
                ctx.Append($"return ");
                RenderInlineProxyConstruction(methodInfo.ReturnType, proxyClassName, "res");
                ctx.AppendLine(";");
            }
            else
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
            if (parameterInfo.IsInjectedInstanceParameter || !parameterInfo.Type.RequiresTypeConversion || !parameterInfo.Type.SupportsTypeConversion)
                continue;

            ctx.Append($"const {GetInteropInvocationVariable(parameterInfo)} = ");
            ClassInfo classInfo = ctx.GetClassInfo(parameterInfo.Type.GetInnermostType());
            string proxyClassName = symbolNameProvider.GetUserClassSymbolName(classInfo, SymbolNameFlags.Proxy);
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

    private void RenderInteropInvocation(string methodName, IEnumerable<MethodParameterInfo> methodParameters, MethodParameterInfo? initializerObject = null)
    {
        ctx.Append("TypeShimConfig.exports.");
        RenderInteropMethodAccessor(methodName);
        ctx.Append("(");
        RenderMethodInvocationParameters("this.instance");
        ctx.Append(")");

        void RenderMethodInvocationParameters(string instanceParameterExpression)
        {
            bool isFirst = true;
            foreach (MethodParameterInfo parameter in methodParameters)
            {
                if (!isFirst) ctx.Append(", ");
                
                ctx.Append(parameter.IsInjectedInstanceParameter ? instanceParameterExpression : GetInteropInvocationVariable(parameter));
                isFirst = false;
            }
            if (initializerObject == null) return;

            if (!isFirst) ctx.Append(", ");
            ctx.Append(initializerObject.Name);
        }

        void RenderInteropMethodAccessor(string methodName)
        {
            ctx.Append(ctx.Class.Namespace).Append('.').Append(RenderConstants.InteropClassName(ctx.Class)).Append('.').Append(methodName);
        }
    }

    private static string GetInteropInvocationVariable(MethodParameterInfo param) // TODO: get from ctx localscope (check param.Name call sites!)
    {
        return param.Type.RequiresTypeConversion && param.Type.SupportsTypeConversion ? $"{param.Name}Instance" : param.Name;
    }
}