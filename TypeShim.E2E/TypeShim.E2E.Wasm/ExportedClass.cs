using System;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class ExportedClass : IDisposable
{
    public int Id { get; set; }

    public void Dispose()
    {
        // no-op for testing purposes
    }
}
