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

    public int SumByteSpan(Span<byte> span)
    {
        int sum = 0;
        foreach (byte b in span)
        {
            sum += b;
        }
        return sum;
    }

    public int SumInt32Span(Span<int> span)
    {
        int sum = 0;
        foreach (int i in span)
        {
            sum += i;
        }
        return sum;
    }

    public double SumDoubleSpan(Span<double> span)
    {
        double sum = 0;
        foreach (double d in span)
        {
            sum += d;
        }
        return sum;
    }

    public int SumByteArraySegment(ArraySegment<byte> segment)
    {
        int sum = 0;
        foreach (byte b in segment)
        {
            sum += b;
        }
        return sum;
    }

    public int SumInt32ArraySegment(ArraySegment<int> segment)
    {
        int sum = 0;
        foreach (int i in segment)
        {
            sum += i;
        }
        return sum;
    }

    public double SumDoubleArraySegment(ArraySegment<double> segment)
    {
        double sum = 0;
        foreach (double d in segment)
        {
            sum += d;
        }
        return sum;
    }
}