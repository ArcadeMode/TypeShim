using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Typescript;

internal class TypeScriptSymbolNameRenderer(InteropTypeInfo typeInfo, RenderContext ctx)
{
    public static string Render(InteropTypeInfo typeInfo, RenderContext ctx)
    {
        TypeScriptSymbolNameRenderer renderer = new(typeInfo, ctx);
        return renderer.RenderCore(TypeShimSymbolType.Proxy, interop: false);
    }

    public static string Render(InteropTypeInfo typeInfo, RenderContext ctx, TypeShimSymbolType symbolType, bool interop)
    {
        TypeScriptSymbolNameRenderer renderer = new(typeInfo, ctx);
        return renderer.RenderCore(symbolType, interop);
    }

    public string Render() => RenderCore(TypeShimSymbolType.Proxy, interop: false);

    private string RenderCore(TypeShimSymbolType symbolType, bool interop)
    {
        TypeScriptSymbolNameTemplate symbolNameTemplate = GetSymbolNameTemplate(interop);
        string template = symbolNameTemplate.Template;
        foreach (KeyValuePair<string, InteropTypeInfo> kvp in symbolNameTemplate.InnerTypes)
        {
            TypeScriptSymbolNameRenderer innerRenderer = new(kvp.Value, ctx);
            TypeScriptSymbolNameTemplate targetTemplate = interop ? kvp.Value.TypeScriptInteropTypeSyntax : kvp.Value.TypeScriptTypeSyntax;
            template = template.Replace(kvp.Key, innerRenderer.RenderCore(symbolType, interop));
        }

        return template.Replace(TypeScriptSymbolNameTemplate.SuffixPlaceholder, ResolveSuffix(symbolType, interop));
    }

    private TypeScriptSymbolNameTemplate GetSymbolNameTemplate(bool interop) => interop ? typeInfo.TypeScriptInteropTypeSyntax : typeInfo.TypeScriptTypeSyntax;

    private string ResolveSuffix(TypeShimSymbolType symbolType, bool interop)
    {
        if (typeInfo.GetInnermostType() is not { IsTSExport: true } innerMostTSExport)
        {
            return string.Empty;
        }


        return (symbolType) switch
        {
            TypeShimSymbolType.Proxy => string.Empty,
            TypeShimSymbolType.Namespace => string.Empty,
            TypeShimSymbolType.Snapshot => $".{RenderConstants.Properties}",
            TypeShimSymbolType.Initializer => $".{RenderConstants.Initializer}",
            TypeShimSymbolType.ProxyInitializerUnion => GetProxyInitializerSuffix(interop, innerMostTSExport),
            _ => throw new NotImplementedException(),
        };

        string GetProxyInitializerSuffix(bool interop, InteropTypeInfo innerMostTSExport)
        {
            if (ctx.SymbolMap.GetClassInfo(innerMostTSExport) is not { Constructor: { AcceptsInitializer: true, IsParameterless: true } })
            {
                return ResolveSuffix(TypeShimSymbolType.Proxy, interop); // initializer not supported, fall back to proxy only.
            }

            return interop ? " | object" : $" | {Render(innerMostTSExport, ctx, TypeShimSymbolType.Initializer, interop)}";
        }
    }
}
