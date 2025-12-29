class TypeShimConfig {
  private static _exports: AssemblyExports | null = null;

  static get exports() {
    if (!TypeShimConfig._exports) {
      throw new Error("TypeShim has not been initialized.");
    }
    return TypeShimConfig._exports;
  }

  static initialize(options: { assemblyExports: AssemblyExports, setModuleImports: (scriptName: string, imports: object) => void }) {
    if (TypeShimConfig._exports){
      throw new Error("TypeShim has already been initialized.");
    }
    options.setModuleImports("@typeshim", { unwrap: (obj: any) => obj });
    TypeShimConfig._exports = options.assemblyExports;
  }
}

export const TypeShimInitializer = { initialize: TypeShimConfig.initialize };

abstract class ProxyBase {
  instance: object;
  constructor(instance: object) {
    this.instance = instance;
  }

  static fromHandle<T extends ProxyBase>(ctor: new (...args: any[]) => T, handle: object): T {
    const obj = Object.create(ctor.prototype) as T;
    obj.instance = handle;
    return obj;
  }
}


// Auto-generated TypeScript module exports interface
export interface AssemblyExports{
  TypeShim: {
    Sample: {
      Capabilities: {
        CapabilitiesModuleInterop: {
          GetCapabilitiesProvider(): object
        },
        CapabilitiesProviderInterop: {
          GetPrimitivesDemo(instance: object, baseString: string): object
          GetArraysDemo(instance: object): object
        },
        PrimitivesDemoInterop: {
          GetStringLength(instance: object): number
          ToUpperCase(instance: object): string
          Concat(instance: object, str1: string, str2: string): string
          ContainsUpperCase(instance: object): boolean
          ResetBaseString(instance: object): void
          MultiplyString(instance: object, times: number): void
          get_InitialStringProperty(instance: object): string
          set_InitialStringProperty(instance: object, value: string): void
          get_StringProperty(instance: object): string
          set_StringProperty(instance: object, value: string): void
        };
        ArraysDemoInterop: {
          SumIntArray(instance: object): number
          AppendToIntArray(instance: object, value: number): void
          get_IntArrayProperty(instance: object): Array<number>
          set_IntArrayProperty(instance: object, value: Array<number>): void
        },
      };
      PeopleInterop: {
        get_All(instance: object): Array<object>
        set_All(instance: object, value: Array<object>): void
      },
      PersonInterop: {
        IsOlderThan(instance: object, other: object): boolean
        AdoptPet(instance: object): void
        Adopt(instance: object, newPet: object): void
        get_Id(instance: object): number
        set_Id(instance: object, value: number): void
        get_Name(instance: object): string
        set_Name(instance: object, value: string): void
        get_Age(instance: object): number
        set_Age(instance: object, value: number): void
        get_Pets(instance: object): Array<object>
        set_Pets(instance: object, value: Array<object>): void
      },
      DogInterop: {
        Bark(instance: object): string
        GetAge(instance: object, asHumanYears: boolean): number
        get_Name(instance: object): string
        set_Name(instance: object, value: string): void
        get_Breed(instance: object): string
        set_Breed(instance: object, value: string): void
        get_Age(instance: object): number
        set_Age(instance: object, value: number): void
      },
      PeopleProviderInterop: {
        DoStuff(instance: object, task: Promise<object | null>): void
        FetchPeopleAsync(instance: object): Promise<object>
        get_PeopleCache(): Array<object | null> | null
        get_Unit(instance: object): Promise<object | null> | null
        set_Unit(instance: object, value: Promise<object | null> | null): void
      },
      TimeoutUnitInterop: {
        get_Timeout(instance: object): number
        set_Timeout(instance: object, value: number): void
      },
      TypeShimSampleModuleInterop: {
        get_PeopleProvider(): object | null
      },
    };
  };
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.CapabilitiesModule
export namespace CapabilitiesModule {
  export class Proxy {
    private constructor() {}

    public static GetCapabilitiesProvider(): CapabilitiesProvider.Proxy {
      const res = TypeShimConfig.exports.TypeShim.Sample.Capabilities.CapabilitiesModuleInterop.GetCapabilitiesProvider();
      return ProxyBase.fromHandle(CapabilitiesProvider.Proxy, res);
    }

  }

}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.CapabilitiesProvider
export namespace CapabilitiesProvider {
  export class Proxy extends ProxyBase {
    constructor() {
      super(null!);
    }

    public GetPrimitivesDemo(baseString: string): PrimitivesDemo.Proxy {
      const res = TypeShimConfig.exports.TypeShim.Sample.Capabilities.CapabilitiesProviderInterop.GetPrimitivesDemo(this.instance, baseString);
      return ProxyBase.fromHandle(PrimitivesDemo.Proxy, res);
    }

    public GetArraysDemo(): ArraysDemo.Proxy {
      const res = TypeShimConfig.exports.TypeShim.Sample.Capabilities.CapabilitiesProviderInterop.GetArraysDemo(this.instance);
      return ProxyBase.fromHandle(ArraysDemo.Proxy, res);
    }

  }

}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.PrimitivesDemo
export namespace PrimitivesDemo {
  export class Proxy extends ProxyBase {
    constructor() {
      super(null!);
    }

    public GetStringLength(): number {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.GetStringLength(this.instance);
    }

    public ToUpperCase(): string {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ToUpperCase(this.instance);
    }

    public Concat(str1: string, str2: string): string {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.Concat(this.instance, str1, str2);
    }

    public ContainsUpperCase(): boolean {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ContainsUpperCase(this.instance);
    }

    public ResetBaseString(): void {
      TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ResetBaseString(this.instance);
    }

    public MultiplyString(times: number): void {
      TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.MultiplyString(this.instance, times);
    }

    public get InitialStringProperty(): string {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.get_InitialStringProperty(this.instance);
    }

    public set InitialStringProperty(value: string) {
      TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.set_InitialStringProperty(this.instance, value);
    }

    public get StringProperty(): string {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.get_StringProperty(this.instance);
    }

    public set StringProperty(value: string) {
      TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.set_StringProperty(this.instance, value);
    }

  }

  export interface Snapshot {
    InitialStringProperty: string;
    StringProperty: string;
  }
  export const Snapshot: {
    [Symbol.hasInstance](v: unknown): boolean;
  } = {
    [Symbol.hasInstance](v: unknown) {
      if (!v || typeof v !== 'object') return false;
      const o = v as any;
      return typeof o.InitialStringProperty === 'string' && typeof o.StringProperty === 'string';
    }
  };
  export function snapshot(proxy: PrimitivesDemo.Proxy): PrimitivesDemo.Snapshot {
    return {
      InitialStringProperty: proxy.InitialStringProperty,
      StringProperty: proxy.StringProperty,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.ArraysDemo
export namespace ArraysDemo {
  export class Proxy extends ProxyBase {
    constructor() {
      super(null!);
    }

    public SumIntArray(): number {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.SumIntArray(this.instance);
    }

    public AppendToIntArray(value: number): void {
      TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.AppendToIntArray(this.instance, value);
    }

    public get IntArrayProperty(): Array<number> {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.get_IntArrayProperty(this.instance);
    }

    public set IntArrayProperty(value: Array<number>) {
      TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.set_IntArrayProperty(this.instance, value);
    }

  }

  export interface Snapshot {
    IntArrayProperty: Array<number>;
  }
  export const Snapshot: {
    [Symbol.hasInstance](v: unknown): boolean;
  } = {
    [Symbol.hasInstance](v: unknown) {
      if (!v || typeof v !== 'object') return false;
      const o = v as any;
      return Array.isArray(o.IntArrayProperty) && o.IntArrayProperty.every((e: any) => typeof e === 'number');
    }
  };
  export function snapshot(proxy: ArraysDemo.Proxy): ArraysDemo.Snapshot {
    return {
      IntArrayProperty: proxy.IntArrayProperty,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.People
export namespace People {
  export class Proxy extends ProxyBase {
    constructor() {
      super(null!);
    }

    public get All(): Array<Person.Proxy> {
      const res = TypeShimConfig.exports.TypeShim.Sample.PeopleInterop.get_All(this.instance);
      return res.map(e => ProxyBase.fromHandle(Person.Proxy, e));
    }

    public set All(value: Array<Person.Proxy | Person.Snapshot>) {
      const valueInstance = value.map(e => e instanceof Person.Proxy ? e.instance : e);
      TypeShimConfig.exports.TypeShim.Sample.PeopleInterop.set_All(this.instance, valueInstance);
    }

  }

  export interface Snapshot {
    All: Array<Person.Snapshot>;
  }
  export const Snapshot: {
    [Symbol.hasInstance](v: unknown): boolean;
  } = {
    [Symbol.hasInstance](v: unknown) {
      if (!v || typeof v !== 'object') return false;
      const o = v as any;
      return Array.isArray(o.All) && o.All.every((e: any) => e instanceof Person.Snapshot);
    }
  };
  export function snapshot(proxy: People.Proxy): People.Snapshot {
    return {
      All: proxy.All.map(e => Person.snapshot(e)),
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Person
export namespace Person {
  export class Proxy extends ProxyBase {
    constructor() {
      super(null!);
    }

    public IsOlderThan(other: Person.Proxy | Person.Snapshot): boolean {
      const otherInstance = other instanceof Person.Proxy ? other.instance : other;
      return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.IsOlderThan(this.instance, otherInstance);
    }

    public AdoptPet(): void {
      TypeShimConfig.exports.TypeShim.Sample.PersonInterop.AdoptPet(this.instance);
    }

    public Adopt(newPet: Dog.Proxy | Dog.Snapshot): void {
      const newPetInstance = newPet instanceof Dog.Proxy ? newPet.instance : newPet;
      TypeShimConfig.exports.TypeShim.Sample.PersonInterop.Adopt(this.instance, newPetInstance);
    }

    public get Id(): number {
      return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.get_Id(this.instance);
    }

    public set Id(value: number) {
      TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Id(this.instance, value);
    }

    public get Name(): string {
      return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.get_Name(this.instance);
    }

    public set Name(value: string) {
      TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Name(this.instance, value);
    }

    public get Age(): number {
      return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.get_Age(this.instance);
    }

    public set Age(value: number) {
      TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Age(this.instance, value);
    }

    public get Pets(): Array<Dog.Proxy> {
      const res = TypeShimConfig.exports.TypeShim.Sample.PersonInterop.get_Pets(this.instance);
      return res.map(e => ProxyBase.fromHandle(Dog.Proxy, e));
    }

    public set Pets(value: Array<Dog.Proxy | Dog.Snapshot>) {
      const valueInstance = value.map(e => e instanceof Dog.Proxy ? e.instance : e);
      TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Pets(this.instance, valueInstance);
    }

  }

  export interface Snapshot {
    Id: number;
    Name: string;
    Age: number;
    Pets: Array<Dog.Snapshot>;
  }
  export const Snapshot: {
    [Symbol.hasInstance](v: unknown): boolean;
  } = {
    [Symbol.hasInstance](v: unknown) {
      if (!v || typeof v !== 'object') return false;
      const o = v as any;
      return typeof o.Id === 'number' && typeof o.Name === 'string' && typeof o.Age === 'number' && Array.isArray(o.Pets) && o.Pets.every((e: any) => e instanceof Dog.Snapshot);
    }
  };
  export function snapshot(proxy: Person.Proxy): Person.Snapshot {
    return {
      Id: proxy.Id,
      Name: proxy.Name,
      Age: proxy.Age,
      Pets: proxy.Pets.map(e => Dog.snapshot(e)),
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Dog
export namespace Dog {
  export class Proxy extends ProxyBase {
    constructor() {
      super(null!);
    }

    public Bark(): string {
      return TypeShimConfig.exports.TypeShim.Sample.DogInterop.Bark(this.instance);
    }

    public GetAge(asHumanYears: boolean): number {
      return TypeShimConfig.exports.TypeShim.Sample.DogInterop.GetAge(this.instance, asHumanYears);
    }

    public get Name(): string {
      return TypeShimConfig.exports.TypeShim.Sample.DogInterop.get_Name(this.instance);
    }

    public set Name(value: string) {
      TypeShimConfig.exports.TypeShim.Sample.DogInterop.set_Name(this.instance, value);
    }

    public get Breed(): string {
      return TypeShimConfig.exports.TypeShim.Sample.DogInterop.get_Breed(this.instance);
    }

    public set Breed(value: string) {
      TypeShimConfig.exports.TypeShim.Sample.DogInterop.set_Breed(this.instance, value);
    }

    public get Age(): number {
      return TypeShimConfig.exports.TypeShim.Sample.DogInterop.get_Age(this.instance);
    }

    public set Age(value: number) {
      TypeShimConfig.exports.TypeShim.Sample.DogInterop.set_Age(this.instance, value);
    }

  }

  export interface Snapshot {
    Name: string;
    Breed: string;
    Age: number;
  }
  export const Snapshot: {
    [Symbol.hasInstance](v: unknown): boolean;
  } = {
    [Symbol.hasInstance](v: unknown) {
      if (!v || typeof v !== 'object') return false;
      const o = v as any;
      return typeof o.Name === 'string' && typeof o.Breed === 'string' && typeof o.Age === 'number';
    }
  };
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
  export class Proxy extends ProxyBase {
    constructor() {
      super(null!);
    }

    public DoStuff(task: Promise<TimeoutUnit.Proxy | TimeoutUnit.Snapshot | null>): void {
      const taskInstance = task.then(e => e ? e instanceof TimeoutUnit.Proxy ? e.instance : e : null);
      TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.DoStuff(this.instance, taskInstance);
    }

    public async FetchPeopleAsync(): Promise<People.Proxy> {
      const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.FetchPeopleAsync(this.instance);
      return res.then(e => ProxyBase.fromHandle(People.Proxy, e));
    }

    public static get PeopleCache(): Array<Person.Proxy | null> | null {
      const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.get_PeopleCache();
      return res ? res.map(e => e ? ProxyBase.fromHandle(Person.Proxy, e) : null) : null;
    }

    public get Unit(): Promise<TimeoutUnit.Proxy | null> | null {
      const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.get_Unit(this.instance);
      return res ? res.then(e => e ? ProxyBase.fromHandle(TimeoutUnit.Proxy, e) : null) : null;
    }

    public set Unit(value: Promise<TimeoutUnit.Proxy | TimeoutUnit.Snapshot | null> | null) {
      const valueInstance = value ? value.then(e => e ? e instanceof TimeoutUnit.Proxy ? e.instance : e : null) : null;
      TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.set_Unit(this.instance, valueInstance);
    }

  }

}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.TimeoutUnit
export namespace TimeoutUnit {
  export class Proxy extends ProxyBase {
    constructor() {
      super(null!);
    }

    public get Timeout(): number {
      return TypeShimConfig.exports.TypeShim.Sample.TimeoutUnitInterop.get_Timeout(this.instance);
    }

    public set Timeout(value: number) {
      TypeShimConfig.exports.TypeShim.Sample.TimeoutUnitInterop.set_Timeout(this.instance, value);
    }

  }

  export interface Snapshot {
    Timeout: number;
  }
  export const Snapshot: {
    [Symbol.hasInstance](v: unknown): boolean;
  } = {
    [Symbol.hasInstance](v: unknown) {
      if (!v || typeof v !== 'object') return false;
      const o = v as any;
      return typeof o.Timeout === 'number';
    }
  };
  export function snapshot(proxy: TimeoutUnit.Proxy): TimeoutUnit.Snapshot {
    return {
      Timeout: proxy.Timeout,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.TypeShimSampleModule
export namespace TypeShimSampleModule {
  export class Proxy {
    private constructor() {}

    public static get PeopleProvider(): PeopleProvider.Proxy | null {
      const res = TypeShimConfig.exports.TypeShim.Sample.TypeShimSampleModuleInterop.get_PeopleProvider();
      return res ? ProxyBase.fromHandle(PeopleProvider.Proxy, res) : null;
    }

  }

}

