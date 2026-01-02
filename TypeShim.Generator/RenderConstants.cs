using TypeShim.Generator.Parsing;

namespace TypeShim.Generator;

internal static class RenderConstants
{
    internal const string FromJSObject = "FromJSObject";
    internal const string FromObject = "FromObject";

    internal const string AssemblyExports = "AssemblyExports";
    internal const string Proxy = "Proxy";
    internal const string Properties = "Properties";
    internal const string Initializer = "Initializer";

    internal const string PropertiesTSFunction = "materialize";
    
    internal const string ManagedObject = "ManagedObject";

    internal static string InteropClassName(ClassInfo classInfo) => $"{classInfo.Name}Interop";

}
