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
    /// <summary>
    /// Wether to strip the surrounding type information down to just the symbol name itself, without nullability, Task, array etc.
    /// </summary>
    Isolated = 16,
}

internal class TypescriptSymbolNameProvider(TypeScriptTypeMapper typeMapper)
{
    internal string GetUserClassSymbolName(InteropTypeInfo typeInfo, ClassInfo classInfo, SymbolNameFlags flags)
    {
        return GetUserClassSymbolName(typeInfo, classInfo.Type, flags);
    }

    internal string? GetUserClassSymbolNameIfExists(InteropTypeInfo typeInfo, SymbolNameFlags flags)
    {
        if (ExtractUserType(typeInfo) is not InteropTypeInfo userTypeInfo)
            return null;

        return GetUserClassSymbolName(typeInfo, userTypeInfo, flags);
    }

    internal string GetUserClassSymbolName(InteropTypeInfo typeInfo, InteropTypeInfo userTypeInfo, SymbolNameFlags flags)
    {
        InteropTypeInfo targetType = flags.HasFlag(SymbolNameFlags.Isolated) ? userTypeInfo : typeInfo;
        return (flags & ~SymbolNameFlags.Isolated) switch
        {
            SymbolNameFlags.Proxy => $"{typeMapper.ToTypeScriptType(targetType).Render(suffix: $".{RenderConstants.Proxy}")}",
            SymbolNameFlags.Properties => $"{typeMapper.ToTypeScriptType(targetType).Render(suffix: $".{RenderConstants.Properties}")}",
            SymbolNameFlags.Initializer => $"{typeMapper.ToTypeScriptType(targetType).Render(suffix: $".{RenderConstants.Initializer}")}",
            SymbolNameFlags.ProxyInitializerUnion => $"{typeMapper.ToTypeScriptType(targetType).Render(suffix: $".{RenderConstants.Proxy} | {typeMapper.ToTypeScriptType(userTypeInfo).Render(suffix: $".{RenderConstants.Initializer}")}")}",
            _ => throw new NotImplementedException(),
        };
    }

    internal string GetUserClassSymbolName(ClassInfo classInfo, string typeSuffix) => $"{typeMapper.ToTypeScriptType(classInfo.Type).Render(suffix: $".{typeSuffix}")}";

    /// <summary>
    /// returns the TypeScript type name without any suffixes. Mostly useful for non-user types like string, int with accomodations for Tasks, arrays etc.
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <returns></returns>
    internal string GetNakedSymbolReference(InteropTypeInfo typeInfo) => typeMapper.ToTypeScriptType(typeInfo).Render(suffix: string.Empty);

    private InteropTypeInfo? ExtractUserType(InteropTypeInfo typeInfo)
    {
        if (typeMapper.IsUserType(typeInfo))
            return typeInfo;
        if (typeInfo.TypeArgument is InteropTypeInfo argTypeInfo)
            return ExtractUserType(argTypeInfo);
        return null;
    }
}
