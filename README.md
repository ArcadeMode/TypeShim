<h1 align=center tabindex=-1>TypeShim</h1>
<p align=center tabindex=-1>
  <i>Strongly-typed .NET-JS interop facade generation</i>
</p>

## Why TypeShim
The [JSImport/JSExport API](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0), the backbone of [.NET Webassembly applications](https://github.com/dotnet/runtime/blob/74cf618d63c3d092eb91a9bb00ba8152cc2dfc76/src/mono/wasm/features.md), while powerful, lacks type information and exclusively supports static methods. It requires repetitive code patterns for type transformation and quite some boilerplate to achieve reasonable ergonomics in your code.

Enter: _TypeShim_. Drop a `[TSExport]` on your C# class and _voilà_, TypeShim generates a set of JSExport methods which are neatly wrapped by TypeScript classes to provide access to your .NET class as if it were truly exported to TypeScript.

## Features at a glance

- 🏭 Generated for _your_ project, recognizable class names in TypeScript.
- 💰 [Enriched type marshalling](#enriched-type-support) .
- 🛡 Type-safety across the interop boundary.
- 🤖 Automatically export:
  - Constructors.
  - Static ánd instance methods.
  - Static ánd instance properties.
- 🦜 Implements repetitive interop patterns for you.
- 🪁 Lightweight: won't lock you in or interfere with other JSExport/JSImport.
- 👍 Minimal setup: [NuGet install](#installing) + one `[TSExport]`.

## Samples
Samples below demonstrate the same operations when interfacing with TypeShim generated code vs `JSExport` generated code. Either way you will load your wasm browserapp as [described in the docs](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/wasm-browser-app?view=aspnetcore-10.0#javascript-interop-on-) in order to retrieve its `exports`. 


### TypeShim
Preservation of type information across the interop boundary, including instance method and property access.

<details>
    <summary>See the <code>TypeShim</code> C# implementation of <code>PeopleRepository</code> and <code>Person</code></summary>
&nbsp;

  ```csharp
using TypeShim;

namespace Sample.People;

[TsExport]
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
using { TypeShimInitializer, PeopleRepository, Person } from './typeshim.ts';

public UsingTypeShim() {
    const runtime = await dotnet.withApplicationArguments(args).create()
    TypeShimInitializer.initialize(runtime);

    const repository = new PeopleRepository.Proxy();
    const alice: Person.Proxy = repository.GetPerson(0);
    const bob = new Person.Proxy({
      Name: 'Bob',
      Age: 20
    });

    console.log(alice.Name, bob.Name); // prints "Alice", "Bob"
    console.log(alice.IsOlderThan(bob)) // prints false
    alice.Age = 30;
    console.log(alice.IsOlderThan(bob)) // prints true

    repository.AddPerson({ Name: "Charlie", Age: 40 });
    const charlie: Person.Proxy = repository.GetPerson(1);
    console.log(alice.IsOlderThan(charlie)) // prints false
    console.log(bob.IsOlderThan(charlie)) // prints true
}
```

### 'Raw' JSExport
The same behavior as the TypeShim sample, with handwritten JSExport. Certain parts enabled by TypeShim have not been replicated as the point may be clear at a glance: this is a large amount of difficult to maintain boilerplate if you have to write it yourself.

<details>
  <summary>See the <code>JSExport</code> C# implementation of <code>PeopleRepository</code> and <code>Person</code></summary>
&nbsp;

Note the error sensitivity of passing untyped objects across the interop boundary.

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

    private static readonly PersonRepository _instance = new();
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
    const runtime = await dotnet.withApplicationArguments(args).create();
    const exports = runtime.assemblyExports;

    const repository: any = exports.Sample.People.PeopleRepository.GetInstance(); 
    const alice: any = exports.Sample.People.PeopleRepository.GetPerson(repository, 0);
    const bob: any = exports.Sample.People.People.ConstructPerson("Bob", 20);
    console.log(exports.Sample.People.Person.GetName(alice), exports.Sample.People.Person.GetName(bob)); // prints "Alice", "Bob"
    console.log(exports.Sample.People.Person.IsOlderThan(alice, bob)); // prints false
    exports.Sample.People.Person.SetAge(alice, 30);
    console.log(exports.Sample.People.Person.IsOlderThan(alice, bob)); // prints true
}
```

## Semantically rich TypeScript interop

TypeShim makes your C# classes accessible from TypeScript, with some powerful features built in so you can take control over your classes ánd data locality. First, you will be using `[TSExport]` annotate your classes, then controlling which members are public to define your interop API. Any class annotated with the TSExportAttribute will receive a TypeScript counterpart which includes the public members you have chosen. 

The build-time generated TypeScript can provide the following subcomponents for each exported class `MyClass`:

### Proxies (`MyClass`)
`MyClass` grants access to the exported C# `MyClass` class _in a proxying capacity_, this type will also be referred to as a `Proxy`. A proxy only contains _public_ members and a dotnet instance of the class being proxied _always_ lives in the dotnet runtime. To aquire an instance you may invoke your exported constructor or returned by any method and/or property. Alternatively you can access static members all the same. Proxies may also be used as parameters and will behave as typical reference types when performing any such operation.

### Snapshots (`MyClass.Snapshot`)
The snapshot type is created if your class has public properties. TypeShim provides a utility function `MyClass.materialize(your_instance)` that returns a snapshot. Snapshots are fully decoupled from the dotnet object and live in the JS runtime. This is useful when you no longer require the Proxy instance but want to continue working with its data. Properties of proxy types will be materialized as well.

### Initializers (`MyClass.Initializer`)
The `Initializer` type is created if the exported class has an exported constructor and accepts an initializer body in `new()` expressions. Initializer objects live in the JS runtime and may be used in the process of creating dotnet object instances, if it exists it will be a parameter in the constructor of the associated Proxy.

Additionally, _if the class exports a parameterless constructor_ then initializer objects can also be passed instead of proxies in method parameters, property setters and even in other initializer objects. TypeShim will construct the appropriate dotnet class instance(s) from the initializer. Initializer's can even contain properties of Proxy type instead of an Initializer if you want to reference an existing object. Below a brief demonstration of the provided flexibility.

> Arrays of mixed proxies and initializers are supported. The contained initializers will be constructed into new dotnet class instances while the object references behind the proxies are preserved.

<table style="width:100%">
<tr>
<td>

```ts
const bike = new Bike("Ducati", { 
  Cc: 1200,
  Hp: 147
});
const rider = new Rider({
    Name: "Dude",
    Bike: bike
});

```

</td>
<td>

```ts
const bike: Bike.Initializer = {
  Brand: "Ducati" 
  Cc: 1200,
  Hp: 147
};
const rider = new Rider({
    Name: "Dude",
    Bike: bike
});
```
  
</td>
</table>

## <a name="enriched-type-support"></a> Enriched Type support

TypeShim enriches the supported types by JSExport by adding _your_ classes to the [types marshalled by .NET](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings). Repetitive patterns for type transformation and higher order types that you'd have to lower into the supported types yourself are readily supported and tested in TypeShim.

Ofcourse, TypeShim brings all [types marshalled by .NET](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mappings) to TypeScript. This work is largely completed, but some types are still on the roadmap for support.  Support for generics is limited to `Task` and `[]`. Every supported type can be used in methods as return and parameter types, they are also supported as property types. 

> TypeShim and JSExport/JSImport are perfectly usable side-by-side, in case you want to handroll parts of your interop.

TypeShim aims to continue to broaden its type support in order to improve the developer experience of .NET Wasm browser apps. Notably `Task<int[]>` generates compiler error's with JSExport but is within reach to support in TypeShim. Other commonly used types include `Enum` and `IEnumerable`. 

| TypeShim Shimmed Type | Mapped Type | Support | Note |
|----------------------|-------------|--------|------|
| `Object` (`object`)  | `ManagedObject`       | ✅     | a disposable opaque handle |
| `TClass`  | `ManagedObject`       | ✅     |  unexported reference types    |
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
| `JSObject`           | `Object`    | ✅     | Requires manual JSObject handling |
| `String`             | `String`    | ✅     |      |
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

*<sub>For `[TSExport]` classes</sub>

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

### <a name="limitations"></a>Limitations

TSExports are subject to minimal, but some, constraints. 
- Certain types are not supported by either TypeShim or .NET wasm type marshalling. Analyzers have been implemented to notify of such cases.
- As overloading is not a real language feature in JavaScript nor TypeScript, this is currently not supported in TypeShim either. You can still define overloads that are not public. This goes for both constructors and methods.
- By default, JSExport yields value semantics for Array instances, this one reference type that is atypical. It is under consideration to adres but most simply you may define your own List class to preserve reference semantics.


## Contributing

TODO_CONTRIBUTING

---

> Got ideas, found a bug or want more features? Feel free to open a discussion or an issue!

