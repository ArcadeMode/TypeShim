using System;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class MemoryViewClass
{
    public Span<int> GetInt32Span() => new Span<int>(new[] { 0, 1, 2, 3, 4 }); 
}
