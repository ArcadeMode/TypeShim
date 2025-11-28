using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeShim.Sample;

public class RandomPersonGenerator
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

    public RandomPersonGenerator(int? seed = null)
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
                pet = new Dog
                {
                    Name = DogNames[_rng.Next(DogNames.Length)],
                    Breed = DogBreeds[_rng.Next(DogBreeds.Length)]
                };
            }

            persons.Add(new Person(i, fullName, age, pet));
        }

        return persons;
    }

    /// <summary>
    /// Creates random, non-overlapping partner pairs.
    /// Returns a dictionary where each key has a partner value; pairs appear only once.
    /// </summary>
    /// <param name="persons">Source list of persons.</param>
    /// <param name="pairProbability">
    /// Probability that any eligible (unpaired) person will be paired.
    /// Higher values produce more pairs. 0–1.
    /// </param>
    /// <returns>Dictionary of Person -> PartnerPerson (each pair only stored once)</returns>
    public Dictionary<Person, Person> GeneratePartnerDictionary(
        List<Person> persons,
        double pairProbability = 0.5)
    {
        if (persons == null) throw new ArgumentNullException(nameof(persons));
        if (pairProbability < 0 || pairProbability > 1) throw new ArgumentOutOfRangeException(nameof(pairProbability));

        // Shuffle to randomize pairing order
        var shuffled = persons.OrderBy(_ => _rng.Next()).ToList();
        var used = new HashSet<Person>();
        var partnerships = new Dictionary<Person, Person>();

        int i = 0;
        while (i < shuffled.Count - 1)
        {
            var a = shuffled[i];
            if (used.Contains(a))
            {
                i++;
                continue;
            }

            // Decide whether to attempt a pairing
            if (_rng.NextDouble() <= pairProbability)
            {
                // Find next available partner
                int j = i + 1;
                while (j < shuffled.Count && used.Contains(shuffled[j]))
                    j++;

                if (j < shuffled.Count)
                {
                    var b = shuffled[j];
                    // Pair a with b (store only one direction)
                    partnerships[a] = b;
                    used.Add(a);
                    used.Add(b);
                    // Move past b
                    i = j + 1;
                    continue;
                }
            }

            // Not paired; move on
            i++;
        }

        return partnerships;
    }

    /// <summary>
    /// Generates a dictionary mapping each person to an array of their friends.
    /// Each person can have up to half the input size as friends, with the number of friends
    /// centered around a quarter of the input size (normal-like distribution).
    /// Friendships are mutual: if A is a friend of B, B is a friend of A.
    /// </summary>
    /// <param name="persons">Source list of persons.</param>
    /// <returns>Dictionary of Person -> array of friends (Person[])</returns>
    public Dictionary<Person, Person[]> GenerateFriendsDictionary(List<Person> persons)
    {
        if (persons == null) throw new ArgumentNullException(nameof(persons));
        int n = persons.Count;
        if (n < 2) return persons.ToDictionary(p => p, p => Array.Empty<Person>());

        // Parameters for friend count distribution
        int minFriends = 0;
        int maxFriends = Math.Max(1, n / 2);
        double mean = n / 4.0;
        double stddev = Math.Max(1.0, n / 8.0);

        // Helper: sample friend count for a person
        int SampleFriendCount()
        {
            // Box-Muller transform for normal distribution
            double u1 = 1.0 - _rng.NextDouble();
            double u2 = 1.0 - _rng.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            int count = (int)Math.Round(mean + stddev * randStdNormal);
            return Math.Max(minFriends, Math.Min(maxFriends, count));
        }

        // Prepare friend sets
        var friendSets = persons.ToDictionary(p => p, p => new HashSet<Person>());

        // Shuffle persons for random assignment
        var shuffled = persons.OrderBy(_ => _rng.Next()).ToList();

        // Assign friends
        foreach (var person in shuffled)
        {
            int desiredFriends = SampleFriendCount();
            var candidates = shuffled.Where(p => p != person && !friendSets[person].Contains(p) && friendSets[p].Count < maxFriends).OrderBy(_ => _rng.Next()).ToList();

            foreach (var candidate in candidates)
            {
                if (friendSets[person].Count >= desiredFriends) break;
                if (friendSets[candidate].Count >= maxFriends) continue;
                // Mutual friendship
                friendSets[person].Add(candidate);
                friendSets[candidate].Add(person);
            }
        }

        // Convert sets to arrays
        return friendSets.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }
}