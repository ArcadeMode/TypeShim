using System.Runtime.InteropServices.JavaScript;
using TypeShim;

namespace QR.Wasm;

[TsExport]
public class PersonRepository
{
    private static readonly PersonRepository _instance = new();

    internal Person Person = new Person()
    {
        Name = "Alice",
        Age = 28,
        Pet = new Dog
        {
            Name = "Buddy",
            Age = 4
        }
    };

    public Person GetPerson()
    {
        return Person;
    }


    public static PersonRepository GetInstance()
    {
        return _instance;
    }
}
