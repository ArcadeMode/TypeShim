// Auto-generated TypeScript module exports interface
export interface AssemblyExports {
    TypeShim: {
        Sample: {
            Capabilities: {
                CapabilitiesModuleInterop: CapabilitiesModuleInterop;
                CapabilitiesProviderInterop: CapabilitiesProviderInterop;
                PrimitivesDemoInterop: PrimitivesDemoInterop;
                ArraysDemoInterop: ArraysDemoInterop;
            };
            PeopleInterop: PeopleInterop;
            PersonInterop: PersonInterop;
            DogInterop: DogInterop;
            PeopleProviderInterop: PeopleProviderInterop;
            TypeShimSampleModuleInterop: TypeShimSampleModuleInterop;
        };
    };
}

// Auto-generated TypeShim TSModule class. Source class: TypeShim.Sample.Capabilities.CapabilitiesModule
export class CapabilitiesModule {
  private interop: AssemblyExports;

  constructor(interop: AssemblyExports) {
    this.interop = interop;
  }

  public GetCapabilitiesProvider(): CapabilitiesProvider.Proxy {
    const res = this.interop.TypeShim.Sample.Capabilities.CapabilitiesModuleInterop.GetCapabilitiesProvider();
    return new CapabilitiesProvider.Proxy(res, this.interop);
  }

}

// Auto-generated TypeShim TSModule class. Source class: TypeShim.Sample.TypeShimSampleModule
export class TypeShimSampleModule {
  private interop: AssemblyExports;

  constructor(interop: AssemblyExports) {
    this.interop = interop;
  }

  public get PeopleProvider(): PeopleProvider.Proxy | null {
    const res = this.interop.TypeShim.Sample.TypeShimSampleModuleInterop.get_PeopleProvider();
    return res ? new PeopleProvider.Proxy(res, this.interop) : null;
  }

}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Capabilities.CapabilitiesModule
export interface CapabilitiesModuleInterop {
    GetCapabilitiesProvider(): object;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Capabilities.CapabilitiesProvider
export interface CapabilitiesProviderInterop {
    GetPrimitivesDemo(instance: object, baseString: string): object;
    GetArraysDemo(instance: object): object;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Capabilities.PrimitivesDemo
export interface PrimitivesDemoInterop {
    GetStringLength(instance: object): number;
    ToUpperCase(instance: object): string;
    Concat(instance: object, str1: string, str2: string): string;
    ContainsUpperCase(instance: object): boolean;
    ResetBaseString(instance: object): void;
    MultiplyString(instance: object, times: number): void;
    get_InitialStringProperty(instance: object): string;
    get_StringProperty(instance: object): string;
    set_StringProperty(instance: object, value: string): void;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Capabilities.ArraysDemo
export interface ArraysDemoInterop {
    SumIntArray(instance: object): number;
    AppendToIntArray(instance: object, value: number): void;
    get_IntArrayProperty(instance: object): Array<number>;
    set_IntArrayProperty(instance: object, value: Array<number>): void;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.People
export interface PeopleInterop {
    get_All(instance: object): Array<object>;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Person
export interface PersonInterop {
    IsOlderThan(instance: object, other: object): boolean;
    AdoptPet(instance: object): void;
    get_Id(instance: object): number;
    set_Id(instance: object, value: number): void;
    get_Name(instance: object): string;
    set_Name(instance: object, value: string): void;
    get_Age(instance: object): number;
    set_Age(instance: object, value: number): void;
    get_Pet(instance: object): object | null;
    set_Pet(instance: object, value: object | null): void;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Dog
export interface DogInterop {
    Bark(instance: object): string;
    GetAge(instance: object, asHumanYears: boolean): number;
    get_Name(instance: object): string;
    set_Name(instance: object, value: string): void;
    get_Breed(instance: object): string;
    set_Breed(instance: object, value: string): void;
    get_Age(instance: object): number;
    set_Age(instance: object, value: number): void;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.PeopleProvider
export interface PeopleProviderInterop {
    FetchPeopleAsync(instance: object): Promise<object>;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.TypeShimSampleModule
export interface TypeShimSampleModuleInterop {
    get_PeopleProvider(): object | null;
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.CapabilitiesProvider
export namespace CapabilitiesProvider {
  export class Proxy {
    interop: AssemblyExports;
    instance: object;

    constructor(instance: object, interop: AssemblyExports) {
      this.interop = interop;
      this.instance = instance;
    }

    public GetPrimitivesDemo(baseString: string): PrimitivesDemo.Proxy {
      const res = this.interop.TypeShim.Sample.Capabilities.CapabilitiesProviderInterop.GetPrimitivesDemo(this.instance, baseString);
      return new PrimitivesDemo.Proxy(res, this.interop);
    }

    public GetArraysDemo(): ArraysDemo.Proxy {
      const res = this.interop.TypeShim.Sample.Capabilities.CapabilitiesProviderInterop.GetArraysDemo(this.instance);
      return new ArraysDemo.Proxy(res, this.interop);
    }

  }

  export interface Snapshot {
  }
  export function snapshot(proxy: CapabilitiesProvider.Proxy): CapabilitiesProvider.Snapshot {
    return {
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.PrimitivesDemo
export namespace PrimitivesDemo {
  export class Proxy {
    interop: AssemblyExports;
    instance: object;

    constructor(instance: object, interop: AssemblyExports) {
      this.interop = interop;
      this.instance = instance;
    }

    public GetStringLength(): number {
      return this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.GetStringLength(this.instance);
    }

    public ToUpperCase(): string {
      return this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ToUpperCase(this.instance);
    }

    public Concat(str1: string, str2: string): string {
      return this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.Concat(this.instance, str1, str2);
    }

    public ContainsUpperCase(): boolean {
      return this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ContainsUpperCase(this.instance);
    }

    public ResetBaseString(): void {
      this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ResetBaseString(this.instance);
    }

    public MultiplyString(times: number): void {
      this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.MultiplyString(this.instance, times);
    }

    public get InitialStringProperty(): string {
      return this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.get_InitialStringProperty(this.instance);
    }

    public get StringProperty(): string {
      return this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.get_StringProperty(this.instance);
    }

    public set StringProperty(value: string) {
      this.interop.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.set_StringProperty(this.instance, value);
    }

  }

  export interface Snapshot {
    InitialStringProperty: string;
    StringProperty: string;
  }
  export function snapshot(proxy: PrimitivesDemo.Proxy): PrimitivesDemo.Snapshot {
    return {
      InitialStringProperty: proxy.InitialStringProperty,
      StringProperty: proxy.StringProperty,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.ArraysDemo
export namespace ArraysDemo {
  export class Proxy {
    interop: AssemblyExports;
    instance: object;

    constructor(instance: object, interop: AssemblyExports) {
      this.interop = interop;
      this.instance = instance;
    }

    public SumIntArray(): number {
      return this.interop.TypeShim.Sample.Capabilities.ArraysDemoInterop.SumIntArray(this.instance);
    }

    public AppendToIntArray(value: number): void {
      this.interop.TypeShim.Sample.Capabilities.ArraysDemoInterop.AppendToIntArray(this.instance, value);
    }

    public get IntArrayProperty(): Array<number> {
      return this.interop.TypeShim.Sample.Capabilities.ArraysDemoInterop.get_IntArrayProperty(this.instance);
    }

    public set IntArrayProperty(value: Array<number>) {
      this.interop.TypeShim.Sample.Capabilities.ArraysDemoInterop.set_IntArrayProperty(this.instance, value);
    }

  }

  export interface Snapshot {
    IntArrayProperty: Array<number>;
  }
  export function snapshot(proxy: ArraysDemo.Proxy): ArraysDemo.Snapshot {
    return {
      IntArrayProperty: proxy.IntArrayProperty.map(item => item),
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.People
export namespace People {
  export class Proxy {
    interop: AssemblyExports;
    instance: object;

    constructor(instance: object, interop: AssemblyExports) {
      this.interop = interop;
      this.instance = instance;
    }

    public get All(): Array<Person.Proxy> {
      const res = this.interop.TypeShim.Sample.PeopleInterop.get_All(this.instance);
      return res.map(item => new Person.Proxy(item, this.interop));
    }

  }

  export interface Snapshot {
    All: Array<Person.Snapshot>;
  }
  export function snapshot(proxy: People.Proxy): People.Snapshot {
    return {
      All: proxy.All.map(item => Person.snapshot(item)),
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Person
export namespace Person {
  export class Proxy {
    interop: AssemblyExports;
    instance: object;

    constructor(instance: object, interop: AssemblyExports) {
      this.interop = interop;
      this.instance = instance;
    }

    public IsOlderThan(other: Person.Proxy): boolean {
      const otherInstance = other instanceof Person.Proxy ? other.instance : other;
      return this.interop.TypeShim.Sample.PersonInterop.IsOlderThan(this.instance, otherInstance);
    }

    public AdoptPet(): void {
      this.interop.TypeShim.Sample.PersonInterop.AdoptPet(this.instance);
    }

    public get Id(): number {
      return this.interop.TypeShim.Sample.PersonInterop.get_Id(this.instance);
    }

    public set Id(value: number) {
      this.interop.TypeShim.Sample.PersonInterop.set_Id(this.instance, value);
    }

    public get Name(): string {
      return this.interop.TypeShim.Sample.PersonInterop.get_Name(this.instance);
    }

    public set Name(value: string) {
      this.interop.TypeShim.Sample.PersonInterop.set_Name(this.instance, value);
    }

    public get Age(): number {
      return this.interop.TypeShim.Sample.PersonInterop.get_Age(this.instance);
    }

    public set Age(value: number) {
      this.interop.TypeShim.Sample.PersonInterop.set_Age(this.instance, value);
    }

    public get Pet(): Dog.Proxy | null {
      const res = this.interop.TypeShim.Sample.PersonInterop.get_Pet(this.instance);
      return res ? new Dog.Proxy(res, this.interop) : null;
    }

    public set Pet(value: Dog.Proxy | null) {
      const valueInstance = value instanceof Dog.Proxy ? value.instance : value;
      this.interop.TypeShim.Sample.PersonInterop.set_Pet(this.instance, valueInstance);
    }

  }

  export interface Snapshot {
    Id: number;
    Name: string;
    Age: number;
    Pet: Dog.Snapshot | null;
  }
  export function snapshot(proxy: Person.Proxy): Person.Snapshot {
    return {
      Id: proxy.Id,
      Name: proxy.Name,
      Age: proxy.Age,
      Pet: proxy.Pet ? Dog.snapshot(proxy.Pet) : null,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Dog
export namespace Dog {
  export class Proxy {
    interop: AssemblyExports;
    instance: object;

    constructor(instance: object, interop: AssemblyExports) {
      this.interop = interop;
      this.instance = instance;
    }

    public Bark(): string {
      return this.interop.TypeShim.Sample.DogInterop.Bark(this.instance);
    }

    public GetAge(asHumanYears: boolean): number {
      return this.interop.TypeShim.Sample.DogInterop.GetAge(this.instance, asHumanYears);
    }

    public get Name(): string {
      return this.interop.TypeShim.Sample.DogInterop.get_Name(this.instance);
    }

    public set Name(value: string) {
      this.interop.TypeShim.Sample.DogInterop.set_Name(this.instance, value);
    }

    public get Breed(): string {
      return this.interop.TypeShim.Sample.DogInterop.get_Breed(this.instance);
    }

    public set Breed(value: string) {
      this.interop.TypeShim.Sample.DogInterop.set_Breed(this.instance, value);
    }

    public get Age(): number {
      return this.interop.TypeShim.Sample.DogInterop.get_Age(this.instance);
    }

    public set Age(value: number) {
      this.interop.TypeShim.Sample.DogInterop.set_Age(this.instance, value);
    }

  }

  export interface Snapshot {
    Name: string;
    Breed: string;
    Age: number;
  }
  export function snapshot(proxy: Dog.Proxy): Dog.Snapshot {
    return {
      Name: proxy.Name,
      Breed: proxy.Breed,
      Age: proxy.Age,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.PeopleProvider
export namespace PeopleProvider {
  export class Proxy {
    interop: AssemblyExports;
    instance: object;

    constructor(instance: object, interop: AssemblyExports) {
      this.interop = interop;
      this.instance = instance;
    }

    public async FetchPeopleAsync(): Promise<People.Proxy> {
      const res = await this.interop.TypeShim.Sample.PeopleProviderInterop.FetchPeopleAsync(this.instance);
      return new People.Proxy(res, this.interop);
    }

  }

  export interface Snapshot {
  }
  export function snapshot(proxy: PeopleProvider.Proxy): PeopleProvider.Snapshot {
    return {
    };
  }
}

