using System;
using System.Threading.Tasks;
using TypeShim;

namespace Client.Library;

[TSExport]
public class People()
{
    public required Person[] All { get; set; }
}

/// <summary>
/// Represents a person with a unique identifier, name, age, and a collection of owned pets.
/// </summary>
/// <remarks>The Pets property contains an array of Dog objects that represent the pets currently owned by the
/// person. The Person class provides methods to compare ages with another person and to adopt new pets.
/// </remarks>
[TSExport]
public class Person
{

    // this class has a few comments,
    // To demonstrate that these will reflect in the generated TypeScript code
    // as documentation for the properties and methods of the Person class!

    /// <summary>
    /// A <b>nested</b> type, scoped to <see cref="Person"/>. TypeShim exports it to
    /// TypeScript as <c>Person.LifeStage</c>, keeping the enum namespaced to its owner.
    /// </summary>
    public enum LifeStage { Child, Teenager, Adult, Senior }

    /// <summary>
    /// Comments work <i>including formatting</i>
    /// <list type="bullet">
    /// <item><description>Even</description></item>
    /// <item><description>Lists</description></item>
    /// <item><description><b>Work!</b></description></item>
    /// </list>
    /// </summary>
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required int Age { get; set; }
    public required Dog[] Pets { get; set; }

    /// <summary>
    /// The <see cref="LifeStage"/> derived from <see cref="Age"/>. Demonstrates a nested enum
    /// crossing the interop boundary, consumed in TypeScript as <c>Person.LifeStage</c>.
    /// </summary>
    public LifeStage Stage => Age switch
    {
        < 13 => LifeStage.Child,
        < 20 => LifeStage.Teenager,
        < 65 => LifeStage.Adult,
        _ => LifeStage.Senior,
    };

    /// <summary>
    /// Checks if this person is older than another person.
    /// </summary>
    /// <param name="other">Another person</param>
    /// <returns>True if this person is older than the other person, otherwise false</returns>
    public bool IsOlderThan(Person other)
    {
        return Age > other.Age;
    }

    /// <summary>
    /// Adopts a <b>new pet</b> for this person.
    /// </summary>
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
