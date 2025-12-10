namespace TypeShim;

/// <summary>
/// <b>TSExport classes must be non-static.</b> 
/// <br/>
/// <br/>
/// TSExport classes are meant to be used as as instance objects in TypeScript code, they should not contain static members in the public API.
/// Whenever a method returns an instance of a TSExport class, TypeShim generates a TypeScript proxy class that wraps the interop calls to the underlying C# instance.
/// <code>
///     [TSExport]
///     public class MyCounter 
///     {
///         public int Count { get; private set; } = 0;
///         public string Increment() => $"Hello from C#, Count is now {++Count}";
///     }
/// </code>
/// 
/// Usage in TypeScript looks like this:
/// 
/// <code>
///     const module: SomeWasmModuleYouMade = /* assume you already have a TSModule here, could also be another TSExport-ed class */;
///     const counter: MyCounter = module.GetCounter();
///     console.log(counter.Count); // 0
///     console.log(counter.Increment()); // "Hello from C#, Count is now 1"
///     console.log(counter.Count); // 1
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TSExportAttribute : Attribute
{
}