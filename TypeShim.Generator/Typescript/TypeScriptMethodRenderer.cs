using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
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
            ctx.Append(' ');
            RenderConstructorBody();
        }

        void RenderConstructorSignature()
        {
            ctx.Append("constructor(");
            RenderParameterList(constructorInfo.Parameters);
            if (constructorInfo.InitializerObject != null)
            {
                if (constructorInfo.Parameters.Length != 0) ctx.Append(", ");
                string initializerType = TypeScriptSymbolNameRenderer.Render(ctx.Class.Type, ctx, TypeShimSymbolType.Initializer, interop: false);
                ctx.Append(constructorInfo.InitializerObject.Name).Append(": ").Append(initializerType);
            }
            ctx.Append(")");
        }

        void RenderConstructorBody()
        {
            ctx.AppendLine("{");
            using (ctx.Indent())
            {
                RenderHandleExtractionToConsts(constructorInfo.Parameters);
                string proxyClassName = TypeScriptSymbolNameRenderer.Render(ctx.Class.Type, ctx, TypeShimSymbolType.Proxy, interop: false);
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

            ctx.Append(parameterInfo.Name).Append(": ");
            if (parameterInfo.Type.IsDelegateType())
            {
                TypeScriptSymbolNameRenderer.RenderDelegate(parameterInfo.Type, ctx, parameterSymbolType: TypeShimSymbolType.Proxy, returnSymbolType: TypeShimSymbolType.Proxy, interop: false);
            }
            else
            {
                TypeShimSymbolType symbolType = parameterInfo.Type is { RequiresTypeConversion: true, SupportsTypeConversion: true }
                    ? TypeShimSymbolType.ProxyInitializerUnion
                    : TypeShimSymbolType.None;
                ctx.Append(TypeScriptSymbolNameRenderer.Render(parameterInfo.Type, ctx, symbolType, interop: false));
            }
            isFirst = false;
        }
    }

    private void RenderReturnType(MethodInfo methodInfo)
    {
        if (methodInfo.ReturnType is not { RequiresTypeConversion: true, SupportsTypeConversion: true })
        {
            ctx.Append(TypeScriptSymbolNameRenderer.Render(methodInfo.ReturnType, ctx));
        }
        else if (methodInfo.ReturnType.IsDelegateType())
        {
            TypeScriptSymbolNameRenderer.RenderDelegate(methodInfo.ReturnType, ctx, parameterSymbolType: TypeShimSymbolType.ProxyInitializerUnion, returnSymbolType: TypeShimSymbolType.Proxy, interop: false);
        }
        else
        {
            string returnTypeAsProxy = TypeScriptSymbolNameRenderer.Render(methodInfo.ReturnType, ctx, TypeShimSymbolType.Proxy, interop: false);
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
                // user class return type, wrap in proxy
                ctx.Append("const res = ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters);
                ctx.AppendLine(";");
                ctx.Append($"return ");
                
                if (methodInfo.ReturnType.IsDelegateType())
                {
                    RenderInlineDelegateHandleExtraction(methodInfo.ReturnType.ArgumentInfo!, "res");
                }
                else
                {
                    string returnTypeProxyClassName = TypeScriptSymbolNameRenderer.Render(methodInfo.ReturnType.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false);
                    RenderInlineProxyConstruction(methodInfo.ReturnType, returnTypeProxyClassName, "res");
                }
                ctx.AppendLine(";");
            }
            else if (RequiresCharConversion(methodInfo.ReturnType))
            {
                ctx.Append("const retVal = ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters);
                ctx.AppendLine(";");

                ctx.Append("return ");
                RenderNumberToCharConversion(methodInfo.ReturnType, () => ctx.Append("retVal"));
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
    }
    private void RenderInlineProxyConstruction(InteropTypeInfo typeInfo, string proxyClassName, string sourceVarName)
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

    private void RenderInlineDelegateProxyConstruction(DelegateArgumentInfo delegateInfo, string targetDelegateExpression)
    {
        ctx.Append("(");
        foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
        {
            if (param.Index > 0) ctx.Append(", ");
            string paramTypeName = TypeScriptSymbolNameRenderer.Render(param.Type, ctx, TypeShimSymbolType.Proxy, interop: true);
            ctx.Append("arg").Append(param.Index).Append(": ").Append(paramTypeName);
        }
        ctx.Append(") => ").Append(targetDelegateExpression).Append("(");
        // TODO: handle return type conversion (CHECK IF NEEDED?)
        // (param Func<string, MyClass> --> (arg0: string) => return new MyClassProxy(delegate(arg0)))
        foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
        {
            if (param.Index > 0) ctx.Append(", ");

            if (param.Type.RequiresTypeConversion && param.Type.SupportsTypeConversion)
            {
                RenderInlineProxyConstruction(param.Type, TypeScriptSymbolNameRenderer.Render(param.Type.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false), "arg" + param.Index);
            }
            else
            {
                ctx.Append("arg").Append(param.Index);
            }
        }
        ctx.Append(")");
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
            if (parameterInfo.Type.IsDelegateType())
            {
                RenderInlineDelegateProxyConstruction(parameterInfo.Type.ArgumentInfo!, parameterInfo.Name);
            }
            else
            {
                string proxyClassName = TypeScriptSymbolNameRenderer.Render(parameterInfo.Type.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false);
                RenderInlineHandleExtraction(parameterInfo.Type, proxyClassName, parameterInfo.Name);
            }
            ctx.AppendLine(";");
        }

    }
    private void RenderInlineHandleExtraction(InteropTypeInfo typeInfo, string proxyClassName, string sourceVarName)
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
        else if (ctx.SymbolMap.GetClassInfo(typeInfo) is { Constructor: { AcceptsInitializer: true, IsParameterless: true } })
        {
            // accepts initializer or proxy, if proxy, extract handle, if init, pass as is
            ctx.Append(sourceVarName).Append(" instanceof ").Append(proxyClassName).Append(" ? ").Append(sourceVarName).Append(".instance : ").Append(sourceVarName);
        }
        else // simple proxy
        {
            ctx.Append(sourceVarName).Append(".instance");
        }
    }

    /// <summary>
    /// Renders the delegate that wraps the user delegate to extract handles from proxies before invoking the target delegate.
    /// Used when passing a delegate from TS to .NET. 
    /// </summary>
    /// <param name="delegateInfo"></param>
    /// <param name="targetDelegateExpression"></param>
    private void RenderInlineDelegateHandleExtraction(DelegateArgumentInfo delegateInfo, string targetDelegateExpression)
    {
        // Build signature
        ctx.Append("(");
        foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
        {
            if (param.Index > 0) ctx.Append(", ");
            ctx.Append("arg").Append(param.Index).Append(": ").Append(TypeScriptSymbolNameRenderer.Render(param.Type, ctx, TypeShimSymbolType.ProxyInitializerUnion, interop: false));
        }
        ctx.Append(") => ");

        // Invoke target delegate (with optional return type conversion)
        if (delegateInfo.ReturnType.RequiresTypeConversion && delegateInfo.ReturnType.SupportsTypeConversion)
        {
            Debug.Assert(delegateInfo.ReturnType.ManagedType == KnownManagedType.Object, "Non object type that requires type-conversion encountered in delegate return type");
            ctx.Append("{ const retVal = ");
        }

        RenderTargetDelegateInvocation(delegateInfo, targetDelegateExpression);

        if (delegateInfo.ReturnType.RequiresTypeConversion && delegateInfo.ReturnType.SupportsTypeConversion)
        {
            Debug.Assert(delegateInfo.ReturnType.ManagedType == KnownManagedType.Object, "Non object type that requires type-conversion encountered in delegate return type");
            string returnTypeProxyClassName = TypeScriptSymbolNameRenderer.Render(delegateInfo.ReturnType.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false);
            ctx.Append("; return ");
            RenderInlineProxyConstruction(delegateInfo.ReturnType, returnTypeProxyClassName, "retVal");
            ctx.Append(" }");
        }

        void RenderTargetDelegateInvocation(DelegateArgumentInfo delegateInfo, string targetDelegateExpression)
        {
            ctx.Append(targetDelegateExpression).Append("(");
            foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
            {
                if (param.Index > 0) ctx.Append(", ");
                if (param.Type.RequiresTypeConversion && param.Type.SupportsTypeConversion)
                {
                    RenderInlineHandleExtraction(param.Type, TypeScriptSymbolNameRenderer.Render(param.Type.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false), "arg" + param.Index);
                }
                else
                {
                    ctx.Append("arg").Append(param.Index);
                }
            }
            ctx.Append(")");
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
                if(RequiresCharConversion(parameter.Type)) RenderCharToNumberConversion(parameter.Type);
                isFirst = false;
            }
            if (initializerObject == null) return;

            if (!isFirst) ctx.Append(", ");

            ctx.Append("{ ...").Append(initializerObject.Name);

            foreach (PropertyInfo propertyInfo in ctx.Class.Properties)
            {
                if (!RequiresCharConversion(propertyInfo.Type)) continue;
                ctx.Append(", ").Append(propertyInfo.Name).Append(": ").Append(initializerObject.Name).Append('.').Append(propertyInfo.Name);
                RenderCharToNumberConversion(propertyInfo.Type);
            }
            ctx.Append('}');
        }

        void RenderInteropMethodAccessor(string methodName)
        {
            ctx.Append(ctx.Class.Namespace).Append('.').Append(RenderConstants.InteropClassName(ctx.Class)).Append('.').Append(methodName);
        }
    }

    private static bool RequiresCharConversion(InteropTypeInfo typeInfo)
    {
        // dotnet does not marshall chars as strings atm. TypeShim converts from/to numbers while crossing the boundary.
        return typeInfo switch
        {
            { ManagedType: KnownManagedType.Nullable } => RequiresCharConversion(typeInfo.TypeArgument!),
            { ManagedType: KnownManagedType.Task } => RequiresCharConversion(typeInfo.TypeArgument!),
            { ManagedType: KnownManagedType.Char } => true,
            _ => false
        };
    }

    /// <summary>
    /// Assumes type requires char conversion, may behave unexpectedly otherwise
    /// </summary>
    /// <param name="typeInfo"></param>
    void RenderCharToNumberConversion(InteropTypeInfo typeInfo)
    {
        if (typeInfo.ManagedType == KnownManagedType.Char)
        {
            ctx.Append(".charCodeAt(0)");
        }
        else if (typeInfo is { ManagedType: KnownManagedType.Nullable })
        {
            ctx.Append('?');
            RenderCharToNumberConversion(typeInfo.TypeArgument!);
        }
        else if (typeInfo is { ManagedType: KnownManagedType.Task })
        {
            ctx.Append(".then(c => c");
            if (typeInfo.TypeArgument is { ManagedType: KnownManagedType.Nullable })
            {
                ctx.Append('?');
            }
            RenderCharToNumberConversion(typeInfo.TypeArgument!);
        }
    }

    /// <summary>
    /// Assumes type requires char conversion, may behave unexpectedly otherwise
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <param name="renderCharExpression"></param>
    void RenderNumberToCharConversion(InteropTypeInfo typeInfo, Action renderCharExpression)
    {
        if (typeInfo.ManagedType == KnownManagedType.Nullable)
        {
            renderCharExpression();
            ctx.Append(" ? ");
            RenderNumberToCharConversion(typeInfo.TypeArgument!, renderCharExpression);
            ctx.Append(" : null");
            return;
        } 
        else if (typeInfo.ManagedType == KnownManagedType.Char)
        {
            ctx.Append("String.fromCharCode(");
        }

        renderCharExpression();

        if (typeInfo.ManagedType == KnownManagedType.Char)
        {
            ctx.Append(")");
        }
        else if (typeInfo is { ManagedType: KnownManagedType.Task })
        {
            ctx.Append(".then(c => ");
            RenderNumberToCharConversion(typeInfo.TypeArgument!, () => ctx.Append("c"));
        }
    }

    private static string GetInteropInvocationVariable(MethodParameterInfo param) // TODO: get from ctx localscope (check param.Name call sites!)
    {
        return param.Type.RequiresTypeConversion && param.Type.SupportsTypeConversion ? $"{param.Name}Instance" : param.Name;
    }
}