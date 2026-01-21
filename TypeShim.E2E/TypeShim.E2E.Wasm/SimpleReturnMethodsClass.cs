using System;

namespace TypeShim.Sample;

[TSExport]
public class SimpleReturnMethodsClass
{
    public void VoidMethod() { }
    public int IntMethod() => 42;
    public bool BoolMethod() => true;
    public string StringMethod() => "Hello, from .NET";
    public double DoubleMethod() => 3.14159;
    public DateTime DateTimeMethod() => new DateTime(1995, 4, 1);
    public DateTime DateTimeNowDateMethod() => DateTime.Now.Date;
    public DateTimeOffset DateTimeOffsetMethod() => new DateTimeOffset(DateTimeMethod(), TimeSpan.FromHours(1));
    public ExportedClass ExportedClassMethod() => new() { Id = 420 };
}