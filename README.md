<h1 align=center tabindex=-1>TypeShim</h1>
<p align=center tabindex=-1>
  <i>Generated .NET-JS interop facades from C# to TypeScript.</i>
</p>

## Why TypeShim
The [JSImport/JSExport API](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0), the backbone of [.NET Webassembly applications](https://github.com/dotnet/runtime/blob/74cf618d63c3d092eb91a9bb00ba8152cc2dfc76/src/mono/wasm/features.md), while powerful, lacks type information and exclusively supports static methods. It requires repetitive code patterns for type transformation and quite some boilerplate to achieve reasonable ergonomics in your code.

Enter: _TypeShim_. Drop a `[TSExport]`/`[TSModule]` on your C# class and _voilà_, TypeShim generates a set of JSExport methods and TypeScript classes to access your .NET class as if it were truely exported to TypeScript.

## Features at a glance

- [Enriched type marshalling](#enriched-type-support) 
    - _Your_ classes accessible from TypeScript.
    - Code generation ensures Type-safety across the interop boundary.
- Compile-time generated TypeScript shims
    - Your classes including _all_ their public members.
        - Static methods _and properties_ 
        - _Instance methods and properties_
    - Powerful state locality semantics 🚧
        - Proxies to access your instances in .NET
        - Snapshots to create or materialize instances in JS.
        - Flexible, Proxies/Snapshots can be used interchangeably with the interop facade.
- Compile-time generated C# shims
    - JSExport/JSImport tailored to work with the TypeScript shims
    - Repetitive type conversions are generated automatically 
    - JSObjects get mapped to your C# classes
s automatically.  
- Lightweight: won't interfere with existing JSExport/JSImports.
- Minimal setup: just [NuGet install](#installing) and add one attribute to your class.

## Feature: instance member access from TypeScript

Samples below demonstrate the same operations when interfacing with TypeShim generated code vs `JSExport` generated code. Either way you will load your wasm browserapp as [described in the docs](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/wasm-browser-app?view=aspnetcore-10.0#javascript-interop-on-) in order to retrieve its `exports`. 


### TypeShim
Preservation of type information across the interop boundary, including instance method and property access.

<details>
    <summary>See the <code>TypeShim</code> C# implementation of <code>PeopleRepository</code> and <code>Person</code></summary>
&nbsp;

  ```csharp
using TypeShim;

namespace Sample.People;

[TsModule]
public static class PeopleModule
{
    public static PeopleRepository { get; internal set; } = new PeopleRepository();
}

[TsExport]
public class PeopleRepository
{
    internal List<Person> People = [
        new Person()
        {
            Name = "Alice",
            Age = 26,
        },
        new Person()
        {
            Name = "Bob",
            Age = 29,
        }
    ];

    public Person GetPerson(int i)
    {
        return People[i];
    }
}

[TsExport]
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
</details>

 ```js
public UsingTypeShim(exports: AssemblyExports) {
    const module = new PeopleModule(exports)
    const alice: Person = module.PeopleRepository.GetPerson(0);
    const bob: Person = module.PeopleRepository.GetPerson(1);
    console.log(alice.Name, bob.Name); // prints "Alice", "Bob"
    console.log(alice.IsOlderThan(bob)) // prints false
    alice.Age = 30;
    console.log(alice.IsOlderThan(bob)) // prints true
}
```

### 'Raw' JSExport
The exact same behavior as the TypeShim sample, with handwritten JSExport.

<details>
  <summary>See the <code>JSExport</code> C# implementation of <code>PeopleRepository</code> and <code>Person</code></summary>
&nbsp;

Note the error sensitivity of passing untyped objects across the interop boundary.

  ```csharp
namespace Sample.People;

public class PeopleModule 
{
    private static readonly PersonRepository _instance = new();
    [JSExport]
    [return: JSMarshalAsType<JSType.Object>]
    public static object GetPeopleRepository()
    {
        return _instance;
    }
}

public class PeopleRepository
{
    internal List<Person> People = [
        new Person()
        {
            Name = "Alice",
            Age = 26,
        },
        new Person()
        {
            Name = "Bob",
            Age = 29,
        }
    ];

    [JSExport]
    [return: JSMarshalAsType<JSType.Object>]
    public static object GetPerson([JSMarshalAsType<JSType.Object>] object repository, [JSMarshalAsType<JSType.Number>] int i)
    {
        PersonRepository pr = (PersonRepository)repository;
        return pr.People[i];
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    
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
        return p.Name = name;
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
        return p.Age = age;
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

```js
public UsingRawJSExport(exports: any) {
    const repository: any = exports.Sample.People.PeopleModule.GetPeopleRepository(); 
    const alice: any = exports.Sample.People.PeopleRepository.GetPerson(repository, 0);
    const bob: any = exports.Sample.People.PeopleRepository.GetPerson(repository, 1);
    console.log(exports.Sample.People.Person.GetName(alice), exports.Sample.People.Person.GetName(bob)); // prints "Alice", "Bob"
    console.log(exports.Sample.People.Person.IsOlderThan(alice, bob)); // prints false
    exports.Sample.People.Person.SetAge(alice, 30);
    console.log(exports.Sample.People.Person.IsOlderThan(alice, bob)); // prints true
}
```

## <a name="enriched-type-support"></a> Feature: Enriched Type support

TypeShim enriches the supported types by JSExport by adding _your_ classes to the [types marshalled by .NET](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings). Repetitive patterns for type transformation and higher order types that you'd have to lower into the supported types yourself are readily supported and tested in TypeShim.

Ofcourse, TypeShim brings all [types marshalled by .NET](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings) to TypeScript. This work is largely completed, but some types are still on the roadmap for support.  Support for generics is limited to `Task` and `[]`. Every supported type can be used in methods as return and parameter types, they are also supported as property types. 

> TypeShim and JSExport/JSImport are perfectly usable side-by-side, in case you want to handroll parts of your interop.

TypeShim aims to continue to broaden its type support in order to improve the developer experience of .NET Wasm browser apps. Notably `Task<int[]>` generates compiler error's with JSExport but is within reach to support in TypeShim. Other commonly used types include `Enum` and `IEnumerable`. 

| TypeShim Shimmed Type | Mapped Type | Support | Note |
|----------------------|-------------|--------|------|
| `TClass`                  |  `TClass`        | ✅     | `TClass` generated in TypeScript* |
| `Task<TClass>`            | `Promise<TClass>`| ✅     | `TClass` generated in TypeScript* |
| `Task<T[]>`            | `Promise<T[]>`| 💡     | under consideration (for all array-compatible `T`) |
| `TClass[]`                | `TClass[]`       | ✅     | `TClass` generated in TypeScript* |
| `JSObject`           | `TClass`         | 💡     | [ArcadeMode/TypeShim#4](https://github.com/ArcadeMode/TypeShim/issues/4) (TS → C# only) |
| `TEnum`      | `TEnum`       | 💡     | under consideration |
| `IEnumerable<T>`     | `T[]`       | 💡     | under consideration |
| `Dictionary<TKey, TValue>` | `?`     | 💡     | under consideration |
| `(T1, T2)` | `[T1, T2]`     | 💡     | under consideration |

| .NET Marshalled Type                  | Mapped Type | Support | Note |
|----------------------|-------------|--------|------|
| `Boolean`            | `Boolean`   | ✅     |      |
| `Byte`               | `Number`    | ✅     |      |
| `Char`               | `String`    | ✅     |      |
| `Int16` (`short`)    | `Number`    | ✅     |      |
| `Int32` (`int`)      | `Number`    | ✅     |      |
| `Int64` (`long`)     | `Number`    | ✅     |      |
| `Int64` (`long`)     | `BigInt`    | ⏳     | [ArcadeMode/TypeShim#15](https://github.com/ArcadeMode/TypeShim/issues/15) |
| `Single` (`float`)   | `Number`    | ✅     |      |
| `Double` (`double`)  | `Number`    | ✅     |      |
| `IntPtr` (`nint`)    | `Number`    | ✅     |      |
| `DateTime`           | `Date`      | ✅     |      |
| `DateTimeOffset`     | `Date`      | ✅     |      |
| `Exception`          | `Error`     | ✅     |      |
| `JSObject`           | `Object`    | ✅     | You must process the JSObject manually |
| `String`             | `String`    | ✅     |      |
| `Object` (`object`)  | `Any`       | ✅     |      |
| `T[]`         | `T[]`| ✅     | * [Only supported .NET types](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings) |
| `Span<Byte>`         | `MemoryView`| 🚧     |      |
| `Span<Int32>`        | `MemoryView`| 🚧     |      |
| `Span<Double>`       | `MemoryView`| 🚧     |      |
| `ArraySegment<Byte>` | `MemoryView`| 🚧     |      |
| `ArraySegment<Int32>`| `MemoryView`| 🚧     |      |
| `ArraySegment<Double>`| `MemoryView`| 🚧    |      |
| `Task`               | `Promise`   | ✅     | * [Only supported .NET types](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings) |
| `Action`             | `Function`  | 🚧     |      |
| `Action<T1>`         | `Function`  | 🚧     |      |
| `Action<T1, T2>`     | `Function`  | 🚧     |      |
| `Action<T1, T2, T3>` | `Function`  | 🚧     |      |
| `Func<TResult>`      | `Function`  | 🚧     |      |
| `Func<T1, TResult>`  | `Function`  | 🚧     |      |
| `Func<T1, T2, TResult>` | `Function`| 🚧   |      |
| `Func<T1, T2, T3, TResult>` | `Function` | 🚧 |      |

*<sub>For `[TSExport]`/`[TSModule]` classes</sub>

## Run the sample

To build and run the project:
```
cd Sample/TypeShim.Sample.Client && npm install && npm run build && cd ../TypeShim.Sample.Server && dotnet run
```
The app should be available on [http://localhost:5012](http://localhost:5012)

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

## Contributing

TODO_CONTRIBUTING

---

> Got ideas, found a bug or want more features? Feel free to open a discussion or an issue!

