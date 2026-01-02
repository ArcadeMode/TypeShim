using System.Collections.Generic;

namespace TypeShim.Sample.Capabilities;

[TSExport]
public class ArraysDemo(int[] initialArray)
{
    public int[] IntArrayProperty { get; private set; } = initialArray;
    
    public int SumElements()
    {
        int sum = 0;
        foreach (int item in IntArrayProperty)
        {
            sum += item;
        }
        return sum;
    }

    public void Append(int value)
    {
        IntArrayProperty = [.. IntArrayProperty, value];
    }
}