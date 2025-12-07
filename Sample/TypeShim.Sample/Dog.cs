using System;
using TypeShim;

namespace TypeShim.Sample;

[TsExport]
public class Dog
{
    public required string Name { get; set; }
    public required string Breed { get; set; }
    public required int Age { get; set; }

    public string Bark() => new[] { "bark", "yip", "woof", "arf", "growl", "howl", "whine", "snarl" }[Age % 8];

}
