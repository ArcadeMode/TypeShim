namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class TypescriptClassNameBuilder
{
    internal static string GetInteropInterfaceName(ClassInfo classInfo) => $"{classInfo.Name}Interop";
    internal static string GetProxyClassName(ClassInfo classInfo) => $"{classInfo.Name}Proxy";
}
