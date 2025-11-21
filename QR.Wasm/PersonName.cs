using System;
using System.Runtime.InteropServices.JavaScript;
using TypeShim;

namespace QR.Wasm
{
    [TsExport]
    public class PersonName
    {
        public static implicit operator PersonName(string value) => new() { Value = value };
        public static implicit operator PersonName(JSObject jsObj) => new()
        {
            Value = jsObj.GetPropertyAsString(nameof(Value)) ?? throw new ArgumentException($"JSObject is not a valid {nameof(PersonName)}", nameof(jsObj))
        };

        public string Value { get; set; }
    }
}

