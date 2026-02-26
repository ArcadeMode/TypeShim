namespace TypeShim;

/// <summary>
/// <b>TSExport classes must be non-static.</b> 
/// <br/>
/// <br/>
/// TSExport classes define your .NET-TS API surface. Public methods, properties and constructors, both static and member, are accessible from the generated TypeScript library.<br/>
/// Whenever a TSExport class crosses the interop boundary, TypeShim automatically wraps it in an appropriate TypeScript class matching the C# class's public signature.
/// <code>
///     [TSExport]
///     public class MyCounter 
///     {
///         public int Count { get; private set; } = 0;
///         public string Increment() => $"Hello from C#, Count is now {++Count}";
///         public bool EqualsCount(MyCounter other) => Count == other.Count;
///     }
/// </code>
/// 
/// Can be used in TypeScript like this:
/// 
/// <code>
///     const counter: MyCounter = new MyCounter();
///     console.log(counter.Count); // 0
///     console.log(counter.Increment()); // "Hello from C#, Count is now 1"
///     console.log(counter.Count); // 1
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TSExportAttribute : Attribute
{
}