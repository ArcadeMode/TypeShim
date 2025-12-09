using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using TypeShim;

namespace TypeShim.Sample;

[TSExport]
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

    public bool IsOlderThan(Person other)
    {
        return Age > other.Age;
    }

    public void AdoptPet()
    {
        if (Pet != null)
        {
            Console.WriteLine($"{Name} already has a pet named {Pet.Name}.");
            return;
        }
        RandomEntityGenerator generator = new();
        Pet = generator.GenerateDog();
        Console.WriteLine($"{Name} has adopted a new pet named {Pet.Name}.");
    }
}