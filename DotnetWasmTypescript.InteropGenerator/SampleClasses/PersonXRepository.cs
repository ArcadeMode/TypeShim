using DotnetWasmTypescript.InteropGenerator.SampleClasses;
using TypeScriptExport;

namespace QR.Wasm;

[TsExport]
public partial class PersonXRepository
{
    public string PlaceHolderOrTsGenNoWorkey_FixThis { get; set; }


    internal PersonX Person1 = new PersonX()
    {
        Name = "Alice",
        Age = 28,
    };

    internal static PersonX Person2 = new PersonX()
    {
        Name = "Bob",
        Age = 35,
    }
;

    public PersonX GetPerson1()
    {
        return Person1;
    }

    public static PersonX GetPerson2()
    {
        return Person2;
    }
}