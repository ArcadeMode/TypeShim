using System;
using System.Globalization;

namespace TypeShim.Sample;

[TSExport]
public class SimpleReturnMethodsClass
{
    public void VoidMethod() { }
    public int IntMethod() => 42;
    public bool BoolMethod() => true;
    public string StringMethod() => "Hello, from .NET";
    public double DoubleMethod() => 3.14159;
    public DateTime DateTimeMethod() => new(1995, 4, 1);
    public DateTime DateTimeNowDateMethod() => DateTime.Now.Date;
    public DateTimeOffset DateTimeOffsetMethod() => new DateTimeOffset(new DateTime(1998, 4, 20, 0, 0, 0, DateTimeKind.Unspecified), TimeSpan.FromHours(3));
    public ExportedClass ExportedClassMethod() => new() { Id = 420 };
}