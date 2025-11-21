using QR.Core;
using QR.Wasm;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using TypeScriptExport;
using TypeScriptExportGenerator;

[assembly:System.Runtime.Versioning.SupportedOSPlatform("browser")]

namespace QR.Wasm
{
    [TsExport]
    public class QRCode
    {
        public static string GenerateMyQR(string text, int pixelsPerBlock)
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

