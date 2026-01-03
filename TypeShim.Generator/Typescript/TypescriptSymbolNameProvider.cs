using TypeShim.Shared;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal enum SymbolNameFlags
{
    None = 0,
    Proxy = 1,
    Properties = 2,
    Initializer = 4,
    ProxyInitializerUnion = Proxy | Initializer,
}

internal class TypescriptSymbolNameProvider(TypeScriptTypeMapper typeMapper)
{
    internal string GetUserClassSymbolName(ClassInfo classInfo, SymbolNameFlags flags)
    {
        return GetUserClassSymbolNameCore(classInfo.Type, classInfo.Type, flags);
    }

    internal string GetUserClassSymbolName(ClassInfo classInfo, InteropTypeInfo useSiteTypeInfo, SymbolNameFlags flags)
    {
        return GetUserClassSymbolNameCore(useSiteTypeInfo, classInfo.Type, flags);
    }

    private string GetUserClassSymbolNameCore(InteropTypeInfo useSiteTypeInfo, InteropTypeInfo userTypeInfo, SymbolNameFlags flags)
    {
        return (flags) switch
        {
            SymbolNameFlags.Proxy => useSiteTypeInfo.TypeScriptTypeSyntax.Render(),
            SymbolNameFlags.Properties => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Properties}"),
            SymbolNameFlags.Initializer => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Initializer}"),
            SymbolNameFlags.ProxyInitializerUnion => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $" | {userTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Initializer}")}"),
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// returns the TypeScript type name without any suffixes. Mostly useful for non-user types like string, int with accomodations for Tasks, arrays etc.
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <returns></returns>
    internal string GetNakedSymbolReference(InteropTypeInfo typeInfo) => GetSymbolNameCore(typeInfo, string.Empty);
    internal string GetUserClassSymbolName(ClassInfo classInfo) => GetSymbolNameCore(classInfo.Type, string.Empty);
    private string GetSymbolNameCore(InteropTypeInfo typeInfo, string suffix) => typeInfo.TypeScriptTypeSyntax.Render(suffix);
}
