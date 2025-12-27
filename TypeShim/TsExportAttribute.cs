namespace TypeShim;

/// <summary>
/// <b>TSExport classes must be non-static.</b> 
/// <br/>
/// <br/>
/// TSExport classes define your interop API surface. You can use static and instance members in your TSExport classes, you will use them as static or instance in TypeScript too.
/// Whenever a method returns an instance of a TSExport class, TypeShim generates a TypeScript proxy class that wraps the interop calls to the underlying C# instance.
/// <code>
///     [TSExport]
///     public class MyCounter 
///     {
///         public static MyCounter GetCounter() => new MyCounter();
///         public int Count { get; private set; } = 0;
///         public string Increment() => $"Hello from C#, Count is now {++Count}";
///     }
/// </code>
/// 
/// Usage in TypeScript looks like this:
/// 
/// <code>
///     const counter: MyCounter.Proxy = MyCounter.GetCounter();
///     console.log(counter.Count); // 0
///     console.log(counter.Increment()); // "Hello from C#, Count is now 1"
///     console.log(counter.Count); // 1
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TSExportAttribute : Attribute
{
}