<h1 align=center tabindex=-1>TypeShim - Seamless .NET <> TypeScript interop</h1>
<p align=center tabindex=-1>
  <i>Your .NET classes, transparently accessible in TypeScript</i>
</p>

## Why TypeShim
.NET on WebAssembly is an awesome technology but the [JSImport/JSExport API](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0) can be somewhat cumbersome to work with. TypeShim sets out to provide an opinionated and easy to use wrapper for this API to allow developers to remain focussed on delivering value to their users. With TypeShim it will be a breeze to bring WASM components to your TypeScript-based frontends.

## Features
TypeShim can hide the JSExport API from you but does not stop your from adding your own JSExport-annotated classes. However, the real good stuff happens on the other end where TypeShim generates the TypeScript side of things for you as well.

#### Export your entire C# class to JS with one attribute, no JSExport or typemarshalling info required.
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
        Pet = new Dog
        {
            Name = "Fido",
            Age = 4
        }
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
    internal Dog Pet { get; set; }
    
    public string GetName()
    {
        return Name;
    }

    public void SetName(string name)
    {
        this.Name = name;
    }

    public Dog GetPet()
    {
        return Pet;
    }
}

[TsExport]
public class Dog
{
    public string Name { get; set; }
    public int Age { get; set; }

    public string GetName()
    {
        return Name;
    }
}

```

#### Natural interaction with your .NET class instance from TypeScript.
On the TypeScript side you simply retrieve your assembly's `exports` as [described in the docs](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/wasm-browser-app?view=aspnetcore-10.0#javascript-interop-on-). As you check out your published Wasm app, you will notice an new file `typeshim.ts`. This contains various types, most of which you will recognize instantly.

```typescript
import { WasmModuleExports, WasmModule, PersonRepository, Person, Dog } from 'path/to/Wasm.Project/publish/wwwroot/typeshim';

class MyCoolUIApp {
  public DoInteropStuff(exports: any) {
    const module = new WasmModule((WasmModuleExports)exports)

    // the static 'GetInstance' method retrieves our first object instance from the dotnet side.
    const repository: PersonRepository = module.PersonRepository().GetInstance(); 

    // from here on out we can call instance members 'as usual'
    const person: Person = repository.GetPerson();

    console.log(person.GetName()); // prints "Alice"
    person.SetName("Bob");
    console.log(person.GetName()); // prints "Bob"

    const person_again: Person = repository.GetPerson();
    console.log(person_again.GetName()); // prints "Bob"

    const pet: Dog = person.GetPet();
    console.log("pet.GetName()", pet.GetName()); // prints "Fido"
  }
}
```

#### TypeShim generates your module definition
Your first point of contact with TypeShim on the TypeScript side is with `WasmModule` which brings you your `static` methods as a first-line of access to your C# classes. Though the module you can access `PersonRepository`'s static method to retrieve a class instance. This instance behaves just like a real C# object, calling an instance method calls into your .NET class's instance methods completely transparently.

## Installing

To use TypeShim all you have to do is install it directly into your `Microsoft.NET.Sdk.WebAssembly`-powered project.
```
nuget install TypeShim
```

- **TODO**: document msbuild props.
- **TODO**: ts import process

## Roadmap
This project is still in its early stages but there are a few short- and longterm goals right now

### JSObjects support for JS > .NET calls
To create an object on the JS side and pass it to the .NET side, we'll need to write a generator for the object mapping logic. This sounds like low hanging fruit and would improve usability a lot.

### Properties
Auto-generating the C# properties on the TS side would be a big usability win. Albeit they should probably be used sparingly given the performance implications of interop calls.

### Larger object transfer
Currently we support no properties at all, and fetching many properties would require many interop calls, which is undesirable. By introducing some kind of readonly class annotation we could make it simple to move larger objects over the interop boundary without having to write many mappers by hand. This would require some performance testing to see how usable it is for real world use.

> The MemoryView provides opportunities for zero copy memory access across the interop boundary. We could use this to serialize data and read from it on the JS _or_ .NET side. This could be as simple as json serialization or more advanced protobuf-like implementations could very well be possible.

### JSImports
Longer term goal, but being able to take specified TS classes as input for C# code generation would bring the JS world significantly closer to the .NET side as well. 

> Got inspired or have ideas? Feel free to open a discussion or an issue!

