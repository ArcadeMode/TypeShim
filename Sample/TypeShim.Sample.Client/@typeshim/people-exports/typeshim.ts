// Auto-generated TypeScript module exports interface
export interface WasmModuleExports {
    TypeShim: {
        Sample: {
            DogInterop: DogInterop;
            PeopleInterop: PeopleInterop;
            PeopleProviderInterop: PeopleProviderInterop;
            PersonInterop: PersonInterop;
            TypeShimSampleModuleInterop: TypeShimSampleModuleInterop;
        };
    };
}

export class WasmModule {
  private interop: WasmModuleExports
  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }
  public get TypeShimSampleModule(): TypeShimSampleModuleStatics {
    return new TypeShimSampleModuleStatics(this.interop);
  }
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Dog
export interface DogInterop {
    Bark(instance: object): string;
    get_Name(instance: object): string;
    set_Name(instance: object, value: string): void;
    get_Breed(instance: object): string;
    set_Breed(instance: object, value: string): void;
    get_Age(instance: object): number;
    set_Age(instance: object, value: number): void;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.People
export interface PeopleInterop {
    get_All(instance: object): Array<Person>;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.PeopleProvider
export interface PeopleProviderInterop {
    FetchPeopleAsync(instance: object): Promise<People>;
    FetchElderlyPeopleAsync(instance: object): Promise<People>;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Person
export interface PersonInterop {
    AdoptPet(instance: object): void;
    get_Id(instance: object): number;
    set_Id(instance: object, value: number): void;
    get_Name(instance: object): string;
    set_Name(instance: object, value: string): void;
    get_Age(instance: object): number;
    set_Age(instance: object, value: number): void;
    get_Pet(instance: object): Dog | null;
    set_Pet(instance: object, value: Dog | null): void;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.TypeShimSampleModule
export interface TypeShimSampleModuleInterop {
    get_PeopleProvider(): PeopleProvider | null;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.Dog
export interface Dog {
    Bark(): string;
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
    AdoptPet(): void;
    Id: number;
    Name: string;
    Age: number;
    Pet: Dog | null;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.TypeShimSampleModule
export interface TypeShimSampleModule {
}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.Dog
export class DogProxy implements Dog {
  private interop: WasmModuleExports;
  private instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public Bark(): string {
    return this.interop.TypeShim.Sample.DogInterop.Bark(this.instance);
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
// Auto-generated TypeScript statics class. Source class: TypeShim.Sample.Dog
export class DogStatics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }

}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.People
export class PeopleProxy implements People {
  private interop: WasmModuleExports;
  private instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public get All(): Array<Person> {
    const res = this.interop.TypeShim.Sample.PeopleInterop.get_All(this.instance);
    return res.map(item => new PersonProxy(item, this.interop));
  }

}
// Auto-generated TypeScript statics class. Source class: TypeShim.Sample.People
export class PeopleStatics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }

}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.PeopleProvider
export class PeopleProviderProxy implements PeopleProvider {
  private interop: WasmModuleExports;
  private instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
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
// Auto-generated TypeScript statics class. Source class: TypeShim.Sample.PeopleProvider
export class PeopleProviderStatics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }

}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.Person
export class PersonProxy implements Person {
  private interop: WasmModuleExports;
  private instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
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
    this.interop.TypeShim.Sample.PersonInterop.set_Pet(this.instance, value);
  }

}
// Auto-generated TypeScript statics class. Source class: TypeShim.Sample.Person
export class PersonStatics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }

}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.TypeShimSampleModule
export class TypeShimSampleModuleProxy implements TypeShimSampleModule {
  private interop: WasmModuleExports;
  private instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

}
// Auto-generated TypeScript statics class. Source class: TypeShim.Sample.TypeShimSampleModule
export class TypeShimSampleModuleStatics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }

  public get PeopleProvider(): PeopleProvider | null {
    const res = this.interop.TypeShim.Sample.TypeShimSampleModuleInterop.get_PeopleProvider();
    return res ? new PeopleProviderProxy(res, this.interop) : null;
  }

}

