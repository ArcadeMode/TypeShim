using System.Runtime.InteropServices.JavaScript;
using TypeShim;

namespace QR.Wasm
{
    [TsExport]
    public class Person
    {
        internal string Name { get; set; }

        internal int Age { get; set; }

        internal Dog Pet { get; set; }
        
        public string GetName()
        {
            return Name;
        }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public Dog GetPet()
        {
            return Pet;
        }
    }
}