using TypeShim.Shared;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal enum TypeShimSymbolType
{
    None,
    Proxy,
    Namespace,
    Snapshot,
    Initializer,
    ProxyInitializerUnion,
}

internal class TypescriptSymbolNameProvider(TypeScriptTypeMapper typeMapper)
{
    internal string GetUserClassSymbolName(ClassInfo classInfo, TypeShimSymbolType flags)
    {
        return GetUserClassSymbolNameCore(classInfo.Type, classInfo.Type, flags);
    }

    internal string GetUserClassSymbolName(ClassInfo classInfo, InteropTypeInfo useSiteTypeInfo, TypeShimSymbolType flags)
    {
        return GetUserClassSymbolNameCore(useSiteTypeInfo, classInfo.Type, flags);
    }

    private static string GetUserClassSymbolNameCore(InteropTypeInfo useSiteTypeInfo, InteropTypeInfo userTypeInfo, TypeShimSymbolType flags)
    {
        return (flags) switch
        {
            TypeShimSymbolType.Proxy => useSiteTypeInfo.TypeScriptTypeSyntax.Render(),
            TypeShimSymbolType.Namespace => useSiteTypeInfo.TypeScriptTypeSyntax.Render(),
            TypeShimSymbolType.Snapshot => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Properties}"),
            TypeShimSymbolType.Initializer => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Initializer}"),
            TypeShimSymbolType.ProxyInitializerUnion => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $" | {userTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Initializer}")}"),
            _ => throw new NotImplementedException(),
        };
    }
}
