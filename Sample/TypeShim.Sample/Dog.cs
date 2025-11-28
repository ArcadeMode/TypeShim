using System;
using TypeShim;

namespace TypeShim.Sample;

[TsExport]
public class Dog
{
    public required string Name { get; set; }
    public required string Breed { get; set; }

    public string GetName() => Name;

    public string GetBreed() => Breed;

    public string Bark() => new[] { "bark", "yip", "woof", "arf", "growl", "howl", "whine", "snarl" }[Random.Shared.Next(8)];

}
