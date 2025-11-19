using System;
using System.Collections.Generic;
using System.Text;

namespace DotnetWasmTypescript.InteropGenerator;

internal class TypescriptInteropInterfaceRenderer(ClassInfo classInfo)
{
    //TODO: render mirror TypeScript interop class

    // PURPOSE:
    // - provide strongly typed interop interface for a single class instance
    // - recognizable methods matching the original C# class for end user
}

internal class TypescriptModuleInterfaceRenderer(IEnumerable<ClassInfo> classInfos)
{
    //TODO: render interface with all exported interop methods

    // PURPOSE:
    // - provide strongly typed interface of the WebAssembly module's exports
    
    // KEY POINTS:
    // - defines the entire set of exports available from the WebAssembly module
    // - considers namespaces correctly
}

internal class TypescriptModuleImplementationRenderer(IEnumerable<ClassInfo> classInfos)
{
    //TODO: render class that implements all exported interop methods

    // PURPOSE:
    // - first unit (that user calls) to tie together the object references + interop interface + class proxies

    // KEY POINTS:
    // - first line of interop
    // - hidden from user behind interface with module exports
}

internal class TypescriptInterfaceRenderer(ClassInfo classInfo)
{
    //TODO: render the original class's TypeScript interface

    // PURPOSE:
    // - provide strongly typed interface matching the original C# class
    // - hide interop details from the end user >> hide (xxxProxy) behind this interface
}

internal class TypescriptProxyClassInfoRenderer(ClassInfo classInfo)
{
    //TODO: render proxy class implementing the interface rendered by TypescriptInterfaceRenderer

    // PURPOSE:
    // - glue between interop interface for a single class instance, enabling dynamic method invocation

    // CONSTRUCTOR:
    // - constructor takes ref to managedObject (js runtime) to pass as instance parameter to interop calls
    // --- IF the original class has non-static methods
    // - contructor takes ref to exports interface by TypescriptWasmExportsInterfaceClassInfoRenderer
}

