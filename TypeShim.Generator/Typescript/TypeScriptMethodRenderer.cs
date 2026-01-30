using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal sealed class TypeScriptMethodRenderer(RenderContext ctx)
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
                
                string returnType = ctx.SymbolMap.GetUserClassSymbolName(ctx.Class, TypeShimSymbolType.Initializer);
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
                string proxyClassName = ctx.SymbolMap.GetUserClassSymbolName(ctx.Class, TypeShimSymbolType.Proxy);
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
            RenderReturnType(methodInfo);
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
            RenderReturnType(methodInfo);
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

            ctx.Append(parameterInfo.Name).Append(": ").Append(ResolveParameterType(parameterInfo.Type));
            isFirst = false;
        }

        string ResolveParameterType(InteropTypeInfo typeInfo)
        {
            if (!typeInfo.RequiresTypeConversion || !typeInfo.SupportsTypeConversion)
            {
                return typeInfo.TypeScriptTypeSyntax.Render();
            }

            if (typeInfo.IsDelegateType())
            {
                // delegate parameters, are not to be invoked from JS.
                // Thus,
                // return types are always proxies
                // parameters are always proxies

                // TODO: implement delegate parameter type rendering properly
                return typeInfo.TypeScriptTypeSyntax.Render();
            }

            ClassInfo classInfo = ctx.SymbolMap.GetClassInfo(typeInfo.GetInnermostType());
            TypeShimSymbolType symbolName = classInfo is { Constructor: { IsParameterless: true, AcceptsInitializer: true } } 
                ? TypeShimSymbolType.ProxyInitializerUnion // initializer also accepted
                : TypeShimSymbolType.Proxy;

            return ctx.SymbolMap.GetUserClassSymbolName(classInfo, typeInfo, symbolName);
        }
    }

    private void RenderReturnType(MethodInfo methodInfo)
    {
        if (methodInfo.ReturnType is not { RequiresTypeConversion: true, SupportsTypeConversion: true })
        {
            ctx.Append(methodInfo.ReturnType.TypeScriptTypeSyntax.Render());
            return;
        }

        if (methodInfo.ReturnType.IsDelegateType())
        {
            // delegate return types are to be invoked from JS (or passed back as a parameter elsewhere??)
            // thus,
            // return types are always proxies
            // arguments are either proxies or initializers

            // TODO: implement delegate return type rendering properly
            ctx.Append(methodInfo.ReturnType.TypeScriptTypeSyntax.Render());
        }
        else
        {
            string returnTypeAsProxy = ctx.SymbolMap.GetUserClassSymbolName(methodInfo.ReturnType, TypeShimSymbolType.Proxy);
            ctx.Append(returnTypeAsProxy);
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
                string proxyClassName = ctx.SymbolMap.GetUserClassSymbolName(methodInfo.ReturnType.GetInnermostType(), TypeShimSymbolType.Proxy);
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

                RenderCharConversionIfNecessary(methodInfo.ReturnType, () =>
                {
                    RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters);
                });

                ctx.AppendLine(";");
            }
        }
        ctx.AppendLine("}");

        void RenderCharConversionIfNecessary(InteropTypeInfo typeInfo, Action renderCharExpression)
        {
            // dotnet does not marshall chars as strings, instead as numbers. TypeShim converts to strings on the TS side.
            if (methodInfo.ReturnType.ManagedType == KnownManagedType.Char)
                ctx.Append("String.fromCharCode(");

            renderCharExpression();
            
            if (methodInfo.ReturnType.ManagedType == KnownManagedType.Char)
                ctx.Append(")");
            if (methodInfo.ReturnType is { ManagedType: KnownManagedType.Task, TypeArgument.ManagedType: KnownManagedType.Char })
                ctx.Append(".then(c => String.fromCharCode(c))");
        }

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

            ctx.Append("const ");
            ctx.Append(GetInteropInvocationVariable(parameterInfo));
            ctx.Append(" = ");
            string proxyClassName = ctx.SymbolMap.GetUserClassSymbolName(parameterInfo.Type.GetInnermostType(), TypeShimSymbolType.Proxy);
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
            else if (typeInfo.IsDelegateType())
            {
                // delegate parameters need to be wrapped in another delegate that extracts the handle
                //ctx.Append($"(e => ProxyBase.fromHandle({proxyClassName}, e))");
                // TODO: implement delegate parameter handle extraction properly
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
                RenderCharConversionIfNecessary(parameter);
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

        void RenderCharConversionIfNecessary(MethodParameterInfo parameter)
        {
            // dotnet does not marshall chars as strings atm. We convert from/to numbers while this is the case.
            if (parameter.Type.ManagedType == KnownManagedType.Char)
                ctx.Append(".charCodeAt(0)");
            if (parameter.Type is { ManagedType: KnownManagedType.Task, TypeArgument.ManagedType: KnownManagedType.Char })
                ctx.Append(".then(c => c.charCodeAt(0))");
        }
    }

    private static string GetInteropInvocationVariable(MethodParameterInfo param) // TODO: get from ctx localscope (check param.Name call sites!)
    {
        return param.Type.RequiresTypeConversion && param.Type.SupportsTypeConversion ? $"{param.Name}Instance" : param.Name;
    }
}