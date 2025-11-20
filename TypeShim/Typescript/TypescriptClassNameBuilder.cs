namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class TypescriptClassNameBuilder(TypeScriptTypeMapper typeMapper)
{
    internal string GetInteropInterfaceName(ClassInfo classInfo) => $"{classInfo.Name}Interop";
    internal string GetUserClassProxyName(ClassInfo classInfo) => $"{classInfo.Name}Proxy";
    internal string? GetUserClassProxyNameForReturnType(MethodInfo methodInfo) => typeMapper.HasKnownType(methodInfo.ReturnCLRTypeSyntax.ToString())
        ? $"{typeMapper.ToTypeScriptType(methodInfo.ReturnKnownType, methodInfo.ReturnCLRTypeSyntax.ToString())}Proxy"
        : null;
    internal string GetUserClassStaticsName(ClassInfo classInfo) => $"{classInfo.Name}Statics";
    internal string GetModuleClassName() => "WasmModule";
    internal string GetModuleInteropClassName() => $"{GetModuleClassName()}Exports";
}
