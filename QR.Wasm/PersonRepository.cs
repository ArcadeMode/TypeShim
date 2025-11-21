using System.Runtime.InteropServices.JavaScript;
using TypeShim;

namespace QR.Wasm
{
    [TsExport]
    public partial class PersonRepository
    {
        public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }

        private static readonly PersonRepository _instance = new();

        internal Person Person1 = new Person()
        {
            Name = "Alice",
            Age = 28,
            Pet = new Dog
            {
                Name = "Buddy",
                Age = 4
            }
        };

        internal Person Person2 = new Person()
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

        public static PersonRepository GetInstance()
        {
            return _instance;
        }
    }
}
