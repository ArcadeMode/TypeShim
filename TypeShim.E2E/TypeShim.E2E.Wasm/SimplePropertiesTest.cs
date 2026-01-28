using System;
using System.Runtime.InteropServices.JavaScript;

namespace TypeShim.Sample;

[TSExport]
public class SimplePropertiesTest
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
    public DateTime DateTimeProperty { get; set; }
    public DateTimeOffset DateTimeOffsetProperty { get; set; }
    public object ObjectProperty { get; set; } = new object();
    public ExportedClass ExportedClassProperty { get; set; } = new ExportedClass();
    public JSObject JSObjectProperty { get; set; } = null!;
}
