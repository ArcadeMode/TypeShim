using Microsoft.CodeAnalysis;
using System.Data.Common;
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
                ctx.Append(constructorInfo.InitializerObject.Name).Append(": ");
                TypeScriptSymbolNameRenderer.Render(ctx.Class.Type, ctx, TypeShimSymbolType.Initializer, interop: false);
            }
            ctx.Append(")");
        }

        void RenderConstructorBody()
        {
            ctx.AppendLine("{");
            using (ctx.Indent())
            {
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
                TypeScriptSymbolNameRenderer.Render(parameterInfo.Type, ctx, symbolType, interop: false);
            }
            isFirst = false;
        }
    }

    private void RenderReturnType(MethodInfo methodInfo)
    {
        if (methodInfo.ReturnType is not { RequiresTypeConversion: true, SupportsTypeConversion: true })
        {
            TypeScriptSymbolNameRenderer.Render(methodInfo.ReturnType, ctx);
        }
        else if (methodInfo.ReturnType.IsDelegateType()) //TODO: deal with nullable delegate return types
        {
            TypeScriptSymbolNameRenderer.RenderDelegate(methodInfo.ReturnType, ctx, parameterSymbolType: TypeShimSymbolType.ProxyInitializerUnion, returnSymbolType: TypeShimSymbolType.Proxy, interop: false);
        }
        else
        {
            TypeScriptSymbolNameRenderer.Render(methodInfo.ReturnType, ctx, TypeShimSymbolType.Proxy, interop: false);
        }
    }

    private void RenderMethodBody(MethodInfo methodInfo)
    {
        ctx.AppendLine(" {");
        using (ctx.Indent())
        {
            bool requiresProxyConversion = methodInfo.ReturnType is { RequiresTypeConversion: true, SupportsTypeConversion: true };
            bool requiresCharConversion = RequiresCharConversion(methodInfo.ReturnType);
            if (requiresProxyConversion || requiresCharConversion)
            {
                ctx.Append("const res = ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters);
                ctx.AppendLine(";");

                ctx.Append($"return ");
                void accessorRenderer() => ctx.Append("res");
                if (methodInfo.ReturnType.IsDelegateType())
                {
                    RenderInlineDelegateHandleExtraction(methodInfo.ReturnType.ArgumentInfo!, accessorRenderer);
                }
                else if (requiresCharConversion)
                {
                    RenderNumberToCharConversion(methodInfo.ReturnType, accessorRenderer);
                }
                else
                {
                    //string returnTypeProxyClassName = TypeScriptSymbolNameRenderer.Render(methodInfo.ReturnType.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false);
                    RenderInlineProxyConstruction(methodInfo.ReturnType, "res");
                }
            }
            else
            {
                ctx.Append(methodInfo.ReturnType.ManagedType == KnownManagedType.Void ? string.Empty : "return ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters);
            }
            ctx.AppendLine(";");
        }
        ctx.AppendLine("}");
    }

    private void RenderInlineProxyConstruction(InteropTypeInfo typeInfo, string sourceVarName)
    {
        if (typeInfo is { IsNullableType: true })
        {
            ctx.Append(sourceVarName).Append(" ? ");
            RenderInlineProxyConstruction(typeInfo.TypeArgument!, sourceVarName);
            ctx.Append(" : null");
        }
        else if (typeInfo is { IsArrayType: true } or { IsTaskType: true })
        {
            string transformFunction = typeInfo.IsArrayType ? "map" : "then";
            ctx.Append(sourceVarName).Append('.').Append(transformFunction).Append("(e => ");
            RenderInlineProxyConstruction(typeInfo.TypeArgument!, "e");
            ctx.Append(')');
        }
        else
        {
            ctx.Append("ProxyBase.fromHandle(");
            TypeScriptSymbolNameRenderer.Render(typeInfo, ctx, TypeShimSymbolType.Proxy, interop: false);
            ctx.Append(", ");
            ctx.Append(sourceVarName);
            ctx.Append(")");
        }
    }

    private void RenderInlineDelegateProxyConstruction(DelegateArgumentInfo delegateInfo, Action targetDelegateExpressionRenderer)
    {
        ctx.Append("(");
        foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
        {
            if (param.Index > 0) ctx.Append(", ");

            ctx.Append("arg").Append(param.Index).Append(": ");
            TypeScriptSymbolNameRenderer.Render(param.Type, ctx, TypeShimSymbolType.Proxy, interop: true);
        }
        ctx.Append(") => ");
        // Invoke target delegate (with optional return type conversion)
        if (delegateInfo.ReturnType.RequiresTypeConversion && delegateInfo.ReturnType.SupportsTypeConversion)
        {
            ctx.Append("{ const retVal = ");
        }

        bool requiresCharConversion = RequiresCharConversion(delegateInfo.ReturnType);
        if (requiresCharConversion)
        {
            RenderCharToNumberConversion(delegateInfo.ReturnType, () => RenderTargetDelegateInvocation(delegateInfo, targetDelegateExpressionRenderer));
        }
        else
        {
            RenderTargetDelegateInvocation(delegateInfo, targetDelegateExpressionRenderer);
        }

        if (delegateInfo.ReturnType.RequiresTypeConversion && delegateInfo.ReturnType.SupportsTypeConversion)
        {
            ctx.Append("; return ");
            //string returnTypeProxyClassName = TypeScriptSymbolNameRenderer.Render(delegateInfo.ReturnType.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false);
            RenderInlineHandleExtraction(delegateInfo.ReturnType, () => ctx.Append("retVal"));
            ctx.Append(" }");
        }

        void RenderTargetDelegateInvocation(DelegateArgumentInfo delegateInfo, Action targetDelegateExpressionRenderer)
        {
            targetDelegateExpressionRenderer();
            ctx.Append("(");
            foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
            {
                if (param.Index > 0) ctx.Append(", ");

                if (param.Type.RequiresTypeConversion && param.Type.SupportsTypeConversion)
                {
                    //TypeScriptSymbolNameRenderer.Render(param.Type.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false)
                    RenderInlineProxyConstruction(param.Type, "arg" + param.Index);
                }
                else if (RequiresCharConversion(param.Type))
                {
                    RenderNumberToCharConversion(param.Type, () => ctx.Append("arg" + param.Index));
                }
                else
                {
                    ctx.Append("arg").Append(param.Index);
                }
            }
            ctx.Append(")");
        }            
    }

    private void RenderInlineHandleExtraction(InteropTypeInfo typeInfo, Action expressionRenderer)
    {
        if (typeInfo.IsDelegateType())
        {
            RenderInlineDelegateProxyConstruction(typeInfo.ArgumentInfo!, expressionRenderer);
            return;
        }

        expressionRenderer();
        if (typeInfo is { IsNullableType: true })
        {
            ctx.Append(" ? ");
            RenderInlineHandleExtraction(typeInfo.TypeArgument!, expressionRenderer);
            ctx.Append(" : null");
        }
        else if (typeInfo is { IsArrayType: true } or { IsTaskType: true })
        {
            string transformFunction = typeInfo.IsArrayType ? "map" : "then";
            ctx.Append('.').Append(transformFunction).Append("(e => ");
            RenderInlineHandleExtraction(typeInfo.TypeArgument!, () => ctx.Append("e"));
            ctx.Append(')');
        }
        else if (ctx.SymbolMap.GetClassInfo(typeInfo) is { Constructor: { AcceptsInitializer: true, IsParameterless: true } })
        {
            // accepts initializer or proxy, if proxy, extract handle, if init, pass as is
            ctx.Append(" instanceof ");
            TypeScriptSymbolNameRenderer.Render(typeInfo, ctx, TypeShimSymbolType.Proxy, interop: false);
            ctx.Append(" ? ");
            expressionRenderer();
            ctx.Append(".instance : ");
            expressionRenderer();
        }
        else // simple proxy
        {
            ctx.Append(".instance");
        }
    }

    /// <summary>
    /// Renders the delegate that wraps the user delegate to extract handles from proxies before invoking the target delegate.
    /// Used when passing a delegate from TS to .NET. 
    /// </summary>
    /// <param name="delegateInfo"></param>
    /// <param name="targetDelegateExpression"></param>
    private void RenderInlineDelegateHandleExtraction(DelegateArgumentInfo delegateInfo, Action expressionRenderer)
    {
        // Build signature
        ctx.Append("(");
        foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
        {
            if (param.Index > 0) ctx.Append(", ");
            ctx.Append("arg").Append(param.Index).Append(": ");
            TypeScriptSymbolNameRenderer.Render(param.Type, ctx, TypeShimSymbolType.ProxyInitializerUnion, interop: false);
        }
        ctx.Append(") => ");

        // Invoke target delegate (with optional return type conversion)
        bool requiresProxyConversion = delegateInfo.ReturnType.RequiresTypeConversion && delegateInfo.ReturnType.SupportsTypeConversion;
        bool requiresCharConversion = RequiresCharConversion(delegateInfo.ReturnType);
        if (requiresProxyConversion)
        {
            ctx.Append("{ const retVal = ");
        }

        if (requiresCharConversion)
        {
            RenderNumberToCharConversion(delegateInfo.ReturnType, () => RenderTargetDelegateInvocation(delegateInfo, expressionRenderer));
        }
        else
        {
            RenderTargetDelegateInvocation(delegateInfo, expressionRenderer);
        }

        if (requiresProxyConversion)
        {
            //string returnTypeProxyClassName = TypeScriptSymbolNameRenderer.Render(delegateInfo.ReturnType.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false);
            ctx.Append("; return ");
            RenderInlineProxyConstruction(delegateInfo.ReturnType, "retVal");
            ctx.Append(" }");
        }

        void RenderTargetDelegateInvocation(DelegateArgumentInfo delegateInfo, Action expressionRenderer)
        {
            expressionRenderer();
            ctx.Append("(");
            foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
            {
                if (param.Index > 0) ctx.Append(", ");
                if (param.Type.RequiresTypeConversion && param.Type.SupportsTypeConversion)
                {
                    //TypeScriptSymbolNameRenderer.Render(param.Type.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false)
                    RenderInlineHandleExtraction(param.Type, () => ctx.Append("arg").Append(param.Index));
                }
                else if (RequiresCharConversion(param.Type))
                {
                    RenderCharToNumberConversion(param.Type, () => ctx.Append("arg").Append(param.Index));
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
                bool requiresProxyConversion = parameter.Type.RequiresTypeConversion && parameter.Type.SupportsTypeConversion;
                bool requiresCharConversion = RequiresCharConversion(parameter.Type);
                void renderParameter() => ctx.Append(parameter.IsInjectedInstanceParameter ? instanceParameterExpression : parameter.Name);
                if (!parameter.IsInjectedInstanceParameter && (requiresCharConversion || requiresProxyConversion))
                {
                    RenderTypeConversionForInteropCall(parameter.Type, requiresProxyConversion, requiresCharConversion, renderParameter);
                }
                else
                {
                    renderParameter();
                }
                isFirst = false;
            }
            if (initializerObject == null) return;

            if (!isFirst) ctx.Append(", ");
            RenderInitializerParameter(initializerObject);
        }

        void RenderInteropMethodAccessor(string methodName)
        {
            ctx.Append(ctx.Class.Namespace).Append('.').Append(RenderConstants.InteropClassName(ctx.Class)).Append('.').Append(methodName);
        }

        void RenderInitializerParameter(MethodParameterInfo initializerObject)
        {
            ctx.Append("{ ...").Append(initializerObject.Name);
            foreach (PropertyInfo propertyInfo in ctx.Class.Properties)
            {
                bool requiresProxyConversion = propertyInfo.Type.RequiresTypeConversion && propertyInfo.Type.SupportsTypeConversion;
                bool requiresCharConversion = RequiresCharConversion(propertyInfo.Type);
                if (!requiresCharConversion && !requiresProxyConversion)
                {
                    continue;
                }

                void renderPropertyAccessorExpression() => ctx.Append(initializerObject.Name).Append('.').Append(propertyInfo.Name);
                ctx.Append(", ").Append(propertyInfo.Name).Append(": ");
                RenderTypeConversionForInteropCall(propertyInfo.Type, requiresProxyConversion, requiresCharConversion, renderPropertyAccessorExpression);
            }
            ctx.Append(" }");
        }

        void RenderTypeConversionForInteropCall(InteropTypeInfo typeInfo, bool requiresProxyConversion, bool requiresCharConversion, Action renderParameter)
        {
            if (typeInfo.IsDelegateType() && (requiresProxyConversion || requiresCharConversion))
            {
                RenderInlineDelegateProxyConstruction(typeInfo.ArgumentInfo!, renderParameter);
            }
            else if (requiresProxyConversion)
            {
                //string proxyClassName = TypeScriptSymbolNameRenderer.Render(typeInfo.GetInnermostType(), ctx, TypeShimSymbolType.Proxy, interop: false);
                RenderInlineHandleExtraction(typeInfo, renderParameter);
            }
            else if (requiresCharConversion)
            {
                RenderCharToNumberConversion(typeInfo, renderParameter);
            }
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
            { ArgumentInfo: DelegateArgumentInfo argumentInfo } when (typeInfo.IsDelegateType()) => RequiresCharConversion(argumentInfo.ReturnType) || argumentInfo.ParameterTypes.Any(RequiresCharConversion),
            _ => false
        };
    }

    /// <summary>
    /// Assumes type requires char conversion, may behave unexpectedly otherwise
    /// </summary>
    /// <param name="typeInfo"></param>
    void RenderCharToNumberConversion(InteropTypeInfo typeInfo, Action renderStringExpression)
    {
        if (typeInfo is { ManagedType: KnownManagedType.Nullable })
        {
            renderStringExpression();
            ctx.Append(" ? ");
            RenderCharToNumberConversion(typeInfo.TypeArgument!, renderStringExpression);
            ctx.Append(" : null");
            return;
        }

        renderStringExpression();
        if (typeInfo.ManagedType == KnownManagedType.Char)
        {
            ctx.Append(".charCodeAt(0)");
        }
        else if (typeInfo is { ManagedType: KnownManagedType.Task })
        {
            ctx.Append(".then(c => ");
            RenderCharToNumberConversion(typeInfo.TypeArgument!, () => ctx.Append("c"));
            ctx.Append(")");
        }
    }

    /// <summary>
    /// Assumes type requires char conversion, may behave unexpectedly otherwise
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <param name="renderNumberExpression"></param>
    void RenderNumberToCharConversion(InteropTypeInfo typeInfo, Action renderNumberExpression)
    {
        if (typeInfo.ManagedType == KnownManagedType.Nullable)
        {
            renderNumberExpression();
            ctx.Append(" ? ");
            RenderNumberToCharConversion(typeInfo.TypeArgument!, renderNumberExpression);
            ctx.Append(" : null");
            return;
        } 
        else if (typeInfo.ManagedType == KnownManagedType.Char)
        {
            ctx.Append("String.fromCharCode(");
        }

        renderNumberExpression();

        if (typeInfo.ManagedType == KnownManagedType.Char)
        {
            ctx.Append(")");
        }
        else if (typeInfo is { ManagedType: KnownManagedType.Task })
        {
            ctx.Append(".then(c => ");
            RenderNumberToCharConversion(typeInfo.TypeArgument!, () => ctx.Append("c"));
            ctx.Append(")");
        }
    }
}