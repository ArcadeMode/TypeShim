using System;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class OptionalParametersClass
{
    public const int DefaultCount = 7;

    // Primitive defaults: caller may omit or override.
    public int SumWithDefaults(int a, int b = 10, int c = DefaultCount) => a + b + c;
    public string Greet(string name, string greeting = "Hello") => $"{greeting}, {name}";
    public bool Flag(bool value = true) => value;
    public double Scale(double value, double factor = 1.5) => value * factor;

    // Nullable value-type defaults.
    public int NullableOrFallback(int? value = null) => value ?? -1;
    public int NullableWithDefault(int? value = 42) => value ?? -1;

    // DateTime / DateTimeOffset 'default' => DateTime.MinValue.
    public bool DateIsMinValue(DateTime when = default) => when == DateTime.MinValue;
    public bool DateOffsetIsMinValue(DateTimeOffset when = default) => when == DateTimeOffset.MinValue;

    // Confirms an explicitly-passed DateTime still marshals correctly (not just the default path).
    public int YearOf(DateTime when) => when.Year;
}

[TSExport]
public class OptionalCtorParamClass
{
    public OptionalCtorParamClass(int seed = 3) => Seed = seed;

    public int Seed { get; }
    // Nullable settable property => initializer object is omittable/optional.
    public string? Label { get; set; }
}

