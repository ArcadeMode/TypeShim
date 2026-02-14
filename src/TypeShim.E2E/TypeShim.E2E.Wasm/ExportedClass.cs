using System;
using System.Runtime.InteropServices.JavaScript;

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


public partial class ManualExport
{
    [JSExport]
    [return: JSMarshalAs<JSType.MemoryView>]
    public static Span<int> GetSpan()
    {
        return new Span<int>(new int[] { 1, 2, 3 });
    }
}