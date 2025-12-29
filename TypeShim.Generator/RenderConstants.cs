using TypeShim.Generator.Parsing;

namespace TypeShim.Generator;

internal static class RenderConstants
{
    internal const string FromJSObjectMethodName = "FromJSObject";
    internal const string FromObjectMethodName = "FromObject";

    internal const string AssemblyExports = "AssemblyExports";
    internal const string Proxy = "Proxy";
    internal const string Snapshot = "Snapshot";

    internal static string InteropClassName(ClassInfo classInfo) => $"{classInfo.Name}Interop";

}
