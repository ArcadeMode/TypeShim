// Auto-generated TypeScript module exports interface
export interface AssemblyExports {
    TypeShim: {
        Sample: {
            Capabilities: {
                CapabilitiesModuleInterop: CapabilitiesModuleInterop;
                CapabilitiesInterop: CapabilitiesInterop;
                PrimitivesCapabilityInterop: PrimitivesCapabilityInterop;
            };
            DogInterop: DogInterop;
            PeopleInterop: PeopleInterop;
            PeopleProviderInterop: PeopleProviderInterop;
            PersonInterop: PersonInterop;
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

  public get Capabilities(): Capabilities {
    const res = this.interop.TypeShim.Sample.Capabilities.CapabilitiesModuleInterop.get_Capabilities();
    return new CapabilitiesProxy(res, this.interop);
  }

}

// Auto-generated TypeShim TSModule class. Source class: TypeShim.Sample.TypeShimSampleModule
export class TypeShimSampleModule {
  private interop: AssemblyExports;

  constructor(interop: AssemblyExports) {
    this.interop = interop;
  }

  public get PeopleProvider(): PeopleProvider | null {
    const res = this.interop.TypeShim.Sample.TypeShimSampleModuleInterop.get_PeopleProvider();
    return res ? new PeopleProviderProxy(res, this.interop) : null;
  }

}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Capabilities.CapabilitiesModule
export interface CapabilitiesModuleInterop {
    get_Capabilities(): object;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Capabilities.Capabilities
export interface CapabilitiesInterop {
    VoidMethod(instance: object): void;
    GetStringCapability(instance: object, baseString: string): object;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Capabilities.PrimitivesCapability
export interface PrimitivesCapabilityInterop {
    GetStringLength(instance: object): number;
    ToUpperCase(instance: object): string;
    Concat(instance: object, str1: string, str2: string): string;
    ContainsUpperCase(instance: object): boolean;
    get_BaseString(instance: object): string;
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

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.People
export interface PeopleInterop {
    get_All(instance: object): Array<object>;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.PeopleProvider
export interface PeopleProviderInterop {
    FetchPeopleAsync(instance: object): Promise<object>;
    FetchElderlyPeopleAsync(instance: object): Promise<object>;
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

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.TypeShimSampleModule
export interface TypeShimSampleModuleInterop {
    get_PeopleProvider(): object | null;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.Capabilities.Capabilities
export interface Capabilities {
    VoidMethod(): void;
    GetStringCapability(baseString: string): PrimitivesCapability;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.Capabilities.PrimitivesCapability
export interface PrimitivesCapability {
    GetStringLength(): number;
    ToUpperCase(): string;
    Concat(str1: string, str2: string): string;
    ContainsUpperCase(): boolean;
    readonly BaseString: string;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.Dog
export interface Dog {
    Bark(): string;
    GetAge(asHumanYears: boolean): number;
    Name: string;
    Breed: string;
    Age: number;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.People
export interface People {
    readonly All: Array<Person>;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.PeopleProvider
export interface PeopleProvider {
    FetchPeopleAsync(): Promise<People>;
    FetchElderlyPeopleAsync(): Promise<People>;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.Person
export interface Person {
    IsOlderThan(other: Person): boolean;
    AdoptPet(): void;
    Id: number;
    Name: string;
    Age: number;
    Pet: Dog | null;
}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.Capabilities.Capabilities
class CapabilitiesProxy implements Capabilities {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public VoidMethod(): void {
    this.interop.TypeShim.Sample.Capabilities.CapabilitiesInterop.VoidMethod(this.instance);
  }

  public GetStringCapability(baseString: string): PrimitivesCapability {
    const res = this.interop.TypeShim.Sample.Capabilities.CapabilitiesInterop.GetStringCapability(this.instance, baseString);
    return new PrimitivesCapabilityProxy(res, this.interop);
  }

}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.Capabilities.PrimitivesCapability
class PrimitivesCapabilityProxy implements PrimitivesCapability {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public GetStringLength(): number {
    return this.interop.TypeShim.Sample.Capabilities.PrimitivesCapabilityInterop.GetStringLength(this.instance);
  }

  public ToUpperCase(): string {
    return this.interop.TypeShim.Sample.Capabilities.PrimitivesCapabilityInterop.ToUpperCase(this.instance);
  }

  public Concat(str1: string, str2: string): string {
    return this.interop.TypeShim.Sample.Capabilities.PrimitivesCapabilityInterop.Concat(this.instance, str1, str2);
  }

  public ContainsUpperCase(): boolean {
    return this.interop.TypeShim.Sample.Capabilities.PrimitivesCapabilityInterop.ContainsUpperCase(this.instance);
  }

  public get BaseString(): string {
    return this.interop.TypeShim.Sample.Capabilities.PrimitivesCapabilityInterop.get_BaseString(this.instance);
  }

}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.Dog
class DogProxy implements Dog {
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

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.People
class PeopleProxy implements People {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public get All(): Array<Person> {
    const res = this.interop.TypeShim.Sample.PeopleInterop.get_All(this.instance);
    return res.map(item => new PersonProxy(item, this.interop));
  }

}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.PeopleProvider
class PeopleProviderProxy implements PeopleProvider {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public async FetchPeopleAsync(): Promise<People> {
    const res = await this.interop.TypeShim.Sample.PeopleProviderInterop.FetchPeopleAsync(this.instance);
    return new PeopleProxy(res, this.interop);
  }

  public async FetchElderlyPeopleAsync(): Promise<People> {
    const res = await this.interop.TypeShim.Sample.PeopleProviderInterop.FetchElderlyPeopleAsync(this.instance);
    return new PeopleProxy(res, this.interop);
  }

}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.Person
class PersonProxy implements Person {
  interop: AssemblyExports;
  instance: object;

  constructor(instance: object, interop: AssemblyExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public IsOlderThan(other: Person): boolean {
    const otherInstance = other instanceof PersonProxy ? other.instance : other;
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

  public get Pet(): Dog | null {
    const res = this.interop.TypeShim.Sample.PersonInterop.get_Pet(this.instance);
    return res ? new DogProxy(res, this.interop) : null;
  }

  public set Pet(value: Dog | null) {
    const valueInstance = value instanceof DogProxy ? value.instance : value;
    this.interop.TypeShim.Sample.PersonInterop.set_Pet(this.instance, valueInstance);
  }

}

