using DotnetWasmTypescript.InteropGenerator.SampleClasses;
using System.Runtime.InteropServices.JavaScript;
using TypeScriptExport;
using TypeScriptExportGenerator;

namespace QR.Wasm
{
    [TsExport]
    public partial class PersonRepository
    {
        public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }


        internal Person Person1 = new Person()
        {
            Name = "Alice",
            Age = 28,
        };

        internal Person Person2 = new Person()
        {
            Name = "Bob",
            Age = 35,
        };

        [return: TsExportAs<Person>]
        public Person GetPerson1()
        {
            return Person1;
        }

        [return: TsExportAs<Person>]
        public Person GetPerson2()
        {
            return Person2;
        }
    }
}

//namespace QR.Wasm.Impl
//{
//    [TsExport]
//    public static class PersonRepository
//    {
//        public static Wasm.Person GetPerson1()
//        {
//            return QR.Wasm.PersonRepository.Person1;
//        }

//        public static Wasm.Person GetPerson2()
//        {
//            return QR.Wasm.PersonRepository.Person2;
//        }
//    }
//}
