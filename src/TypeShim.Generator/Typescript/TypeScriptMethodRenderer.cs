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
            TypeScriptJSDocRenderer.RenderJSDoc(ctx, constructorInfo.Comment);
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
                RenderInteropInvocation(constructorInfo.Name, constructorInfo.Parameters, instanceParameter: null, constructorInfo.InitializerObject);
                ctx.AppendLine(");");
            }
            ctx.AppendLine("}");
        }
    }

    internal void RenderProxyMethod(MethodInfo methodInfo)
    {
        TypeScriptJSDocRenderer.RenderJSDoc(ctx, methodInfo.Comment);
        RenderProxyMethodSignature(methodInfo);
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
        TypeScriptJSDocRenderer.RenderJSDoc(ctx, propertyInfo.Comment);
        MethodInfo getterInfo = propertyInfo.GetMethod;
        RenderProxyPropertyGetterSignature(getterInfo);
        RenderMethodBody(getterInfo);

        if (propertyInfo.SetMethod is MethodInfo setterInfo)
        {
            ctx.AppendLine();
            RenderProxyPropertySetterSignature(setterInfo);
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

            bool isDelegate = parameterInfo.Type.IsDelegateType() || (parameterInfo.Type.IsNullableType && parameterInfo.Type.TypeArgument!.IsDelegateType());
            TypeShimSymbolType returnSymbolType = !isDelegate && ctx.SymbolMap.IsConversionRequiringClassOrDelegate(parameterInfo.Type)
                ? TypeShimSymbolType.ProxyInitializerUnion
                : TypeShimSymbolType.None;
            TypeScriptSymbolNameRenderer.Render(parameterInfo.Type, ctx, returnSymbolType, parameterSymbolType: TypeShimSymbolType.Proxy, interop: false);
            isFirst = false;
        }
    }

    private void RenderReturnType(MethodInfo methodInfo)
    {
        if (!ctx.SymbolMap.IsConversionRequiringClassOrDelegate(methodInfo.ReturnType))
        {
            TypeScriptSymbolNameRenderer.Render(methodInfo.ReturnType, ctx);
        }
        else
        {
            TypeScriptSymbolNameRenderer.Render(methodInfo.ReturnType, ctx, returnSymbolType: TypeShimSymbolType.Proxy, parameterSymbolType: TypeShimSymbolType.ProxyInitializerUnion, interop: false);
        }
    }

    private void RenderMethodBody(MethodInfo methodInfo)
    {
        ctx.AppendLine(" {");
        using (ctx.Indent())
        {
            bool requiresProxyConversion = ctx.SymbolMap.IsConversionRequiringClassOrDelegate(methodInfo.ReturnType);
            bool requiresCharConversion = RequiresCharConversion(methodInfo.ReturnType);
            if (requiresProxyConversion || requiresCharConversion)
            {
                ctx.Append("const res = ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters, methodInfo.InstanceParameter);
                ctx.AppendLine(";");

                ctx.Append($"return ");
                void resRenderer() => ctx.Append("res");
                RenderInlineProxyConstruction(methodInfo.ReturnType, resRenderer);
            }
            else
            {
                ctx.Append(methodInfo.ReturnType.ManagedType == KnownManagedType.Void ? string.Empty : "return ");
                RenderInteropInvocation(methodInfo.Name, methodInfo.Parameters, methodInfo.InstanceParameter);
            }
            ctx.AppendLine(";");

            if (methodInfo.MatchesDisposeSignature())
            {
                ctx.AppendLine("this.instance.dispose();");
            }
        }
        ctx.AppendLine("}");
    }

    private void RenderInlineProxyConstruction(InteropTypeInfo typeInfo, Action expressionRenderer)
    {
        if (typeInfo.IsDelegateType())
        {
            RenderInlineDelegateHandleExtraction(typeInfo.ArgumentInfo!, expressionRenderer);
            return;
        }

        if (typeInfo is { IsNullableType: true })
        {
            expressionRenderer();
            ctx.Append(" ? ");
            RenderInlineProxyConstruction(typeInfo.TypeArgument!, expressionRenderer);
            ctx.Append(" : null");
        }
        else if (typeInfo is { IsArrayType: true } or { IsTaskType: true })
        {
            string transformFunction = typeInfo.IsArrayType ? "map" : "then";
            expressionRenderer();
            ctx.Append('.').Append(transformFunction).Append("(e => ");
            RenderInlineProxyConstruction(typeInfo.TypeArgument!, () => ctx.Append("e"));
            ctx.Append(')');
        }
        else if (typeInfo.ManagedType == KnownManagedType.Char)
        {
            ctx.Append("String.fromCharCode(");
            expressionRenderer();
            ctx.Append(")");
        }
        else if (ctx.SymbolMap.IsConversionRequiringClassOrDelegate(typeInfo))
        {
            ctx.Append("ProxyBase.fromHandle(");
            TypeScriptSymbolNameRenderer.Render(typeInfo, ctx, TypeShimSymbolType.Proxy, interop: false);
            ctx.Append(", ");
            expressionRenderer();
            ctx.Append(")");
        }
        else
        {
            // Enums cross as their underlying number; the value is used as-is.
            expressionRenderer();
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
        Action renderExpression = () => RenderTargetDelegateInvocation(delegateInfo, targetDelegateExpressionRenderer);
        bool requiresRetVal = delegateInfo.ReturnType.IsNullableType || ctx.SymbolMap.IsConversionRequiringClassOrDelegate(delegateInfo.ReturnType);
        if (requiresRetVal)
        {
            ctx.Append("{ const retVal = ");
            renderExpression();
            ctx.Append("; return ");
            renderExpression = () => ctx.Append("retVal");
        }

        RenderInlineHandleExtraction(delegateInfo.ReturnType, renderExpression);
        if (requiresRetVal)
        {
            ctx.Append(" }");
        } 

        void RenderTargetDelegateInvocation(DelegateArgumentInfo delegateInfo, Action targetDelegateExpressionRenderer)
        {
            targetDelegateExpressionRenderer();
            ctx.Append("(");
            foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
            {
                if (param.Index > 0) ctx.Append(", ");

                if (ctx.SymbolMap.IsConversionRequiringClassOrDelegate(param.Type) || RequiresCharConversion(param.Type))
                {
                    RenderInlineProxyConstruction(param.Type, () => ctx.Append("arg" + param.Index));
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
        else if (typeInfo.ManagedType == KnownManagedType.Char)
        {
            ctx.Append(".charCodeAt(0)");
        }
        else if (typeInfo is { IsArrayType: true } or { IsTaskType: true } && typeInfo.TypeArgument != null)
        {
            string transformFunction = typeInfo.IsArrayType ? "map" : "then";
            ctx.Append('.').Append(transformFunction).Append("(e => ");
            RenderInlineHandleExtraction(typeInfo.TypeArgument, () => ctx.Append("e"));
            ctx.Append(')');
        }
        else if (typeInfo.IsTSExport && ctx.SymbolMap.GetNamedTypeInfo(typeInfo) is ClassInfo classInfo)
        {
            if (classInfo is { Constructor: { AcceptsInitializer: true, IsParameterless: true } })
            {
                // accepts initializer or proxy, if proxy, extract handle, if init, pass as is
                ctx.Append(" instanceof ");
                TypeScriptSymbolNameRenderer.Render(typeInfo, ctx, TypeShimSymbolType.Proxy, interop: false);
                ctx.Append(" ? ");
                expressionRenderer();
                ctx.Append(".instance : ");
                expressionRenderer();
            } 
            else
            {
                // simple proxy, extract handle
                ctx.Append(".instance");
            }
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

        Action renderExpression = () => RenderTargetDelegateInvocation(delegateInfo, expressionRenderer);
        bool requiresRetVal = delegateInfo.ReturnType.IsNullableType || ctx.SymbolMap.IsConversionRequiringClassOrDelegate(delegateInfo.ReturnType);
        if (requiresRetVal)
        {
            ctx.Append("{ const retVal = ");
            renderExpression();
            ctx.Append("; return ");
            renderExpression = () => ctx.Append("retVal");
        }

        if (ctx.SymbolMap.IsConversionRequiringClassOrDelegate(delegateInfo.ReturnType) || RequiresCharConversion(delegateInfo.ReturnType))
        {
            RenderInlineProxyConstruction(delegateInfo.ReturnType, renderExpression);
        }
        else
        {
            renderExpression();
        }

        if (requiresRetVal)
        {
            ctx.Append(" }");
        }

        void RenderTargetDelegateInvocation(DelegateArgumentInfo delegateInfo, Action expressionRenderer)
        {
            expressionRenderer();
            ctx.Append("(");
            foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
            {
                if (param.Index > 0) ctx.Append(", ");
                
                RenderInlineHandleExtraction(param.Type, () => ctx.Append("arg").Append(param.Index));
            }
            ctx.Append(")");
        }
    }

    private void RenderInteropInvocation(string methodName, IEnumerable<MethodParameterInfo> methodParameters, MethodParameterInfo? instanceParameter = null, MethodParameterInfo? initializerObject = null)
    {
        ctx.Append("TypeShimConfig.exports.");
        RenderInteropMethodAccessor(methodName);
        ctx.Append("(");
        RenderMethodInvocationParameters();
        ctx.Append(")");

        void RenderMethodInvocationParameters()
        {
            bool isFirst = true;
            if (instanceParameter != null)
            {
                ctx.Append("this.instance");
                isFirst = false;
            }
            foreach (MethodParameterInfo parameter in methodParameters)
            {
                if (!isFirst) ctx.Append(", ");

                void renderParameter() => ctx.Append(parameter.Name);
                if (ctx.SymbolMap.IsConversionRequiringClassOrDelegate(parameter.Type) || RequiresCharConversion(parameter.Type))
                {
                    RenderInlineHandleExtraction(parameter.Type, renderParameter);
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
            foreach (PropertyInfo propertyInfo in ctx.Class.Constructor?.MemberInitializers ?? throw new InvalidOperationException($"Can not render initializer parameter for class {ctx.Class.Name} with no constructor"))
            {
                bool requiresProxyConversion = ctx.SymbolMap.IsConversionRequiringClassOrDelegate(propertyInfo.Type);
                bool requiresCharConversion = RequiresCharConversion(propertyInfo.Type);
                if (!requiresCharConversion && !requiresProxyConversion)
                {
                    continue;
                }

                void renderPropertyAccessorExpression() => ctx.Append(initializerObject.Name).Append('.').Append(propertyInfo.Name);
                ctx.Append(", ").Append(propertyInfo.Name).Append(": ");
                RenderInlineHandleExtraction(propertyInfo.Type, renderPropertyAccessorExpression);
            }
            ctx.Append(" }");
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
}