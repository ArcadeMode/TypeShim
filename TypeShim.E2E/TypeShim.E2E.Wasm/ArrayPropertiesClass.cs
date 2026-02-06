using System.Runtime.InteropServices.JavaScript;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class  ArrayPropertiesClass
{
    public byte[] ByteArrayProperty { get; set; } = [];
    public JSObject[] JSObjectArrayProperty { get; set; } = [];
    public object[] ObjectArrayProperty { get; set; } = [];
    public ExportedClass[] ExportedClassArrayProperty { get; set; } = [];
    public int[] IntArrayProperty { get; set; } = [];
    public string[] StringArrayProperty { get; set; } = [];
    public double[] DoubleArrayProperty { get; set; } = [];
}
