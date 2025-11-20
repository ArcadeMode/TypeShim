using System;
using System.Collections.Generic;

namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class TypescriptModuleImplementationRenderer(IEnumerable<ClassInfo> classInfos)
{
    //TODO: render class that implements all exported interop methods

    // PURPOSE:
    // - first unit (that user calls) to tie together the object references + interop interface + class proxies

    // KEY POINTS:
    // - first line of interop
    // - hidden from user behind interface with module exports
}
