namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class TypescriptClassNameBuilder
{
    internal string GetInteropInterfaceName(ClassInfo classInfo) => $"{classInfo.Name}Interop";
    internal string GetUserClassProxyName(ClassInfo classInfo) => $"{classInfo.Name}Proxy";
    internal string GetUserClassStaticsName(ClassInfo classInfo) => $"{classInfo.Name}Statics";
    internal string GetModuleClassName() => "WasmModule";
    internal string GetModuleInteropClassName() => $"{GetModuleClassName()}Exports";
}
