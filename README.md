<h1 align=center tabindex=-1>TypeShim - Typesafe .NET ↔︎ TypeScript interop</h1>
<p align=center tabindex=-1>
  <i>Your .NET classes, transparently accessible from TypeScript</i>
</p>

## Why TypeShim
.NET on WebAssembly is an awesome technology and the [JSImport/JSExport API](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0) has some solid primitives to build on top of. Sadly it lacks type information and instance member access. These limitations were what gave rise to the idea of TypeShim.

TypeShim sets out to provide an richer functionality by leveraging code generation with the goal of making the JSImport/JSExport API easy to work with ánd provide type information in TypeScript.

## At a glance
- Automated JSExport interop code generation for your convenience
- Extended type marshalling 
    - TypeShim shims _your_ classes to the TypeScript world.
- Compile time type-safety across the interop boundary 
- Natural [POCO](https://en.wikipedia.org/wiki/Plain_old_CLR_object) member access **in TypeScript**.
    - JSExport offers only static method access. 
    - TypeShim extends this with:
        - instance method access. ✅
        - instance property access. 🚧
        - static property access. 🚧
- `[JSExport]`/`[JSImport]` work fine in conjunction with TypeShim.
    - If you need customization, TypeShim won't get in your way
- TypeShim is compatible with both Wasm publish modes: Selfcontained or JS bundled.

## Check the sample

TODO: brief explanation how to run the sample.



## Feature: instance member access from TypeScript

Samples below demonstrate the same operations when interfacing with TypeShim generated code vs `JSExport` generated code. Either way you will load your exports as [described in the docs](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/wasm-browser-app?view=aspnetcore-10.0#javascript-interop-on-). 

**TypeShim**
 ```js
    public UsingTypeShim(exports: WasmModuleExports) {
        const module = new WasmModule(exports)
        const repository: PersonRepository = module.PersonRepository().GetInstance(); 
        const person: Person = repository.GetPerson();
        console.log(person.GetName()); // prints "Alice"
        person.SetName("Bob");
        console.log(person.GetName()); // prints "Bob"
    }
```
**Raw JSExport**
  ```js
    public UsingRawJSExport(exports: any) {
        const repository: any = exports.MyBusiness.People.PersonRepository.GetInstance(); 
        const person: any = exports.MyBusiness.People.PersonRepository.GetPerson(repository);
        console.log(exports.MyBusiness.People.Person.GetName(person)); // prints "Alice"
        exports.MyBusiness.People.Person.SetName(person, "Bob");
        console.log(exports.MyBusiness.People.Person.GetName(person)); // prints "Bob"
    }
  ```

### Easy opt-in to interop from C#.
TypeShim generates your interop definitions for you at compile time, ensuring up-to-dateness and correctness.

<details>
    <summary>See the <code>TypeShim</code> C# implementation of <code>PersonRepository</code> and <code>Person</code></summary>
&nbsp;

TypeShim preserves your type information across the interop boundary.

  ```csharp
using TypeShim;

namespace MyBusiness.People;

[TsExport]
public class PersonRepository
{
    private static readonly PersonRepository _instance = new();

    internal Person Person = new Person()
    {
        Name = "Alice",
        Age = 28,
    };

    public Person GetPerson()
    {
        return Person;
    }

    public static PersonRepository GetInstance()
    {
        return _instance;
    }
}

[TsExport]
public class Person
{
    internal string Name { get; set; }
    internal int Age { get; set; }
    
    public string GetName()
    {
        return Name;
    }

    public void SetName(string name)
    {
        this.Name = name;
    }
}
```
</details>

<details>
  <summary>See the <code>JSExport</code> C# implementation of <code>PersonRepository</code> and <code>Person</code></summary>
&nbsp;

Note the error sensitivity of passing untyped objects across the interop boundary.

  ```csharp
namespace MyBusiness.People;

public class PersonRepository
{
    private static readonly PersonRepository _instance = new();

    internal Person Person = new Person()
    {
        Name = "Alice",
        Age = 28
    };

    [JSExport]
    [return: JSMarshalAsType<JSType.Object>]
    public static object GetPerson([JSMarshalAsType<JSType.Object>] object repository)
    {
        PersonRepository pr = (PersonRepository)repository;
        return pr.Person;
    }

    [JSExport]
    [return: JSMarshalAsType<JSType.Object>]
    public static object GetInstance()
    {
        return _instance;
    }
}

[TsExport]
public class Person
{
    internal string Name { get; set; }
    internal int Age { get; set; }
    
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
}
```
</details>

## Feature: Enriched Type support

TypeShim adds functionality to bring your classes over the interop boundary, as TypeScript interfaces with matching signatures. It also brings all [types marshalled by .NET](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings) to TypeScript. This work is largely completed, but some types are still on the roadmap for support.  Support for generics is limited to `Task` and `[]`. 

Every supported type can be used in methods as return and parameter types. When properties get support, they will inherit the existing supported types automatically.

TypeShim aims to broaden its type support by building on top of the .NET Marshalled types. i.e. `Enum` and `IEnumerable` could be supported by leveraging existing `Int32` and `[]` marshalling combined with some generated shimming code. 

| TypeShim Shimmed Type | Mapped Type | Support | Note |
|----------------------|-------------|--------|------|
| `TClass`                  |  `TClass`        | ✅     | `TClass`-shim generated in TypeScript* |
| `Task<TClass>`            | `Promise<TClass>`| ✅     | `TClass`-shim generated in TypeScript* |
| `TClass[]`                | `TClass[]`       | ✅     | `TClass`-shim generated in TypeScript* |
| `JSObject`           | `TClass`         | 💡     | [ArcadeMode/TypeShim#4](https://github.com/ArcadeMode/TypeShim/issues/4) (TS → C# only) |
| `TEnum`      | `TEnum`       | 💡     | under consideration |
| `IEnumerable<T>`     | `T[]`       | 💡     | under consideration |
| `Dictionary<TKey, TValue>` | `?`     | 💡     | under consideration |

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

*<sub>For `[TSExport]` annotated classes</sub>


## Installing

To use TypeShim all you have to do is install it directly into your `Microsoft.NET.Sdk.WebAssembly`-powered project.
```
nuget install TypeShim
```

## Configuration

TypeShim is configured through MSBuild properties, you may provide these through your `.csproj` file or from the `msbuild`/`dotnet` cli. 

| Name                               | Default     | Description                                                                                                                               | Example / Options                 |
|-------------------------------------|-------------|-------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------|
| `TypeShim_TypeScriptOutputDirectory`| `"wwwroot"` | Directory path (relative to `OutDir`) where `typeshim.ts` is generated. Supports relative paths.                                          | `../../myfrontend`                |
| `TypeShim_TypeScriptOutputFileName` | `"typeshim.ts"` | Filename of the generated TypeShim TypeScript code.                                                                                       | `typeshim.ts`                     |
| `TypeShim_GeneratedDir`             | `TypeShim`  | Directory path (relative to `IntermediateOutputPath`) for generated `YourClass.Interop.g.cs` files.                                       | `TypeShim`                        |
| `TypeShim_MSBuildMessagePriority`   | `Normal`    | MSBuild message priority. Set to High for debugging.                                                                                      | `Low`, `Normal`, `High`           |

> Got inspired or have ideas? Feel free to open a discussion or an issue!

