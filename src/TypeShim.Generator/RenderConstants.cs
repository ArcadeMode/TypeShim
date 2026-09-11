using TypeShim.Generator.Parsing;

namespace TypeShim.Generator;

internal static class RenderConstants
{
    internal const string FromJSObject = "FromJSObject";
    internal const string FromObject = "FromObject";

    internal const string AssemblyExports = "AssemblyExports";
    internal const string Proxy = "Proxy";
    internal const string Properties = "Snapshot";
    internal const string Initializer = "Initializer";

    internal const string ProxyMaterializeFunction = "materialize";
    
    internal const string ManagedObject = "ManagedObject";

    internal const string MarshallPropertyAsClass = "MarshallPropertyAs";

    internal static string InteropClassName(ClassInfo classInfo) => classInfo.IsTSExport ? $"{classInfo.Name}Interop" : classInfo.Name;

    /// <summary>
    /// Fully-qualified (<c>global::</c>-prefixed) name of the generated interop class, so that
    /// references from generated C# in another namespace resolve regardless of using directives.
    /// Handles the global (empty) namespace case.
    /// </summary>
    internal static string FullyQualifiedInteropClassName(ClassInfo classInfo)
    {
        string interopClassName = InteropClassName(classInfo);
        return string.IsNullOrEmpty(classInfo.Namespace)
            ? $"global::{interopClassName}"
            : $"global::{classInfo.Namespace}.{interopClassName}";
    }

    internal static string UnsafeAccessorSetMethod(PropertyInfo propertyInfo) => $"Set{propertyInfo.Name}";

    internal const string UnsafeAccessorConstructorMethod = "CreateInstance";

}
