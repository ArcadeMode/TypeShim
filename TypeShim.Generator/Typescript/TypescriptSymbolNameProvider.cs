using TypeShim.Shared;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal enum SymbolNameFlags
{
    None = 0,
    Proxy = 1,
    Snapshot = 2,
    ProxySnapshotUnion = Proxy | Snapshot,
    /// <summary>
    /// Wether to strip the surrounding type information down to just the symbol name itself, without nullability, Task, array etc.
    /// </summary>
    Isolated = 4,
}

internal class TypescriptSymbolNameProvider(TypeScriptTypeMapper typeMapper)
{
    internal string GetModuleInteropClassName() => $"AssemblyExports";
    internal string GetInteropInterfaceName(ClassInfo classInfo) => $"{classInfo.Name}Interop";

    internal string GetUserClassNamespace(ClassInfo classInfo)
    {
        return classInfo.Name;
    }

    internal string? GetUserClassSymbolNameIfExists(InteropTypeInfo typeInfo, SymbolNameFlags flags)
    {
        if (ExtractUserType(typeInfo) is not InteropTypeInfo userTypeInfo)
            return null;

        InteropTypeInfo targetType = flags.HasFlag(SymbolNameFlags.Isolated) ? userTypeInfo : typeInfo;
        return (flags & ~SymbolNameFlags.Isolated) switch
        {
            SymbolNameFlags.Proxy => GetUserClassProxyReferenceName(targetType),
            SymbolNameFlags.Snapshot => GetUserClassSnapshotReferenceName(targetType),
            SymbolNameFlags.ProxySnapshotUnion => $"{typeMapper.ToTypeScriptType(targetType).Render(suffix: $".{GetUserClassProxySymbolName()} | {GetUserClassSnapshotReferenceName(userTypeInfo)}")}",
            _ => null,
        };
    }

    internal string GetUserClassProxySymbolName() => $"Proxy";
    internal string GetUserClassProxySymbolName(ClassInfo classInfo) => GetUserClassProxyReferenceName(classInfo.Type);
    private string GetUserClassProxyReferenceName(InteropTypeInfo typeInfo) => $"{typeMapper.ToTypeScriptType(typeInfo).Render(suffix: $".{GetUserClassProxySymbolName()}")}";

    internal string GetUserClassSnapshotSymbolName() => $"Snapshot";
    internal string GetUserClassSnapshotSymbolName(ClassInfo classInfo) => GetUserClassSnapshotReferenceName(classInfo.Type);
    private string GetUserClassSnapshotReferenceName(InteropTypeInfo typeInfo) => $"{typeMapper.ToTypeScriptType(typeInfo).Render(suffix: $".{GetUserClassSnapshotSymbolName()}")}";

    /// <summary>
    /// returns the TypeScript type name without any proxy or snapshot suffixes. Mostly useful for non-user types like string, int with accomodations for Tasks, arrays etc.
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
