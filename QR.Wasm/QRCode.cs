using System;
using System.Runtime.InteropServices.JavaScript;
using TypeShim;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

namespace QR.Wasm
{
    [TsExport]
    public class QRCode
    {
        public static string Generate(string text, int pixelsPerBlock)
        {
            return QRHelper.Generate(text, pixelsPerBlock);
        }
    }


    [TsExport]
    public partial class PersonName
    {
        public static implicit operator PersonName(string value) => new() { Value = value };
        public static implicit operator PersonName(JSObject jsObj) => new()
        {
            Value = jsObj.GetPropertyAsString(nameof(Value)) ?? throw new ArgumentException($"JSObject is not a valid {nameof(PersonName)}", nameof(jsObj))
        };

        public string Value { get; set; }
    }

    [TsExport]
    public struct Dog
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    [TsExport]
    public enum Color
    {
        Red,
        Green,
        Blue
    }
}

