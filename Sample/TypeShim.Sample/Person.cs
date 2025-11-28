using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using TypeShim;

namespace TypeShim.Sample;

[TsExport]
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public Dog? Pet { get; set; }

    internal Person(int id, string name, int age, Dog? pet = null)
    {
        Id = id;
        Name = name;
        Age = age;
        Pet = pet;
    }

    public int GetId() => Id;
    public string GetName() => Name;
    public int GetAge() => Age;
    public Dog? GetPet() => Pet;

    // TODO: analyzers for unsupported interop? jsexport no likey nullable value types in arrays it seems
    //public int?[] GetLuckyNumbers() 
    //{
    //    // Example method returning an array with nullable integers
    //    return [7, null, 42, 3];
    //}

    //public Dog?[] GetFriendPets()
    //{
    //    // Example method returning an array with nullable reference types
    //    return [null, null, null];
    //}
}