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
    public partial class QRCode
    {
        public string Wowzers { get; set; }

        [JSExport]
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
    public partial class PersonNameExport
    {
        [JSExport]
        [return: JSMarshalAs<JSType.String>]
        internal static string GetValue([JSMarshalAs<JSType.Any>] object personNameObject)
        {

            if (personNameObject is PersonName personName)
            {
                return GetValue(personName);
            }
            throw new ArgumentException($"Argument was not compatible with expected type {nameof(PersonName)}");
        }

        public static string GetValue(PersonName personName)
        {
            return personName.Value;
        }
    }

    //[TsExport]
    //public partial class PersonProvider
    //{
    //    public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }

    //    [JSExport]
    //    [return: JSMarshalAs<JSType.Any>]
    //    internal static object GetPerson(JSObject personNameObject)
    //    {
    //        if (personNameObject.GetPropertyAsString("Value") is string name)
    //        {
    //            return GetPerson((PersonName)name);
    //        }
    //        throw new ArgumentException($"Argument was not compatible with expected type {nameof(PersonName)}");
    //    }

    //    public static Person GetPerson(PersonName name)
    //    {
    //        return new Person
    //        {
    //            Name = name,
    //            Age = 30,
    //            Pet = new Dog
    //            {
    //                Name = "Fido",
    //                Age = 5
    //            }
    //        };
    //    }
    //}




    [TsExport]
    public struct Dog
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    [TsExport]
    public partial class DogExport
    {
        [JSExport]
        [return: JSMarshalAs<JSType.Any>]
        internal static object GetOwner([JSMarshalAs<JSType.Any>] object daggoe)
        {
            return GetOwner((Dog)daggoe);
        }

        public static Person GetOwner(Dog daggoe)
        {
            return new Person
            {
                Name = "henkie",
                Age = 30,
                Pet = daggoe
            };
        }
    }

    [TsExport]
    public enum Color
    {
        Red,
        Green,
        Blue
    }
}

