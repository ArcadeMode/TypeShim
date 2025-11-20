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
}