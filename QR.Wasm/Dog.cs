using TypeShim;

namespace QR.Wasm
{
    [TsExport]
    public class Dog
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string GetName()
        {
            return Name;
        }
    }
}

