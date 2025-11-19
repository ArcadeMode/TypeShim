using System.Runtime.InteropServices.JavaScript;
using TypeScriptExport;

namespace DotnetWasmTypescript.InteropGenerator.SampleClasses
{
    [TsExport]
    public class Person
    {
        public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }

        internal PersonName Name { get; set; }

        internal int Age { get; set; }

        
        public string GetName()
        {
            return Name.Value;
        }

        public void SetName(PersonName name)
        {
            this.Name = name;
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
    //[TsExport]
    //public partial class PersonInterop
    //{

    //    public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }

    //    [JSExport]
    //    //[return: JSMarshalAs<JSType.Any>]
    //    //[return: TsExportAs<string>]
    //    public static string GetName([JSMarshalAs<JSType.Any>, TsExportAs<Person>] object p)
    //    {
    //        Person person = (Person)p;
    //        return person.GetName();
    //    }

    //    [JSExport]
    //    public static void SetName([JSMarshalAs<JSType.Any>, TsExportAs<Person>] object p, [TsExportAs<PersonName>] JSObject name)
    //    {
    //        Person person = (Person)p;
    //        person.SetName((PersonName)name);
    //    }
    //}


}

//namespace QR.Wasm.Impl
//{
//    [TsExport]
//    public partial class Person
//    {
//        public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }
        

//        public static PersonName GetName(Person p)
//        {
//            return p.Name;
//        }

        

//        public static void SetName(Person p, PersonName name)
//        {
//            p.Name = name;
//        }
//    }
//}