// Auto-generated TypeScript module exports interface
export interface WasmModuleExports {
    TypeShim: {
        Sample: {
            DogInterop: DogInterop;
            PeopleInterop: PeopleInterop;
            PeopleProviderInterop: PeopleProviderInterop;
            PersonInterop: PersonInterop;
        };
    };
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Dog
export interface DogInterop {
    GetName(instance: object): string;
    GetBreed(instance: object): string;
    Bark(instance: object): string;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.People
export interface PeopleInterop {
    GetAll(instance: object): Array<Person>;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.PeopleProvider
export interface PeopleProviderInterop {
    FetchPeopleAsync(): Promise<People>;
    FetchElderlyPeopleAsync(): Promise<People>;
}

// Auto-generated TypeScript interop interface. Source class: TypeShim.Sample.Person
export interface PersonInterop {
    GetId(instance: object): number;
    GetName(instance: object): string;
    GetAge(instance: object): number;
    GetPet(instance: object): Dog | null;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.Dog
export interface Dog {
    GetName(): string;
    GetBreed(): string;
    Bark(): string;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.People
export interface People {
    GetAll(): Array<Person>;
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.PeopleProvider
export interface PeopleProvider {
}

// Auto-generated TypeScript interface. Source class: TypeShim.Sample.Person
export interface Person {
    GetId(): number;
    GetName(): string;
    GetAge(): number;
    GetPet(): Dog | null;
}

export class WasmModule {
  private interop: WasmModuleExports
  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }
  public PeopleProvider(): PeopleProviderStatics {
    return new PeopleProviderStatics(this.interop);
  }
}

// Auto-generated TypeScript proxy class. Source class: TypeShim.Sample.Dog
export class DogProxy implements Dog {
  private interop: WasmModuleExports;
  private instance: object;

  constructor(instance: object, interop: WasmModuleExports) {
    this.interop = interop;
    this.instance = instance;
  }

  public GetName(): string {
    return this.interop.TypeShim.Sample.DogInterop.GetName(this.instance);
  }

  public GetBreed(): string {
    return this.interop.TypeShim.Sample.DogInterop.GetBreed(this.instance);
  }

  public Bark(): string {
    return this.interop.TypeShim.Sample.DogInterop.Bark(this.instance);
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

  public GetAll(): Array<Person> {
    const res = this.interop.TypeShim.Sample.PeopleInterop.GetAll(this.instance);
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

}
// Auto-generated TypeScript statics class. Source class: TypeShim.Sample.PeopleProvider
export class PeopleProviderStatics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }

  public async FetchPeopleAsync(): Promise<People> {
    const res = await this.interop.TypeShim.Sample.PeopleProviderInterop.FetchPeopleAsync();
    return new PeopleProxy(res, this.interop);
  }

  public async FetchElderlyPeopleAsync(): Promise<People> {
    const res = await this.interop.TypeShim.Sample.PeopleProviderInterop.FetchElderlyPeopleAsync();
    return new PeopleProxy(res, this.interop);
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

  public GetId(): number {
    return this.interop.TypeShim.Sample.PersonInterop.GetId(this.instance);
  }

  public GetName(): string {
    return this.interop.TypeShim.Sample.PersonInterop.GetName(this.instance);
  }

  public GetAge(): number {
    return this.interop.TypeShim.Sample.PersonInterop.GetAge(this.instance);
  }

  public GetPet(): Dog | null {
    const res = this.interop.TypeShim.Sample.PersonInterop.GetPet(this.instance);
    return res ? new DogProxy(res, this.interop) : null;
  }

}
// Auto-generated TypeScript statics class. Source class: TypeShim.Sample.Person
export class PersonStatics {
  private interop: WasmModuleExports;

  constructor(interop: WasmModuleExports) {
    this.interop = interop;
  }

}

