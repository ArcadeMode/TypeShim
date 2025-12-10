namespace TypeShim;


/// <summary>
/// <b>TSModules must be static.</b> 
/// <br/>
/// <br/>
/// TSModules act as a first contact point and should mostly be used to provide instances of <see cref="TSExportAttribute"/>-annotated classes.
/// <br/>
/// <br/>
/// TypeShim generates a TypeScript class that can be constructed with the assemblyExports provided by /_framework/dotnet.js. 
/// <br/>
/// A minimal example with primitive types:
/// <code>
///     [TSModule]
///     public static class MyCoolWasmModule 
///     {
///         public static int TheAnswer { get; init; } = 42;
///         public static string GetGreeting() => "Hello from C#";
///     }
/// </code>
/// 
/// Usage in TypeScript looks like this:
/// 
/// <code>
///     const assemblyExports: AssemblyExports = /* get from /_framework/dotnet.js */;
///     const module = new MyCoolWasmModule(assemblyExports);
///     console.log(module.TheAnswer); // 42
///     console.log(module.GetGreeting()); // "Hello from C#"
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TSModuleAttribute : Attribute
{
}

