using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

internal class TypescriptClassNameBuilder(TypeScriptTypeMapper typeMapper)
{
    internal string GetInteropInterfaceName(ClassInfo classInfo) => $"{classInfo.Name}Interop";
    internal string GetUserClassProxyName(ClassInfo classInfo) => $"{classInfo.Name}Proxy";
    internal string? GetUserClassProxyName(InteropTypeInfo typeInfo)
    {
        //TypeSyntax innerTypeSyntax = typeMapper.GetTypeFromNullableSyntax(typeMapper.ExtractInnerTypeArgument(typeInfo.CLRTypeSyntax) // extract T from Task<T>/T[] etc.
        //, out _);

        InteropTypeInfo targetType = typeInfo.TypeArgument ?? typeInfo;
        return typeMapper.IsUserType(targetType)
        ? $"{typeMapper.ToTypeScriptType(targetType)}Proxy"
        : null;
    }
    internal string GetUserClassStaticsName(ClassInfo classInfo) => $"{classInfo.Name}Statics";
    internal string GetModuleClassName() => "WasmModule";
    internal string GetModuleInteropClassName() => $"{GetModuleClassName()}Exports";
}
