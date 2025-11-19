using System.Runtime.InteropServices.JavaScript;
using TypeScriptExport;
using TypeScriptExportGenerator;

namespace QR.Wasm
{
    [TsExport]
    public partial class PersonRepository
    {
        public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }


        internal static Person Person1 = new Person()
        {
            Name = "Alice",
            Age = 28,
            Pet = new Dog
            {
                Name = "Buddy",
                Age = 4
            }
        };

        internal static Person Person2 = new Person()
        {
            Name = "Bob",
            Age = 35,
            Pet = new Dog
            {
                Name = "Max",
                Age = 6
            }
        };

        public Person GetPerson1()
        {
            return Person1;
        }

        public Person GetPerson2()
        {
            return Person2;
        }

        public static Person GetPerson3()
        {
            return Person1;
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
