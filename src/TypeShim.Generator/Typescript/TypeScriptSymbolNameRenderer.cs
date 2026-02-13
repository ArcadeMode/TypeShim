using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        else if (typeInfo.ManagedType is KnownManagedType.Span) 
        {
            ctx.Append("Span<");
            ctx.Append(TypeScriptSymbolNameResolver.ResolveMemoryViewTypeArgSymbol(typeInfo));
            ctx.Append(">");
        }
        else if (typeInfo.ManagedType is KnownManagedType.ArraySegment) 
        {
            ctx.Append("ArraySegment<");
            ctx.Append(TypeScriptSymbolNameResolver.ResolveMemoryViewTypeArgSymbol(typeInfo));
            ctx.Append(">");
        }
        else
        {
            ctx.Append(GetSymbolName(typeInfo));
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

    private string GetSymbolName(InteropTypeInfo typeInfo) 
        => interop ? TypeScriptSymbolNameResolver.ResolveSimpleInteropTypeSymbol(typeInfo) : TypeScriptSymbolNameResolver.ResolveSimpleTypeSymbol(typeInfo);

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

internal static class TypeScriptSymbolNameResolver
{
    internal static string ResolveSimpleInteropTypeSymbol(InteropTypeInfo typeInfo)
    {
        return typeInfo.ManagedType switch
        {
            KnownManagedType.Object // objects are represented differently on the interop boundary
                => "ManagedObject",
            KnownManagedType.Char // chars are represented as numbers on the interop boundary (is intended: https://github.com/dotnet/runtime/issues/123187)
                => "number",
            _ => ResolveSimpleTypeSymbol(typeInfo)
        };
    }

    internal static string ResolveSimpleTypeSymbol(InteropTypeInfo typeInfo)
    {
        return typeInfo.ManagedType switch
        {
            KnownManagedType.Object when typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion
                => typeInfo.CSharpTypeSyntax.ToString(),
            KnownManagedType.Object when typeInfo.RequiresTypeConversion && !typeInfo.SupportsTypeConversion
                => "ManagedObject",
            KnownManagedType.Object when !typeInfo.RequiresTypeConversion
                => "ManagedObject",

            KnownManagedType.None => "undefined",
            KnownManagedType.Void => "void",
            KnownManagedType.JSObject
                => "object",

            KnownManagedType.Boolean => "boolean",
            KnownManagedType.Char
            or KnownManagedType.String => "string",
            KnownManagedType.Byte
            or KnownManagedType.Int16
            or KnownManagedType.Int32
            or KnownManagedType.Int64
            or KnownManagedType.Double
            or KnownManagedType.Single
            or KnownManagedType.IntPtr
                => "number",
            KnownManagedType.DateTime
            or KnownManagedType.DateTimeOffset => "Date",
            KnownManagedType.Exception => "Error",

            KnownManagedType.Unknown
            or _ => "any",
        };
    }

    internal static string ResolveMemoryViewTypeArgSymbol(InteropTypeInfo typeInfo)
    {
        if (typeInfo.ManagedType is not KnownManagedType.Span and not KnownManagedType.ArraySegment)
        {
            throw new InvalidOperationException($"Type '{typeInfo.ManagedType}' is not a valid MemoryView type.");
        }

        return typeInfo.TypeArgument switch
        {
            { ManagedType: KnownManagedType.Byte } => "Uint8Array",
            { ManagedType: KnownManagedType.Int32 } => "Int32Array",
            { ManagedType: KnownManagedType.Double } => "Float64Array",
            _ => throw new InvalidOperationException($"Type argument '{typeInfo.TypeArgument?.ManagedType}' is not valid for MemoryView types.")
        };
    }
}