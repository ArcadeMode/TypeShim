using System;
using System.Threading.Tasks;

namespace TypeShim.Sample;

[TSExport]
public class People()
{
    public required Person[] All { get; set; }
}

[TSExport]
public class Person
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required int Age { get; set; }
    public required Dog[] Pets { get; set; }

    public bool IsOlderThan(Person other)
    {
        return Age > other.Age;
    }

    public void AdoptPet()
    {
        RandomEntityGenerator generator = new();
        Dog pet = generator.GenerateDog();
        Pets = [ ..Pets, pet];
        Console.WriteLine($"{Name} has adopted a new pet named {pet.Name}.");
    }

    public void Adopt(Dog newPet)
    {
        Pets = [ ..Pets, newPet];
        Console.WriteLine($"{Name} has adopted a new pet named {newPet.Name}.");
    }
}

[TSExport]
public class Dog
{
    public required string Name { get; set; }
    public required string Breed { get; set; }
    public required int Age { get; set; }

    public string Bark() => new[] { "bark", "yip", "woof", "arf", "growl", "howl", "whine", "snarl" }[Age % 8];

    public int GetAge(bool asHumanYears)
    {
        return asHumanYears ? Age * 7 : Age;
    }
}
