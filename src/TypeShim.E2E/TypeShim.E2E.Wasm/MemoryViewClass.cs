using System;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class MemoryViewMethodClass
{
    public Span<byte> GetByteSpan() => new Span<byte>([1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4]);
    public Span<int> GetInt32Span() => new Span<int>([0, 1, 2, 3, 4]);
    public Span<float> GetFloatSpan() => new Span<float>([0.5f, 1.5f, 2.5f, 3.5f, 4.5f]);
    public Span<double> GetDoubleSpan() => new Span<double>([0.1, 1.1, 2.1, 3.1, 4.1]);

    public ArraySegment<byte> GetByteArraySegment() => new ArraySegment<byte>([1 << 0, 1 << 1, 1 << 2, 1 << 3, 1 << 4]);
    public ArraySegment<int> GetInt32ArraySegment() => new ArraySegment<int>([0, 1, 2, 3, 4]);
    public ArraySegment<float> GetFloatArraySegment() => new ArraySegment<float>([0.5f, 1.5f, 2.5f, 3.5f, 4.5f]);
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

    public float SumFloatSpan(Span<float> span)
    {
        float sum = 0;
        foreach (float f in span)
        {
            sum += f;
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

    public float SumFloatArraySegment(ArraySegment<float> segment)
    {
        float sum = 0;
        foreach (float f in segment)
        {
            sum += f;
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

[TSExport]
public class MemoryViewPropertyClass
{ 
    public required ArraySegment<byte> ByteArraySegment { get; set; }
    public required ArraySegment<int> Int32ArraySegment { get; set; }
    public required ArraySegment<float> FloatArraySegment { get; set; }
    public required ArraySegment<double> DoubleArraySegment { get; set; }

    // span properties are not allowed in classes.
}

