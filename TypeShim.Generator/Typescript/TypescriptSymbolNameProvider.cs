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

    /// <summary>
    /// returns the TypeScript type name without any proxy or snapshot suffixes. Mostly useful for non-user types like string, int with accomodations for Tasks, arrays etc.
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <returns></returns>
    internal string GetNakedSymbolReference(InteropTypeInfo typeInfo) => typeMapper.ToTypeScriptType(typeInfo).Render(suffix: string.Empty);



}

enum SymbolReferenceType
{
    None,
    Proxy,
    Snapshot
}
