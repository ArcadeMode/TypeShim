using System;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class MemoryViewClass
{
    public Span<byte> GetByteSpan() => new Span<byte>([1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4]);
    public Span<int> GetInt32Span() => new Span<int>([0, 1, 2, 3, 4]);
    public Span<double> GetDoubleSpan() => new Span<double>([0.1, 1.1, 2.1, 3.1, 4.1]);

    public ArraySegment<byte> GetByteArraySegment() => new ArraySegment<byte>([1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4]);
    public ArraySegment<int> GetInt32ArraySegment() => new ArraySegment<int>([0, 1, 2, 3, 4]);
    public ArraySegment<double> GetDoubleArraySegment() => new ArraySegment<double>([0.1, 1.1, 2.1, 3.1, 4.1]);
}
