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

  static fromHandle<T extends ProxyBase>(ctor: { prototype: T }, handle: object): T {
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
        ArraysDemoInterop: {
          SumElements(instance: object, cool: object): number;
          Append(instance: object, value: number): void;
          get_IntArrayProperty(instance: object): Array<number>;
          get_ApiClient(instance: object): object;
        };
        PrimitivesDemoInterop: {
          ctor(jsObject: object): object;
          GetStringLength(instance: object): number;
          ToUpperCase(instance: object): string;
          Concat(instance: object, str1: string, str2: string): string;
          ContainsUpperCase(instance: object): boolean;
          ResetBaseString(instance: object): void;
          MultiplyString(instance: object, times: number): void;
          get_InitialStringProperty(instance: object): string;
          get_StringProperty(instance: object): string;
          set_StringProperty(instance: object, value: string): void;
        };
      };
      PeopleInterop: {
        ctor(jsObject: object): object;
        get_All(instance: object): Array<object>;
        set_All(instance: object, value: Array<object>): void;
      };
      PersonInterop: {
        ctor(jsObject: object): object;
        IsOlderThan(instance: object, other: object): boolean;
        AdoptPet(instance: object): void;
        Adopt(instance: object, newPet: object): void;
        get_Id(instance: object): number;
        set_Id(instance: object, value: number): void;
        get_Name(instance: object): string;
        set_Name(instance: object, value: string): void;
        get_Age(instance: object): number;
        set_Age(instance: object, value: number): void;
        get_Pets(instance: object): Array<object>;
        set_Pets(instance: object, value: Array<object>): void;
      };
      DogInterop: {
        ctor(jsObject: object): object;
        Bark(instance: object): string;
        GetAge(instance: object, asHumanYears: boolean): number;
        get_Name(instance: object): string;
        set_Name(instance: object, value: string): void;
        get_Breed(instance: object): string;
        set_Breed(instance: object, value: string): void;
        get_Age(instance: object): number;
        set_Age(instance: object, value: number): void;
      };
      MyAppInterop: {
        ctor(): object;
        Initialize(baseAddress: string): void;
        GetPeopleProvider(): object;
      };
      PeopleProviderInterop: {
        FetchPeopleAsync(instance: object): Promise<object>;
        get_PeopleCache(instance: object): Array<object> | null;
        get_DelayTask(instance: object): Promise<object | null> | null;
        set_DelayTask(instance: object, value: Promise<object | null> | null): void;
      };
      TimeoutUnitInterop: {
        ctor(jsObject: object): object;
        get_Timeout(instance: object): number;
        set_Timeout(instance: object, value: number): void;
      };
    };
  };
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.ArraysDemo
export namespace ArraysDemo {
  export class Proxy extends ProxyBase {
    private constructor() { super(undefined!); }

    public SumElements(cool: object): number {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.SumElements(this.instance, cool);
    }

    public Append(value: number): void {
      TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.Append(this.instance, value);
    }
    public get IntArrayProperty(): Array<number> {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.get_IntArrayProperty(this.instance);
    }

    public get ApiClient(): object {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.get_ApiClient(this.instance);
    }
  }
  export interface Properties {
    IntArrayProperty: Array<number>;
    ApiClient: object;
  }
  export function materialize(proxy: ArraysDemo.Proxy): ArraysDemo.Properties {
    return {
      IntArrayProperty: proxy.IntArrayProperty,
      ApiClient: object.properties(proxy.ApiClient),
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Capabilities.PrimitivesDemo
export namespace PrimitivesDemo {
  export class Proxy extends ProxyBase {
    constructor(jsObject: PrimitivesDemo.Initializer) {
      super(TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ctor(jsObject));
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

    public get StringProperty(): string {
      return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.get_StringProperty(this.instance);
    }

    public set StringProperty(value: string) {
      TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.set_StringProperty(this.instance, value);
    }
  }
  export interface Properties {
    InitialStringProperty: string;
    StringProperty: string;
  }
  export interface Initializer {
    InitialStringProperty: string;
    StringProperty: string;
  }
  export function materialize(proxy: PrimitivesDemo.Proxy): PrimitivesDemo.Properties {
    return {
      InitialStringProperty: proxy.InitialStringProperty,
      StringProperty: proxy.StringProperty,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.People
export namespace People {
  export class Proxy extends ProxyBase {
    constructor(jsObject: People.Initializer) {
      super(TypeShimConfig.exports.TypeShim.Sample.PeopleInterop.ctor(jsObject));
    }

    public get All(): Array<Person.Proxy> {
      const res = TypeShimConfig.exports.TypeShim.Sample.PeopleInterop.get_All(this.instance);
      return res.map(e => ProxyBase.fromHandle(Person.Proxy, e));
    }

    public set All(value: Array<Person.Proxy | Person.Initializer>) {
      const valueInstance = value.map(e => e instanceof Person.Proxy ? e.instance : e);
      TypeShimConfig.exports.TypeShim.Sample.PeopleInterop.set_All(this.instance, valueInstance);
    }
  }
  export interface Properties {
    All: Array<Person.Properties>;
  }
  export interface Initializer {
    All: Array<Person.Proxy | Person.Initializer>;
  }
  export function materialize(proxy: People.Proxy): People.Properties {
    return {
      All: proxy.All.map(e => Person.properties(e)),
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Person
export namespace Person {
  export class Proxy extends ProxyBase {
    constructor(jsObject: Person.Initializer) {
      super(TypeShimConfig.exports.TypeShim.Sample.PersonInterop.ctor(jsObject));
    }

    public IsOlderThan(other: Person.Proxy | Person.Initializer): boolean {
      const otherInstance = other instanceof Person.Proxy ? other.instance : other;
      return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.IsOlderThan(this.instance, otherInstance);
    }

    public AdoptPet(): void {
      TypeShimConfig.exports.TypeShim.Sample.PersonInterop.AdoptPet(this.instance);
    }

    public Adopt(newPet: Dog.Proxy | Dog.Initializer): void {
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

    public set Pets(value: Array<Dog.Proxy | Dog.Initializer>) {
      const valueInstance = value.map(e => e instanceof Dog.Proxy ? e.instance : e);
      TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Pets(this.instance, valueInstance);
    }
  }
  export interface Properties {
    Id: number;
    Name: string;
    Age: number;
    Pets: Array<Dog.Properties>;
  }
  export interface Initializer {
    Id: number;
    Name: string;
    Age: number;
    Pets: Array<Dog.Proxy | Dog.Initializer>;
  }
  export function materialize(proxy: Person.Proxy): Person.Properties {
    return {
      Id: proxy.Id,
      Name: proxy.Name,
      Age: proxy.Age,
      Pets: proxy.Pets.map(e => Dog.properties(e)),
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.Dog
export namespace Dog {
  export class Proxy extends ProxyBase {
    constructor(jsObject: Dog.Initializer) {
      super(TypeShimConfig.exports.TypeShim.Sample.DogInterop.ctor(jsObject));
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
  export interface Properties {
    Name: string;
    Breed: string;
    Age: number;
  }
  export interface Initializer {
    Name: string;
    Breed: string;
    Age: number;
  }
  export function materialize(proxy: Dog.Proxy): Dog.Properties {
    return {
      Name: proxy.Name,
      Breed: proxy.Breed,
      Age: proxy.Age,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.MyApp
export namespace MyApp {
  export class Proxy extends ProxyBase {
    constructor() {
      super(TypeShimConfig.exports.TypeShim.Sample.MyAppInterop.ctor());
    }

    public static Initialize(baseAddress: string): void {
      TypeShimConfig.exports.TypeShim.Sample.MyAppInterop.Initialize(baseAddress);
    }

    public static GetPeopleProvider(): PeopleProvider.Proxy {
      const res = TypeShimConfig.exports.TypeShim.Sample.MyAppInterop.GetPeopleProvider();
      return ProxyBase.fromHandle(PeopleProvider.Proxy, res);
    }
  }
  export function materialize(proxy: MyApp.Proxy): MyApp.Properties {
    return {
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.PeopleProvider
export namespace PeopleProvider {
  export class Proxy extends ProxyBase {
    private constructor() { super(undefined!); }

    public async FetchPeopleAsync(): Promise<People.Proxy> {
      const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.FetchPeopleAsync(this.instance);
      return res.then(e => ProxyBase.fromHandle(People.Proxy, e));
    }
    public get PeopleCache(): Array<Person.Proxy> | null {
      const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.get_PeopleCache(this.instance);
      return res ? res.map(e => ProxyBase.fromHandle(Person.Proxy, e)) : null;
    }

    public get DelayTask(): Promise<TimeoutUnit.Proxy | null> | null {
      const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.get_DelayTask(this.instance);
      return res ? res.then(e => e ? ProxyBase.fromHandle(TimeoutUnit.Proxy, e) : null) : null;
    }

    public set DelayTask(value: Promise<TimeoutUnit.Proxy | TimeoutUnit.Initializer | null> | null) {
      const valueInstance = value ? value.then(e => e ? e instanceof TimeoutUnit.Proxy ? e.instance : e : null) : null;
      TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.set_DelayTask(this.instance, valueInstance);
    }
  }
  export interface Properties {
    PeopleCache: Array<Person.Properties> | null;
    DelayTask: Promise<TimeoutUnit.Properties | null> | null;
  }
  export function materialize(proxy: PeopleProvider.Proxy): PeopleProvider.Properties {
    return {
      PeopleCache: proxy.PeopleCache ? proxy.PeopleCache.map(e => Person.properties(e)) : null,
      DelayTask: proxy.DelayTask ? proxy.DelayTask.then(e => e ? TimeoutUnit.properties(e) : null) : null,
    };
  }
}

// Auto-generated TypeScript namespace for class: TypeShim.Sample.TimeoutUnit
export namespace TimeoutUnit {
  export class Proxy extends ProxyBase {
    constructor(jsObject: TimeoutUnit.Initializer) {
      super(TypeShimConfig.exports.TypeShim.Sample.TimeoutUnitInterop.ctor(jsObject));
    }

    public get Timeout(): number {
      return TypeShimConfig.exports.TypeShim.Sample.TimeoutUnitInterop.get_Timeout(this.instance);
    }

    public set Timeout(value: number) {
      TypeShimConfig.exports.TypeShim.Sample.TimeoutUnitInterop.set_Timeout(this.instance, value);
    }
  }
  export interface Properties {
    Timeout: number;
  }
  export interface Initializer {
    Timeout: number;
  }
  export function materialize(proxy: TimeoutUnit.Proxy): TimeoutUnit.Properties {
    return {
      Timeout: proxy.Timeout,
    };
  }
}

