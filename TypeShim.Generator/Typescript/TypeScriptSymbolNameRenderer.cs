using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal class TypeScriptSymbolNameRenderer(TypeShimSymbolType returnSymbolType, TypeShimSymbolType parameterSymbolType, bool interop, RenderContext ctx)
{
    public static void Render(InteropTypeInfo typeInfo, RenderContext ctx)
    {
        TypeScriptSymbolNameRenderer renderer = new(TypeShimSymbolType.None, TypeShimSymbolType.None, interop: false, ctx);
        renderer.RenderCore(typeInfo);
    }

    public static void Render(InteropTypeInfo typeInfo, RenderContext ctx, TypeShimSymbolType symbolType, bool interop)
    {
        TypeScriptSymbolNameRenderer renderer = new(symbolType, symbolType, interop, ctx);
        renderer.RenderCore(typeInfo);
    }
    
    public static void Render(InteropTypeInfo typeInfo, RenderContext ctx, TypeShimSymbolType returnSymbolType, TypeShimSymbolType parameterSymbolType, bool interop)
    {
        TypeScriptSymbolNameRenderer renderer = new(returnSymbolType, parameterSymbolType, interop, ctx);
        renderer.RenderCore(typeInfo);
    }
    
    private void RenderCore(InteropTypeInfo typeInfo, bool isDelegateParameter = false)
    {
        if (typeInfo.IsDelegateType())
        {
            RenderDelegateCore(typeInfo.ArgumentInfo!);
        } 
        else if (typeInfo.IsNullableType)
        {
            RenderNullableCore(typeInfo);
        }
        else if (typeInfo.IsArrayType)
        {
            RenderArrayCore(typeInfo);
        }
        else if (typeInfo.IsTaskType)
        {
            RenderPromiseCore(typeInfo);
        }
        else
        {
            ctx.Append(GetSymbolNameTemplate(typeInfo).Template);
            if (typeInfo.IsTSExport)
            {
                RenderSuffix(typeInfo, isDelegateParameter ? parameterSymbolType : returnSymbolType);
            }
        }
    }

    private void RenderNullableCore(InteropTypeInfo typeInfo)
    {
        bool isNullableDelegate = typeInfo.TypeArgument?.IsDelegateType() == true;
        if (isNullableDelegate) ctx.Append("(");

        RenderCore(typeInfo.TypeArgument!);

        if (isNullableDelegate) ctx.Append(")");

        ctx.Append(" | null");
    }

    private void RenderArrayCore(InteropTypeInfo typeInfo)
    {
        ctx.Append("Array<");
        RenderCore(typeInfo.TypeArgument!);
        ctx.Append(">");
    }
    
    private void RenderPromiseCore(InteropTypeInfo typeInfo)
    {
        if (typeInfo.TypeArgument == null)
        {
            ctx.Append("Promise<void>");
        }
        else
        {
            ctx.Append("Promise<");
            RenderCore(typeInfo.TypeArgument!);
            ctx.Append(">");
        }
    }

    private void RenderDelegateCore(DelegateArgumentInfo delegateInfo)
    {
        ctx.Append("(");
        foreach (var param in delegateInfo.ParameterTypes.Select((t, i) => new { Type = t, Index = i }))
        {
            if (param.Index > 0) ctx.Append(", ");
            ctx.Append("arg").Append(param.Index).Append(": ");
            RenderCore(param.Type, isDelegateParameter: true);
        }
        ctx.Append(") => ");
        RenderCore(delegateInfo.ReturnType);
    }

    private TypeScriptSymbolNameTemplate GetSymbolNameTemplate(InteropTypeInfo typeInfo) 
        => interop ? typeInfo.TypeScriptInteropTypeSyntax : typeInfo.TypeScriptTypeSyntax;

    private void RenderSuffix(InteropTypeInfo typeInfo, TypeShimSymbolType symbolType)
    {
        if (typeInfo.GetInnermostType() is not { IsTSExport: true } innerMostTSExport 
            || symbolType is TypeShimSymbolType.Proxy or TypeShimSymbolType.Namespace or TypeShimSymbolType.None)
        {
            return;
        }

        if (symbolType is TypeShimSymbolType.Snapshot)
        {
            ctx.Append('.').Append(RenderConstants.Properties);
            return;
        }
        
        if (symbolType is TypeShimSymbolType.Initializer)
        {
            ctx.Append('.').Append(RenderConstants.Initializer);
            return;
        }

        if (symbolType is TypeShimSymbolType.ProxyInitializerUnion)
        {
            RenderProxyInitializerSuffix(interop, innerMostTSExport);
            return;
        }

        throw new NotImplementedException($"Unhandled type/symboltype combination for {typeInfo.CSharpTypeSyntax} {symbolType}");

        void RenderProxyInitializerSuffix(bool interop, InteropTypeInfo innerMostTSExport)
        {
            if (ctx.SymbolMap.GetClassInfo(innerMostTSExport) is not { Constructor: { AcceptsInitializer: true, IsParameterless: true } })
            {
                RenderSuffix(typeInfo, TypeShimSymbolType.Proxy); // initializer not supported, fall back to proxy only.
            }
            else if (interop)
            {
                ctx.Append(" | object");
            }
            else
            {
                ctx.Append(" | ");
                TypeScriptSymbolNameRenderer innerRenderer = new(TypeShimSymbolType.Initializer, TypeShimSymbolType.Initializer, interop, ctx);
                innerRenderer.RenderCore(innerMostTSExport);
            }
        }
    }

}
