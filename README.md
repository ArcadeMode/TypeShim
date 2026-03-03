<h1 align=center tabindex=-1>TypeShim</h1>
<p align=center tabindex=-1>
  <i>Seamless, type-safe interop between .NET WebAssembly and TypeScript</i>
</p>

<img align="right" tabindex=-1 src="https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/ArcadeMode/0f24ed28316a25f6293d5771a247f19d/raw/typeshim-tests-badge.json" alt="Test status" />

## Why TypeShim
TypeShim makes interop between .NET WebAssembly and TypeScript effortless. One `[TSExport]` projects an entire .NET class across the interop boundary, generating a fully-typed mirror in TypeScript. The result is a natural programming experience on both sides: static and instance members, constructors, properties, methods, object instances, reference equality and value types - it all just _works_.

TypeShim generates strongly-typed interop code for both C# & TypeScript, tailored to your project, so the boundary remains type-safe without manual glue code. The implementation is verified by a comprehensive test suite covering the full pipeline, from code generation through multiple runtimes, ensuring consistent, reliable behavior. Optimized for minimal build impact, TypeShim achieves sub 100 millisecond codegen times even in large projects.

## At a glance

- 📤 Class level exports.
- 💎 [Rich type support](#enriched-type-support).
- ✍ Powerful [concepts](#concepts).
- 🦾 Thoroughly validated
- ⚡ Tuned for [high performance](#performance)
- 👍 [Easy setup](#installing)

## Samples
Samples below demonstrate the same operations when interfacing with TypeShim generated code vs `JSExport` generated code. Either way you will load your wasm browser app as [described in the docs](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/wasm-browser-app?view=aspnetcore-10.0#javascript-interop-on-). The runtime created by `dotnet.create()` can be passed directly into the provided `TypeShimInitializer`'s `initialize` method. The initializer exists so that helper functions for type marshalling can be set up and a reference to the assembly exports can be retrieved for the generated types to use internally.

### TypeShim
A simple example where we have an app about 'people', just to show basic language use powered by TypeShim.
The C# implementation is just classes with the mentioned `[TSExport]` annotation.

```csharp
using TypeShim;

namespace Sample.People;

[TSExport]
public class PeopleRepository
{
    internal List<Person> People = [
        new Person()
        {
            Name = "Alice",
            Age = 26,
        }
    ];

    public Person GetPerson(int i)
    {
        return People[i];
    }

    public void AddPerson(Person p)
    {
        People.Add(p);
    }
}

[TSExport]
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    public bool IsOlderThan(Person p)
    {
        return Age > p.Age;
    }
}
```

On the TypeScript side things look familiar, class names, properties, methods and constructors all resemble the exported C# classes. Note the aforementioned TypeShimInitializer being called before engaging with the generated types. 

```js
import { TypeShimInitializer, PeopleRepository, Person } from './typeshim.ts';

public async UsingTypeShim() {
    const runtime = await dotnet.withApplicationArguments(args).create()
    await TypeShimInitializer.initialize(runtime);

    const repository = new PeopleRepository();
    const alice: Person = repository.GetPerson(0);
    const bob = new Person({
      Name: 'Bob',
      Age: 20
    });

    console.log(alice.Name, bob.Name); // prints "Alice", "Bob"
    console.log(alice.IsOlderThan(bob)) // prints false
    alice.Age = 30;
    console.log(alice.IsOlderThan(bob)) // prints true

    repository.AddPerson({ Name: "Charlie", Age: 40 });
    const charlie: Person = repository.GetPerson(1);
    console.log(alice.IsOlderThan(charlie)) // prints false
    console.log(bob.IsOlderThan(charlie)) // prints true
}
```

#### 'Raw' JSExport
Here you can see a quick demonstration of roughly the same behavior as the TypeShim sample, with handwritten JSExport. Certain parts enabled by TypeShim have not been replicated as the point may be clear at a glance: this is a large amount of difficult to maintain boilerplate if you have to write it yourself.

<details>
  <summary>See the 'Raw' <code>JSExport</code> implementation</summary>
&nbsp;

Note the error sensitivity of passing untyped objects across the interop boundary.

```ts
public async UsingRawJSExport(exports: any) {
    const runtime = await dotnet.withApplicationArguments(args).create();
    const exports = runtime.assemblyExports;

    const repository: any = exports.Sample.People.PeopleRepository.GetInstance(); 
    const alice: any = exports.Sample.People.PeopleRepository.GetPerson(repository, 0);
    const bob: any = exports.Sample.People.People.ConstructPerson("Bob", 20);
    
    console.log(exports.Sample.People.Person.GetName(alice), exports.Sample.People.Person.GetName(bob)); // prints "Alice", "Bob"
    console.log(exports.Sample.People.Person.IsOlderThan(alice, bob)); // prints false
    exports.Sample.People.Person.SetAge(alice, 30);
    console.log(exports.Sample.People.Person.IsOlderThan(alice, bob)); // prints true

    exports.Sample.People.PeopleRepository.AddPerson(repository, "Charlie", 40);
    const charlie: any = exports.Sample.People.PeopleRepository.GetPerson(repository, 1);
    console.log(alice.IsOlderThan(charlie)) // prints false
    console.log(bob.IsOlderThan(charlie)) // prints true
}
```

```csharp
namespace Sample.People;

public class PeopleRepository
{
    internal List<Person> People = [
        new Person()
        {
            Name = "Alice",
            Age = 26,
        }
    ];

    private static readonly PeopleRepository _instance = new();
    [JSExport]
    [return: JSMarshalAsType<JSType.Object>]
    public static object GetInstance()
    {
        return _instance;
    }

    [JSExport]
    [return: JSMarshalAsType<JSType.Object>]
    public static object GetPerson([JSMarshalAsType<JSType.Object>] object repository, [JSMarshalAsType<JSType.Number>] int i)
    {
        PeopleRepository pr = (PeopleRepository)repository;
        return pr.People[i];
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    [JSExport]
    [return: JSMarshalAsType<JSType.String>]
    public static string ConstructPerson([JSMarshalAsType<JSType.Object>] JSObject obj)
    {
        return new Person() // Fragile
        {
            Name = obj.GetPropertyAsString("Name"),
            Age = obj.GetPropertyAsInt32("Age")
        }
    }

    [JSExport]
    [return: JSMarshalAsType<JSType.String>]
    public static string GetName([JSMarshalAsType<JSType.Object>] object instance)
    {
        Person p = (Person)instance;
        return p.Name;
    }

    [JSExport]
    [return: JSMarshalAsType<JSType.Void>]
    public static void SetName([JSMarshalAsType<JSType.Object>] object instance, [JSMarshalAsType<JSType.String>] string name)
    {
        Person p = (Person)instance;
        p.Name = name;
    }

    [JSExport]
    [return: JSMarshalAsType<JSType.Number>]
    public static int GetAge([JSMarshalAsType<JSType.Object>] object instance)
    {
        Person p = (Person)instance;
        return p.Age;
    }

    [JSExport]
    [return: JSMarshalAsType<JSType.Void>]
    public static void SetAge([JSMarshalAsType<JSType.Object>] object instance, [JSMarshalAsType<JSType.Number>] int age)
    {
        Person p = (Person)instance;
        p.Age = age;
    }

    [JSExport]
    [return: JSMarshalAsType<JSType.Void>]
    public static void IsOlderThan([JSMarshalAsType<JSType.Object>] object instance, [JSMarshalAsType<JSType.Object>] object other)
    {
        Person p = (Person)instance;
        Person o = (Person)other;
        return p.Age > o.Age;
    }
}
```
</details>

## <a name="concepts"></a> TypeShim Concepts

Let's briefly introduce the concepts that are used in TypeShim. For starters, you will be using `[TSExport]` to annotate your classes to define your interop API. Every annotated class will receive a TypeScript counterpart. The members included in the TypeScript code are limited to the _public_ members. That includes constructors, properties and methods, both static and instance.

The build-time generated TypeScript can provide the following subcomponents for each exported class `MyClass`:

### Proxies (`MyClass`)
`MyClass` grants access to the exported C# `MyClass` class _in a proxying capacity_, this type will also be referred to as a `Proxy`. A dotnet instance of the class being proxied _always_ lives in the dotnet runtime when you receive a proxy instance, changes to the dotnet object will reflect in the JS runtime. To acquire an instance you may invoke your exported constructor or returned by any method and/or property. Proxies may also be used as parameters and will behave as typical reference types when performing any such operation.

### Snapshots (`MyClass.Snapshot`)
The snapshot type is generated if your class has public properties. TypeShim provides a utility function `MyClass.materialize(your_instance)` that returns a snapshot. Snapshots are fully decoupled from the dotnet object and live in the JS runtime, this means that changes to the proxy object do not reflect in a snapshot. Properties of proxy types will be materialized as well. This is useful when you no longer require the Proxy instance but want to continue working with its data.

### <a name="initializers"></a> Initializers (`MyClass.Initializer`)
The `Initializer` type is generated if the exported class has an exported constructor and accepts an initializer body in `new()` expressions. Initializer objects live in the JS runtime and may be used in the process of creating dotnet object instances, if it exists it will be a parameter in the constructor of the associated Proxy.

Additionally, _if the class exports a parameterless constructor_ then initializer objects can also be passed instead of proxies in method parameters, property setters and even in other initializer objects. TypeShim will construct the appropriate dotnet class instance(s) from the initializer. Initializers can even contain properties of Proxy type instead of an Initializer if you want to reference an existing object. Below a brief demonstration of the provided flexibility.

<table>
<tr>
<td style="width: 400px;">

```ts
const bike = new Bike("Ducati", { 
  Cc: 1200,
  Hp: 147
});
const rider = new Rider({
    Name: "Casey Stoner",
    Bike: bike
});

```

Passing an object reference in an initializer object.

</td>
<td style="width: 400px;">

```ts
const bike: Bike.Initializer = {
  Brand: "Ducati" 
  Cc: 1200,
  Hp: 147
};
const rider = new Rider({
    Name: "Pecco",
    Bike: bike
});
```

Passing an initializer object in another initializer object.

</td>
</tr>
</table>

> 💡 Arrays of mixed proxies and initializers are supported as parameters for methods if the above conditions for the array element type are satisfied. The contained initializer objects will be constructed into new dotnet class instances while the object references behind the proxies are preserved.

## <a name="enriched-type-support"></a> Enriched Type support

TypeShim enriches the supported types by JSExport by adding _your_ classes to the [types marshalled by .NET](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings). Repetitive patterns for type transformation are readily supported and tested in TypeShim.

Of course, TypeShim brings all [types marshalled by .NET](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings) to TypeScript. Makes TypeShim officially offer a superset of the .NET types available in JS.

TypeShim aims to continue to broaden its type support. Suggestions and contributions are welcome.

| TypeShim Shimmed Type | Mapped Type | Support | Note |
|----------------------|-------------|--------|------|
| `Object` (`object`)  | `ManagedObject`       | ✅     | a disposable opaque handle |
| `TClass`  | `ManagedObject`       | ✅     |  unexported reference types    |
| `TClass`                  |  `TClass`        | ✅     | `TClass` generated in TypeScript* |
| `Task<TClass>`            | `Promise<TClass>`| ✅     | `TClass` generated in TypeScript* |
| `Task<T[]>`            | `Promise<T[]>`| 💡     | under consideration (for all array-compatible `T`) |
| `TClass[]`                | `TClass[]`       | ✅     | `TClass` generated in TypeScript* |
| `JSObject`           | `TClass`         | ✅     | [Initializers](#initializers) |
| `TEnum`      | `TEnum`       | 💡     | under consideration |
| `IEnumerable<T>`     | `T[]`       | 💡     | under consideration |
| `Dictionary<TKey, TValue>` | `?`     | 💡     | under consideration |
| `(T1, T2)` | `[T1, T2]`     | 💡     | under consideration |

Table 1. TypeShim supported interop types

| .NET Marshalled Type                  | Mapped Type | Support | Note |
|----------------------|-------------|--------|------|
| `Boolean`            | `Boolean`   | ✅     |      |
| `Byte`               | `Number`    | ✅     |      |
| `Char`               | `String`    | ✅     |      |
| `Int16` (`short`)    | `Number`    | ✅     |      |
| `Int32` (`int`)      | `Number`    | ✅     |      |
| `Int64` (`long`)     | `Number`    | ✅     |      |
| `Int64` (`long`)     | `BigInt`    | ⏳      | [ArcadeMode/TypeShim#15](https://github.com/ArcadeMode/TypeShim/issues/15) |
| `Single` (`float`)   | `Number`    | ✅     |      |
| `Double` (`double`)  | `Number`    | ✅     |      |
| `IntPtr` (`nint`)    | `Number`    | ✅     |      |
| `DateTime`           | `Date`      | ✅     |      |
| `DateTimeOffset`     | `Date`      | ✅     |      |
| `Exception`          | `Error`     | ✅     |      |
| `JSObject`           | `Object`    | ✅     | Requires manual JSObject handling |
| `String`             | `String`    | ✅     |      |
| `T[]`                | `T[]`       | ✅     | * [Only supported .NET types](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings) |
| `Span<Byte>`         | `MemoryView`| ✅     |      |
| `Span<Int32>`        | `MemoryView`| ✅     |      |
| `Span<Double>`       | `MemoryView`| ✅     |      |
| `ArraySegment<Byte>` | `MemoryView`| ✅     |      |
| `ArraySegment<Int32>`| `MemoryView`| ✅     |      |
| `ArraySegment<Double>`| `MemoryView`| ✅    |      |
| `Task`               | `Promise`   | ✅     | * [Only supported .NET types](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings) |
| `Action`             | `Function`  | ✅     |      |
| `Action<T1>`         | `Function`  | ✅     |      |
| `Action<T1, T2>`     | `Function`  | ✅     |      |
| `Action<T1, T2, T3>` | `Function`  | ✅     |      |
| `Func<TResult>`      | `Function`  | ✅     |      |
| `Func<T1, TResult>`  | `Function`  | ✅     |      |
| `Func<T1, T2, TResult>` | `Function`| ✅   |      |
| `Func<T1, T2, T3, TResult>` | `Function` | ✅ |      |

Table 2. TypeShim support for .NET-JS interop types

*<sub>For `[TSExport]` classes</sub>

## <a name="installing"></a>Installing

To use TypeShim all you have to do is install it directly into your `Microsoft.NET.Sdk.WebAssembly`-powered project. Check the [configuration](#configuration) section for configuration you might want to adjust to your project.

```
nuget install TypeShim
```

## <a name="configuration"></a>Configuration

TypeShim is configured through MSBuild properties, you may provide these through your `.csproj` file or from the `msbuild`/`dotnet` cli. 

| Name                               | Default     | Description                                                                                                                               | Example / Options                 |
|-------------------------------------|-------------|-------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------|
| `TypeShim_TypeScriptOutputDirectory`| `"wwwroot"` | Directory path (relative to `OutDir`) where `typeshim.ts` is generated. Supports relative paths.                                          | `../../myfrontend`                |
| `TypeShim_TypeScriptOutputFileName` | `"typeshim.ts"` | Filename of the generated TypeShim TypeScript code.                                                                                       | `typeshim.ts`                     |
| `TypeShim_GeneratedDir`             | `TypeShim`  | Directory path (relative to `IntermediateOutputPath`) for generated `YourClass.Interop.g.cs` files.                                       | `TypeShim`                        |
| `TypeShim_MSBuildMessagePriority`   | `Normal`    | MSBuild message priority. Set to High for debugging.                                                                                      | `Low`, `Normal`, `High`           |

Table 3. Configuration options

### <a name="limitations"></a>Limitations

TSExports are subject to minimal, but some, constraints. 
- Certain types are not supported by either TypeShim or .NET wasm type marshalling. Analyzers have been implemented to notify of such cases.
- As overloading is not a real language feature in JavaScript nor TypeScript, this is currently not supported in TypeShim either. You can still define overloads that are not public. This goes for both constructors and methods.
- By default, JSExport yields value semantics for Array instances, this is one reference type that is atypical. It is under consideration to be addressed but an effective alternative is to define your own List class to preserve reference semantics.
- Classes with generic type parameters can not be part of interop codegen at this time.

### <a name="performance"></a>Performance

TypeShim has been optimized to achieve average codegen times of ~1 ms per class in a set of benchmarks going up to 200 classes. By optimizing the implementation and providing NativeAOT builds via the NuGet package, most users should see end-to-end codegen times of roughly 50–200 ms for projects with 25–200 classes. Every PR validates both AOT and JIT performance to help maintain these numbers.

Performance is prioritized to minimize build-time impact and deliver the best possible experience for TypeShim users. Secondly it was a good excuse to play around with profiling tools and get some hands on experience with performance optimization and NativeAOT. 

The earlier versions of TypeShim used regular JIT builds which suffered expensive runtime start times and an inability to warm-up so even smaller projects would require more than 1 second for codegen. Switching to NativeAOT brought this down to the quarterisecond range and after several optimizations has been reduced to below a tenth of a second in many cases.

Results from the continuous benchmarking that is now part of every pull request are shown in Table 4. The 0 classes case demonstrates the overhead of starting the process without doing any work.

| Method   | Compilation | ClassCount | Mean        | Error     | StdDev    |
|--------- |------------ |-----------:|------------:|----------:|----------:|
| **Generate** | **AOT**         | **0**          |    **14.02 ms** |  **1.319 ms** |  **0.873 ms** |
| **Generate** | **AOT**         | **1**          |    **31.35 ms** |  **0.969 ms** |  **0.641 ms** |
| **Generate** | **AOT**         | **10**         |    **31.82 ms** |  **1.683 ms** |  **1.113 ms** |
| **Generate** | **AOT**         | **25**         |    **45.32 ms** |  **1.565 ms** |  **1.035 ms** |
| **Generate** | **AOT**         | **50**         |    **56.50 ms** |  **1.103 ms** |  **0.730 ms** |
| **Generate** | **AOT**         | **100**        |    **91.60 ms** |  **2.294 ms** |  **1.517 ms** |
| **Generate** | **AOT**         | **200**        |    **93.92 ms** |  **1.553 ms** |  **1.027 ms** |
| **Generate** | **JIT**         | **0**          |    **42.07 ms** |  **0.687 ms** |  **0.454 ms** |
| **Generate** | **JIT**         | **1**          |   **813.62 ms** | **10.321 ms** |  **6.827 ms** |
| **Generate** | **JIT**         | **10**         |   **814.93 ms** |  **9.107 ms** |  **6.024 ms** |
| **Generate** | **JIT**         | **25**         |   **862.08 ms** | **11.549 ms** |  **7.639 ms** |
| **Generate** | **JIT**         | **50**         |   **900.00 ms** | **14.144 ms** |  **9.355 ms** |
| **Generate** | **JIT**         | **100**        | **1,014.10 ms** | **12.046 ms** |  **7.968 ms** |
| **Generate** | **JIT**         | **200**        |   **986.96 ms** | **22.021 ms** | **14.565 ms** |

Table 4. Benchmark results on an AMD EPYC 7763 2.45GHz Github Actions runner.

## Contributing

Contributions are welcome.
- Please discuss proposals in an issue before submitting changes.
- Bugfixes should come with at least one test demonstrating the issue and its resolution.
- New features should come with unit- and E2E tests to demonstrate their correctness.
- PRs should be made from a fork.

---

> Got ideas, found a bug or have an idea for a new feature? Feel free to open a discussion or an issue!
