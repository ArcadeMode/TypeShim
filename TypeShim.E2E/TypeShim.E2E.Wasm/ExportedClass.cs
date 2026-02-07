using System;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class ExportedClass // for referencing an exported class
{
    public int Id { get; set; }
}
