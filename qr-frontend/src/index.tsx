import React from 'react';
//import { generate } from 'dotnet-qr';

console.log("React QR component loaded.");


import { dotnet } from 'dotnet-qr'
import { PersonName, Person, QRCode, PersonRepositoryInterop, PersonInterop, PersonRepository, WasmModuleExports, WasmModule } from './exported-types';

//export type QRWasmExports = {
//    QR: {
//        Wasm: {
//            QRCode: QRCode;
//            PersonInterop: PersonInterop;
//            PersonName: PersonName;
//            PersonRepositoryInterop: PersonRepositoryInterop;
//        }
//    }
//}

export class DotnetBootstrapper<TExports> {
    private dotnetObj: any | null = null;
    private exportsPromise: Promise<TExports> | null = null;

    public async Create(): Promise<TExports> {
        if (this.exportsPromise == null) {
            this.dotnetObj ??= await dotnet.create();
            const getAssemblyExports = this.dotnetObj.getAssemblyExports;
            const config = this.dotnetObj.getConfig();
            this.exportsPromise = getAssemblyExports(config.mainAssemblyName);
        }

        return await this.exportsPromise!;
    }
}

//export class QR_Wasm_Interop {
//    private exports: QRWasmExports;

//    constructor(exports: QRWasmExports) {
//        this.exports = exports;
//    }
//    //TODO: FEATURE: mark as included in Exports API?? (name it..)
//    public GetPersonRepository(): PersonRepository {
//        return new PersonRepositoryImpl(this.exports);
//    }
//}

//// KEY IDEAS
//// - INTEROP STATICS
//// - NO SUFFIX PUBLIC API (EITHER EQUALS INTEROP OR HAS FIRST ARG AS TARGET FOR DYNAMIC CALLS)
//// - IMPL CLASSES ON TS SIDE TO WRAP MANAGED OBJECTS AND INTEROP CALLS

//export class PersonRepositoryImpl implements PersonRepository {
//    private exports: QRWasmExports

//    public PlaceHolderOrTsGenNoWorkey_FixThis: string = "";

//    constructor(exports: QRWasmExports) {
//        this.exports = exports;
//    }

//    public GetPerson1(): Person {
//        return new PersonImpl(this.exports.QR.Wasm.PersonRepositoryInterop.GetPerson1(), this.exports.QR.Wasm.PersonInterop);
//    }

//    public GetPerson2(): Person {
//        return new PersonImpl(this.exports.QR.Wasm.PersonRepositoryInterop.GetPerson2(), this.exports.QR.Wasm.PersonInterop);
//    }
//}

//class PersonImpl implements Person {

//    PlaceHolderOrTsGenNoWorkey_FixThis: string = "";

//    private managedObj: Person;
//    private interop: PersonInterop;
//    constructor(managedObj: Person, interop: PersonInterop) {
//        this.managedObj = managedObj;
//        this.interop = interop;
//    }

//    GetName(): string {
//        return this.interop.GetName(this.managedObj);
//    }

//    SetName(name: PersonName): void {
//        this.interop.SetName(this.managedObj, name);
//    }
//}

let bootstrapper: DotnetBootstrapper<WasmModuleExports> | null = null;

export async function generate(text: string, pixelsPerBlock: number) {
    bootstrapper ??= new DotnetBootstrapper<WasmModuleExports>();
    const exports: WasmModuleExports = await bootstrapper.Create();
    const module = new WasmModule(exports)
    console.log("exports:", exports);

    //const managedObj = exports!.QR.Wasm.PersonRepositoryInterop.GetPerson1();

    //const person = new PersonImpl(managedObj, exports!.QR.Wasm.PersonInterop);

    

    const instance: PersonRepository = module.PersonRepository().GetInstance();
    const person: Person = instance.GetPerson1();
    console.log("PERSON before set", person, person.GetName(), ",", person.GetName());
    person.SetName({ Value: "TSNAME" });
    console.log("PERSON after set", person, person.GetName(), ",", person.GetName());
    return exports!.QR.Wasm.QRCode.Generate(text, pixelsPerBlock);
}


type QrImageProps = {
    text?: string;
    relativePath?: string; // add correct type if used in your code
};

export const QrImage: React.FC<QrImageProps> = ({ text, relativePath }) => {
  const [imageSrc, setImageSrc] = React.useState<string | null>(null);
  React.useEffect(() => {
    async function generateAsync() {
      if (text) {
        var image = await generate(text, 10);
        setImageSrc("data:image/bmp;base64, " + image);
      } else {
        setImageSrc(null);
      }
    }

    generateAsync();
  }, [text]);

  if (imageSrc) {
    return (<img src={imageSrc} />);
  }

  if (imageSrc === null) {
    return null;
  }

  return (
    <i>Loading...</i>
  );
}
