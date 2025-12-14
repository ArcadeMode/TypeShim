using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeShim.Sample;

public class RandomEntityGenerator
{
    private static readonly string[] FirstNames =
    {
        "Alice","Bob","Carol","David","Eva","Frank","Grace","Henry","Ivy","Jack",
        "Kara","Liam","Mona","Nate","Olive","Paul","Quinn","Rita","Sam","Tara",
        "Uma","Vince","Wade","Xena","Yuri","Zane"
    };

    private static readonly string[] LastNames =
    {
        "Anderson","Baker","Carter","Dixon","Edwards","Foster","Garcia","Harris",
        "Irwin","Johnson","King","Lopez","Miller","Nelson","Owens","Parker",
        "Quinn","Roberts","Stevens","Turner","Ulrich","Vasquez","White","Xu",
        "Young","Zimmerman"
    };

    private static readonly string[] DogNames =
    {
        "Buddy","Bella","Max","Luna","Rocky","Lucy","Charlie","Daisy","Milo","Sadie"
    };

    private static readonly string[] DogBreeds =
    {
        "Labrador","Beagle","Bulldog","Poodle","Golden Retriever","Boxer","Dachshund","Spaniel"
    };

    private readonly Random _rng;

    public RandomEntityGenerator(int? seed = null)
    {
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public List<Person> GeneratePersons(int count)
    {
        var persons = new List<Person>(count);

        for (int i = 0; i < count; i++)
        {
            var first = FirstNames[_rng.Next(FirstNames.Length)];
            var last = LastNames[_rng.Next(LastNames.Length)];
            // Make names more likely unique by appending index or random number
            string fullName = $"{first} {last}{(_rng.Next(0, 3) == 0 ? $" #{i + 1}" : "")}";

            int age = _rng.Next(12, 121); // 12–120 inclusive

            Dog? pet = null;
            if (_rng.NextDouble() < 0.75) // 75% chance to have a pet because pets are cool
            {
                pet = GenerateDog();
            }

            persons.Add(new Person(i, fullName, age, pet));
        }

        return persons;
    }

    public Dog GenerateDog()
    {
        return new()
        {
            Name = DogNames[_rng.Next(DogNames.Length)],
            Breed = DogBreeds[_rng.Next(DogBreeds.Length)],
            Age = _rng.Next(1, 21) // Pet age 1 to 20
        };
    }
}