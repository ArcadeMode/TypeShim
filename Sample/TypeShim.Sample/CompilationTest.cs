using System;
using System.Runtime.InteropServices.JavaScript;
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
    public DateTime DateTimeProperty { get; set; }
    public DateTimeOffset DateTimeOffsetProperty { get; set; }
    public object objectProperty { get; set; } = new object();
    public ExportedClass ExportedClassProperty { get; set; } = new ExportedClass();
    public JSObject JSObjectProperty { get; set; } = null!;
    public Task TaskProperty { get; set; } = Task.CompletedTask;
    public Task<nint> TaskOfNIntProperty { get; set; } = Task.FromResult((nint)0);
    public Task<short> TaskOfShortProperty { get; set; } = Task.FromResult((short)0);
    public Task<int> TaskOfIntProperty { get; set; } = Task.FromResult(0);
    public Task<long> TaskOfLongProperty { get; set; } = Task.FromResult(0L);
    public Task<bool> TaskOfBoolProperty { get; set; } = Task.FromResult(false);
    public Task<byte> TaskOfByteProperty { get; set; } = Task.FromResult((byte)0);
    public Task<char> TaskOfCharProperty { get; set; } = Task.FromResult('\0');
    public Task<string> TaskOfStringProperty { get; set; } = Task.FromResult(string.Empty);
    public Task<double> TaskOfDoubleProperty { get; set; } = Task.FromResult(0.0);
    public Task<float> TaskOfFloatProperty { get; set; } = Task.FromResult(0.0f);
    public Task<DateTime> TaskOfDateTimeProperty { get; set; } = Task.FromResult(DateTime.Now);
    public Task<DateTimeOffset> TaskOfDateTimeOffsetProperty { get; set; } = Task.FromResult(DateTimeOffset.Now);
    public Task<object> TaskOfObjectProperty { get; set; } = Task.FromResult(new object());
    public Task<ExportedClass> TaskOfExportedClassProperty { get; set; } = Task.FromResult(new ExportedClass());
    public Task<JSObject> TaskOfJSObjectProperty { get; set; } = Task.FromException<JSObject>(new Exception("biem"));
    //public Task<Exception> TaskOfExceptionProperty { get; set; } = Task.FromResult<Exception>(new Exception("biem"));
    public byte[] ByteArrayProperty { get; set; } = [];
    public JSObject[] JSObjectArrayProperty { get; set; } = [];
    public object[] ObjectArrayProperty { get; set; } = [];
    public ExportedClass[] ExportedClassArrayProperty { get; set; } = [];
    public int[] IntArrayProperty { get; set; } = [];
    public string[] StringArrayProperty { get; set; } = [];
    public double[] DoubleArrayProperty { get; set; } = [];

    public void VoidMethod() { }
    public int IntMethod() => 42;
    public bool BoolMethod() => true;
    public string StringMethod() => "Hello, TypeShim!";
    public double DoubleMethod() => 3.14;
}

[TSExport]
public class ExportedClass
{
    public int Id { get; set; }
}
