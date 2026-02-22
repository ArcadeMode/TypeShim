#nullable enable
using System;
using System.Collections.Generic;
using TypeShim;

namespace TypeShim.Benchmarks.SampleClasses;

/// <summary>
/// A sample class with basic methods and properties
/// </summary>
[TSExport]
public class SampleClass01
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets the count
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Increments the counter
    /// </summary>
    /// <returns>The new count value</returns>
    public int Increment() => ++Count;

    /// <summary>
    /// Greets the user with a message
    /// </summary>
    /// <param name="greeting">The greeting message</param>
    /// <returns>A formatted greeting</returns>
    public string Greet(string greeting) => $"{greeting}, {Name}!";
}

/// <summary>
/// Sample class with static methods
/// </summary>
[TSExport]
public class SampleClass02
{
    /// <summary>
    /// Creates a new instance
    /// </summary>
    public static SampleClass02 Create() => new();

    /// <summary>
    /// Adds two numbers
    /// </summary>
    public static int Add(int a, int b) => a + b;

    /// <summary>
    /// Multiplies two numbers
    /// </summary>
    public static double Multiply(double x, double y) => x * y;

    /// <summary>
    /// Gets the description
    /// </summary>
    public string Description { get; set; } = "Sample";
}

/// <summary>
/// Sample class with various data types
/// </summary>
[TSExport]
public class SampleClass03
{
    /// <summary>
    /// Boolean property
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Integer property
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Long property
    /// </summary>
    public long LongValue { get; set; }

    /// <summary>
    /// Float property
    /// </summary>
    public float FloatValue { get; set; }

    /// <summary>
    /// Double property
    /// </summary>
    public double DoubleValue { get; set; }

    /// <summary>
    /// Checks if value is positive
    /// </summary>
    public bool IsPositive() => Value > 0;

    /// <summary>
    /// Toggles the active state
    /// </summary>
    public void Toggle() => IsActive = !IsActive;
}

/// <summary>
/// Sample class with nullable types
/// </summary>
[TSExport]
public class SampleClass04
{
    /// <summary>
    /// Nullable integer
    /// </summary>
    public int? OptionalValue { get; set; }

    /// <summary>
    /// Nullable string
    /// </summary>
    public string? OptionalName { get; set; }

    /// <summary>
    /// Gets the value or default
    /// </summary>
    public int GetValueOrDefault(int defaultValue) => OptionalValue ?? defaultValue;

    /// <summary>
    /// Sets the optional name
    /// </summary>
    public void SetName(string? name) => OptionalName = name;
}

/// <summary>
/// Sample class with constructors
/// </summary>
[TSExport]
public class SampleClass05
{
    /// <summary>
    /// The identifier
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Creates an instance with an ID
    /// </summary>
    public static SampleClass05 Create(string id) => new(id);

    private SampleClass05(string id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the ID length
    /// </summary>
    public int GetIdLength() => Id.Length;
}

/// <summary>
/// Sample class with array and collection types
/// </summary>
[TSExport]
public class SampleClass06
{
    /// <summary>
    /// Gets the items
    /// </summary>
    public string[] Items { get; private set; } = [];

    /// <summary>
    /// Adds an item
    /// </summary>
    public void AddItem(string item)
    {
        Items = [.. Items, item];
    }

    /// <summary>
    /// Gets items as array
    /// </summary>
    public string[] GetItemsArray() => [.. Items];

    /// <summary>
    /// Gets the count of items
    /// </summary>
    public int GetCount() => Items.Length;
}

/// <summary>
/// Sample class with multiple parameters
/// </summary>
[TSExport]
public class SampleClass07
{
    /// <summary>
    /// Calculates area
    /// </summary>
    public double CalculateArea(double width, double height) => width * height;

    /// <summary>
    /// Formats a full name
    /// </summary>
    public string FormatName(string firstName, string lastName, string? middleName = null)
    {
        return middleName != null 
            ? $"{firstName} {middleName} {lastName}" 
            : $"{firstName} {lastName}";
    }

    /// <summary>
    /// Sums multiple values
    /// </summary>
    public int Sum(int a, int b, int c, int d) => a + b + c + d;
}

/// <summary>
/// Sample class with return types
/// </summary>
[TSExport]
public class SampleClass08
{
    /// <summary>
    /// Returns void
    /// </summary>
    public void DoNothing() { }

    /// <summary>
    /// Returns a string
    /// </summary>
    public string GetString() => "Hello";

    /// <summary>
    /// Returns an integer
    /// </summary>
    public int GetInt() => 42;

    /// <summary>
    /// Returns a boolean
    /// </summary>
    public bool GetBool() => true;

    /// <summary>
    /// Returns a double
    /// </summary>
    public double GetDouble() => 3.14;
}

/// <summary>
/// Sample class with complex operations
/// </summary>
[TSExport]
public class SampleClass09
{
    private int _state;

    /// <summary>
    /// Increments state and returns the result
    /// </summary>
    public int IncrementAndGet() => ++_state;

    /// <summary>
    /// Resets the state
    /// </summary>
    public void Reset() => _state = 0;

    /// <summary>
    /// Gets the current state
    /// </summary>
    public int GetState() => _state;

    /// <summary>
    /// Sets the state to a specific value
    /// </summary>
    public void SetState(int value) => _state = value;
}

/// <summary>
/// Sample class with documentation
/// </summary>
/// <remarks>
/// This class demonstrates comprehensive XML documentation
/// </remarks>
[TSExport]
public class SampleClass10
{
    /// <summary>
    /// Performs a calculation
    /// </summary>
    /// <param name="x">The first operand</param>
    /// <param name="y">The second operand</param>
    /// <returns>The result of the calculation</returns>
    /// <remarks>This method multiplies two numbers</remarks>
    public int Calculate(int x, int y) => x * y;

    /// <summary>
    /// Validates input
    /// </summary>
    /// <param name="input">The input string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool Validate(string input) => !string.IsNullOrEmpty(input);
}
public sealed class AlphaToken
{
    public string Name { get; init; } = "";
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;

    public int NextId() => Id + 1;
    public void Deactivate() => IsActive = false;
    public override string ToString() => $"{Name}#{Id}";
}

public sealed class BetaCounter
{
    public long Value { get; private set; }
    public long Step { get; set; } = 1;

    public void Add() => Value += Step;
    public void Reset(long value = 0) => Value = value;
    public bool IsZero() => Value == 0;
}

public sealed class GammaClock
{
    public DateTimeOffset Now { get; private set; } = DateTimeOffset.UtcNow;
    public TimeSpan Skew { get; set; } = TimeSpan.Zero;

    public DateTimeOffset Read() => Now + Skew;
    public void Tick(TimeSpan delta) => Now = Now.Add(delta);
    public void SyncUtc() => Now = DateTimeOffset.UtcNow;
}

public sealed class DeltaFlags
{
    public int Bits { get; private set; }
    public string Label { get; set; } = "delta";

    public void Set(int mask) => Bits |= mask;
    public void Clear(int mask) => Bits &= ~mask;
    public bool Has(int mask) => (Bits & mask) == mask;
}

public sealed class EpsilonBucket<T>
{
    public List<T> Items { get; } = new();
    public int Capacity { get; set; } = 16;

    public void Add(T item) => Items.Add(item);
    public int Count() => Items.Count;
    public void Clear() => Items.Clear();
}

public sealed class ZetaRange
{
    public int Start { get; set; }
    public int End { get; set; }
    public bool Inclusive { get; set; } = true;

    public bool Contains(int value) => Inclusive ? (value >= Start && value <= End) : (value > Start && value < End);
    public int Length() => Math.Max(0, End - Start);
    public void Normalize() { if (End < Start) (Start, End) = (End, Start); }
}

public sealed class EtaCacheKey
{
    public string Kind { get; init; } = "eta";
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public int Version { get; set; } = 1;

    public string Format() => $"{Kind}:{Version}:{CorrelationId:D}";
    public EtaCacheKey Bump() => new() { Kind = Kind, Version = Version + 1, CorrelationId = CorrelationId };
    public bool IsV1() => Version == 1;
}

public sealed class ThetaMath
{
    public double A { get; set; }
    public double B { get; set; }
    public double Epsilon { get; set; } = 1e-9;

    public double Sum() => A + B;
    public bool NearlyEquals() => Math.Abs(A - B) <= Epsilon;
    public void Swap() => (A, B) = (B, A);
}

public sealed class IotaUser
{
    public string UserName { get; set; } = "";
    public string? Email { get; set; }
    public DateTime CreatedUtc { get; } = DateTime.UtcNow;

    public bool HasEmail() => !string.IsNullOrWhiteSpace(Email);
    public void SetEmail(string? email) => Email = email;
    public string Display() => HasEmail() ? $"{UserName} <{Email}>" : UserName;
}

public sealed class KappaSwitch
{
    public bool Enabled { get; set; }
    public string Reason { get; set; } = "";

    public void Enable(string reason = "") { Enabled = true; Reason = reason; }
    public void Disable(string reason = "") { Enabled = false; Reason = reason; }
    public string State() => Enabled ? "On" : "Off";
}

public sealed class LambdaQueue<T>
{
    private readonly Queue<T> _queue = new();

    public int Count => _queue.Count;
    public bool IsEmpty => _queue.Count == 0;
    public T? LastPushed { get; private set; }

    public void Enqueue(T item) { _queue.Enqueue(item); LastPushed = item; }
    public bool TryDequeue(out T? item) => _queue.TryDequeue(out item);
    public void Clear() => _queue.Clear();
}

public sealed class MuConfig
{
    public string Environment { get; set; } = "dev";
    public int RetryCount { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    public bool IsProd() => string.Equals(Environment, "prod", StringComparison.OrdinalIgnoreCase);
    public void UseProd() => Environment = "prod";
    public string Summary() => $"{Environment}; retries={RetryCount}; timeout={Timeout.TotalSeconds:0}s";
}