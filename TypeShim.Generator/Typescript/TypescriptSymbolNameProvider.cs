using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypescriptSymbolNameProvider(TypeScriptTypeMapper typeMapper)
{
    internal string GetModuleInteropClassName() => $"AssemblyExports";
    internal string GetInteropInterfaceName(ClassInfo classInfo) => $"{classInfo.Name}Interop";

    internal string GetUserClassNamespace(ClassInfo classInfo)
    {
        return classInfo.Name;
    }

    internal string GetProxyDefinitionName() => $"Proxy";
    internal string GetProxyReferenceName(ClassInfo classInfo) => RenderUserClassProxyReferenceName(classInfo.Type);
    internal string? GetProxyReferenceNameIfExists(InteropTypeInfo typeInfo)
    {
        return typeMapper.IsUserType(typeInfo) || (typeInfo.TypeArgument is InteropTypeInfo argTypeInfo && typeMapper.IsUserType(argTypeInfo))
            ? RenderUserClassProxyReferenceName(typeInfo)
            : null;
    }
    private string RenderUserClassProxyReferenceName(InteropTypeInfo typeInfo) => $"{typeMapper.ToTypeScriptType(typeInfo).Render(suffix: $".{GetProxyDefinitionName()}")}";

    internal string GetSnapshotDefinitionName() => $"Snapshot";
    internal string GetSnapshotReferenceName(ClassInfo classInfo) => RenderUserClassSnapshotReferenceName(classInfo.Type);
    internal string? GetSnapshotReferenceNameIfExists(InteropTypeInfo typeInfo)
    {
        return typeMapper.IsUserType(typeInfo) || (typeInfo.TypeArgument is InteropTypeInfo argTypeInfo && typeMapper.IsUserType(argTypeInfo))
            ? RenderUserClassSnapshotReferenceName(typeInfo)
            : null;
    }

    internal string? GetProxySnapshotUnionIfExists(InteropTypeInfo typeInfo)
    {
        if (typeMapper.IsUserType(typeInfo))
        {
            return $"{RenderUserClassProxyReferenceName(typeInfo)} | {RenderUserClassSnapshotReferenceName(typeInfo)}"; // T.Proxy | T.Snapshot
        }

        if (typeInfo.TypeArgument is InteropTypeInfo argTypeInfo && typeMapper.IsUserType(argTypeInfo))
        {
            return $"{typeMapper.ToTypeScriptType(typeInfo).Render(suffix: $".{GetProxyDefinitionName()} | {RenderUserClassSnapshotReferenceName(argTypeInfo)}")}"; // Array<T.Proxy | T.Snapshot>, Promise, null
        }

        return null;
    }

    private string RenderUserClassSnapshotReferenceName(InteropTypeInfo typeInfo) => $"{typeMapper.ToTypeScriptType(typeInfo).Render(suffix: $".{GetSnapshotDefinitionName()}")}";

    /// <summary>
    /// returns the TypeScript type name without any proxy or snapshot suffixes. Mostly useful for non-user types like string, int with accomodations for Tasks, arrays etc.
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <returns></returns>
    internal string GetNakedSymbolReference(InteropTypeInfo typeInfo) => typeMapper.ToTypeScriptType(typeInfo).Render(suffix: string.Empty);

}
