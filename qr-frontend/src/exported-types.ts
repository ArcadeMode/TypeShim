// Auto-generated TypeScript module exports interface
export interface WasmModuleExports {
    QR: {
        Wasm: {
            PersonInterop: PersonInterop;
            PersonRepositoryInterop: PersonRepositoryInterop;
            QRCodeInterop: QRCodeInterop;
            PersonNameInterop: PersonNameInterop;
        };
    };
}

// Auto-generated TypeScript interop interface. Source class: QR.Wasm.Person
export interface PersonInterop {
    GetName(instance: object): string;
    SetName(instance: object, name: string): void;
}

// Auto-generated TypeScript interop interface. Source class: QR.Wasm.PersonRepository
export interface PersonRepositoryInterop {
    GetPerson1(instance: object): Person;
    GetPerson2(instance: object): Person;
    GetInstance(): PersonRepository;
}

// Auto-generated TypeScript interop interface. Source class: QR.Wasm.QRCode
export interface QRCodeInterop {
    Generate(text: string, pixelsPerBlock: number): string;
}

// Auto-generated TypeScript interop interface. Source class: QR.Wasm.PersonName
export interface PersonNameInterop {
}

// Auto-generated TypeScript interface. Source class: QR.Wasm.Person
export interface Person {
    GetName(): string;
    SetName(name: string): void;
}

// Auto-generated TypeScript interface. Source class: QR.Wasm.PersonRepository
export interface PersonRepository {
    GetPerson1(): Person;
    GetPerson2(): Person;
}

// Auto-generated TypeScript interface. Source class: QR.Wasm.QRCode
export interface QRCode {
}

// Auto-generated TypeScript interface. Source class: QR.Wasm.PersonName
export interface PersonName {
}

export class WasmModule {
    private interop: WasmModuleExports
    constructor(interop: WasmModuleExports) {
        this.interop = interop;
    }
    public PersonRepository(): PersonRepositoryStatics {
        return new PersonRepositoryStatics(this.interop);
    }
    public QRCode(): QRCodeStatics {
        return new QRCodeStatics(this.interop);
    }
}

// Auto-generated TypeScript proxy class. Source class: QR.Wasm.Person
export class PersonProxy implements Person {
    private interop: WasmModuleExports;
    private instance: object;

    constructor(instance: object, interop: WasmModuleExports) {
        this.interop = interop;
        this.instance = instance;
    }

    public GetName(): string {
        return this.interop.QR.Wasm.PersonInterop.GetName(this.instance);
    }

    public SetName(name: string): void {
        this.interop.QR.Wasm.PersonInterop.SetName(this.instance, name);
    }

}
// Auto-generated TypeScript statics class. Source class: QR.Wasm.Person
export class PersonStatics {
    private interop: WasmModuleExports;

    constructor(interop: WasmModuleExports) {
        this.interop = interop;
    }

}

// Auto-generated TypeScript proxy class. Source class: QR.Wasm.PersonRepository
export class PersonRepositoryProxy implements PersonRepository {
    private interop: WasmModuleExports;
    private instance: object;

    constructor(instance: object, interop: WasmModuleExports) {
        this.interop = interop;
        this.instance = instance;
    }

    public GetPerson1(): Person {
        return new PersonProxy(this.interop.QR.Wasm.PersonRepositoryInterop.GetPerson1(this.instance), this.interop);
    }

    public GetPerson2(): Person {
        return new PersonProxy(this.interop.QR.Wasm.PersonRepositoryInterop.GetPerson2(this.instance), this.interop);
    }

}
// Auto-generated TypeScript statics class. Source class: QR.Wasm.PersonRepository
export class PersonRepositoryStatics {
    private interop: WasmModuleExports;

    constructor(interop: WasmModuleExports) {
        this.interop = interop;
    }

    public GetInstance(): PersonRepository {
        return new PersonRepositoryProxy(this.interop.QR.Wasm.PersonRepositoryInterop.GetInstance(), this.interop);
    }

}

// Auto-generated TypeScript proxy class. Source class: QR.Wasm.QRCode
export class QRCodeProxy implements QRCode {
    private interop: WasmModuleExports;
    private instance: object;

    constructor(instance: object, interop: WasmModuleExports) {
        this.interop = interop;
        this.instance = instance;
    }

}
// Auto-generated TypeScript statics class. Source class: QR.Wasm.QRCode
export class QRCodeStatics {
    private interop: WasmModuleExports;

    constructor(interop: WasmModuleExports) {
        this.interop = interop;
    }

    public Generate(text: string, pixelsPerBlock: number): string {
        return this.interop.QR.Wasm.QRCodeInterop.Generate(text, pixelsPerBlock);
    }

}

// Auto-generated TypeScript proxy class. Source class: QR.Wasm.PersonName
export class PersonNameProxy implements PersonName {
    private interop: WasmModuleExports;
    private instance: object;

    constructor(instance: object, interop: WasmModuleExports) {
        this.interop = interop;
        this.instance = instance;
    }

}
// Auto-generated TypeScript statics class. Source class: QR.Wasm.PersonName
export class PersonNameStatics {
    private interop: WasmModuleExports;

    constructor(interop: WasmModuleExports) {
        this.interop = interop;
    }

}

