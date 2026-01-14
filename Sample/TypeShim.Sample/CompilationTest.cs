using System;
using System.Threading.Tasks;

namespace TypeShim.Sample;

[TSExport]
public class CompilationTest
{
    public nint NIntProperty { get; set; }
    public byte ByteProperty { get; set; }
    public short ShortProperty { get; set; }
    public int IntProperty { get; set; }
    public long LongProperty { get; set; }
    public bool BoolProperty { get; set; }
    public string StringProperty { get; set; } = string.Empty;
    public char CharProperty { get; set; } 
    public double DoubleProperty { get; set; }
    public float FloatProperty { get; set; }
    //public DateTime DateTimeProperty { get; set; }
    
    public Task TaskProperty { get; set; } = Task.CompletedTask;
    public Task<int> TaskOfIntProperty { get; set; } = Task.FromResult(0);
    public Task<bool> TaskOfBoolProperty { get; set; } = Task.FromResult(false);
    public Task<string> TaskOfStringProperty { get; set; } = Task.FromResult(string.Empty);
    public Task<double> TaskOfDoubleProperty { get; set; } = Task.FromResult(0.0);
    public Task<float> TaskOfFloatProperty { get; set; } = Task.FromResult(0.0f);
    public int[] IntArrayProperty { get; set; } = [];
    public string[] StringArrayProperty { get; set; } = [];
    public double[] DoubleArrayProperty { get; set; } = [];

    public void VoidMethod() { }
    public int IntMethod() => 42;
    public bool BoolMethod() => true;
    public string StringMethod() => "Hello, TypeShim!";
    public double DoubleMethod() => 3.14;


}
