using System;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class IntConstructor(int i)
{
    public int Value { get; } = i;
}

[TSExport]
public class StringConstructor(string s)
{
    public string Value { get; } = s;
}

[TSExport]
public class MultipleConstructor(int i, string s)
{
    public int IntValue { get; } = i;
    public string StringValue { get; } = s;
}

[TSExport]
public class ExportedClassConstructor(ExportedClass e)
{
    public ExportedClass Value { get; } = e;
}

[TSExport]
public class ExportedClassMultipleConstructor(ExportedClass e, ExportedClass f)
{
    public ExportedClass Value { get; } = e;
    public ExportedClass Value2 { get; } = f;
}

[TSExport]
public class ExportedClassArrayConstructor(ExportedClass[] e)
{
    public ExportedClass[] Value { get; } = e;
}

[TSExport]
public class ExportedClassActionConstructor(Action<ExportedClass> e)
{
    public Action<ExportedClass> Value { get; } = e;
}