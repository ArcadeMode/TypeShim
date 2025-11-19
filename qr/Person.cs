using System.Runtime.InteropServices.JavaScript;
using TypeScriptExport;

namespace QR.Wasm
{
    [TsExport]
    public class Person
    {
        public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }

        internal PersonName Name { get; set; }

        internal int Age { get; set; }

        internal Dog Pet { get; set; }
        
        public string GetName()
        {
            return Name.Value;
        }

        public void SetName(PersonName name)
        {
            this.Name = name;
        }
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