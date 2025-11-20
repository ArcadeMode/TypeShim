namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class TypescriptUserClassProxyRenderer(ClassInfo classInfo)
{
    //TODO: render proxy class implementing the interface rendered by TypescriptUserClassInterfaceRenderer

    // PURPOSE:
    // - glue between interop interface for a single class instance, enabling dynamic method invocation

    // CONSTRUCTOR:
    // - constructor takes ref to managedObject (js runtime) to pass as instance parameter to interop calls
    // --- IF the original class has non-static methods
    // - contructor takes ref to exports interface by TypescriptWasmExportsInterfaceClassInfoRenderer
}
