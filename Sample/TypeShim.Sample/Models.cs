using System;

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
    public required Dog? Pet { get; set; }

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

    public void Adopt(Dog newPet)
    {
        if (Pet != null)
        {
            Console.WriteLine($"{Name} already has a pet named {Pet.Name}. Cannot adopt {newPet.Name}");
            return;
        }
        Pet = newPet;
        Console.WriteLine($"{Name} has adopted a new pet named {Pet.Name}.");
    }
}

[TSExport]
public class Dog
{
    public required string Name { get; set; }
    public required string Breed { get; set; }
    public required int Age { get; set; }
    public int[] Ints { get; set; } = [];

    public string Bark() => new[] { "bark", "yip", "woof", "arf", "growl", "howl", "whine", "snarl" }[Age % 8];

    public int GetAge(bool asHumanYears)
    {
        return asHumanYears ? Age * 7 : Age;
    }
}
