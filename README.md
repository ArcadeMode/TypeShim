<h1 align=center tabindex=-1>TypeShim - Seamless .NET <> TypeScript interop</h1>
<p align=center tabindex=-1>
  <i>Your .NET classes, transparently accessible in TypeScript</i>
</p>

## Why TypeShim
.NET on WebAssembly is an awesome technology but the [JSImport/JSExport API](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0) can be somewhat cumbersome to work with. TypeShim sets out to provide an opinionated and easy to use wrapper for this API to allow developers to remain focussed on delivering value to their users. With TypeShim it will be a breeze to bring WASM components to your TypeScript-based frontends.

## Features
TypeShim can hide the JSExport API from you, however you are still free to write classes annotated with JSExport. They just dont play nice together so you either roll your own implementation or let TypeShim do the work for you. Besides generating your JSExport class for you, TypeShim adds the TypeScript side of things for you as well.

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

```

On the typescript end you simply retrieve the `exports` as [described in the docs](https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/wasm-browser-app?view=aspnetcore-10.0#javascript-interop-on-). However in your projects output you will find a generated `typeshim.ts`.

#### Interact with your .NET class instance from TypeScript, completely naturally.

```typescript
import { WasmModuleExports, WasmModule, PersonRepository, Person, Dog } from 'path/to/Wasm.Project/publish/wwwroot/typeshim';

class MyCoolUIApp {
  public DoInteropStuff(exports: any) {
  const module = new WasmModule((WasmModuleExports)exports)

  // the static 'GetInstance' method retrieves our first object instance from the dotnet side.
  const repository: PersonRepository = module.PersonRepository().GetInstance(); 

  // from here on out we can call instance members 'as usual'
  const person: Person = repository.GetPerson();
  console.log("Before:", person.GetName()); // prints "Alice"
  person.SetName("Bob");
  console.log("After:", person.GetName()); // prints "Bob"

  const pet: Dog = person.GetPet();
  console.log("pet.GetName()", pet.GetName()); // prints "Fido"
  }
}
```

#### TypeShim generates your module definition
Types like `WasmModule` are generated automatically to bring you your `static` methods as a first-line of access to your C# classes. Then you receive your very recognizable `PersonRepository` which behaves just like a real C# object. Instance method invocations on the typescript instances call into your .NET class's instance methods completely transparently.

## Installing
**TODO**: nuget, options for msbuild props.

## Roadmap
This project is still in its early stages but there are a few short- and longterm goals right now

### JSObjects support for JS > .NET calls
To create an object on the JS side and pass it to the .NET side, we'll need to write a generator for the object mapping logic. This sounds like low hanging fruit and would improve usability a lot.

### Properties
Auto-generating the C# properties on the TS side would make the ability to . Albeit they should probably be used sparingly given the performance implications of making large amounts of interop calls.

### Larger object transfer
Currently we support no properties at all, and fetching many properties would require many interop calls, which is undesirable. By introducing some kind of readonly class annotation we could make it simple to move larger objects over the interop boundary without having to write many mappers by hand. This would require some performance testing to see how usable it is for real world use.

> The MemoryView provides opportunities for zero copy memory access across the interop boundary. We could use this to serialize data and read from it on the JS _or_ .NET side. This could be as simple as json serialization or more advanced protobuf-like implementations could very well be possible.

### JSImports
Longer term goal, but being able to take specified TS classes as input for C# code generation would bring the JS world significantly closer to the .NET side as well. 

> Got inspired or have ideas? Feel free to open a discussion or an issue!

