import React from 'react';

console.log("React QR component loaded.");


import { Person, Dog, PersonRepository, WasmModuleExports, WasmModule } from '../../QR.Wasm/publish/wwwroot/typeshim';

export class DotnetBootstrapper<TExports> {
    private dotnetObj: any | null = null;
    private exportsPromise: Promise<TExports> | null = null;

    public async Create(): Promise<TExports> {
        if (this.exportsPromise == null) {
            //@ts-ignore
            const dotnetModule = await import('../../QR.Wasm/publish/wwwroot/_framework/dotnet.js');
            this.dotnetObj ??= await (dotnetModule).dotnet.create();
            const getAssemblyExports = this.dotnetObj.getAssemblyExports;
            const config = this.dotnetObj.getConfig();
            this.exportsPromise = getAssemblyExports(config.mainAssemblyName);
        }

        return await this.exportsPromise!;
    }
}

let bootstrapper: DotnetBootstrapper<WasmModuleExports> | null = null;

export async function generate(text: string, pixelsPerBlock: number) {
    bootstrapper ??= new DotnetBootstrapper<WasmModuleExports>();
    const exports: WasmModuleExports = await bootstrapper.Create();
    const module = new WasmModule(exports)

    const repository: PersonRepository = module.PersonRepository().GetInstance();
    const person: Person = repository.GetPerson();
    console.log("Before:", person.GetName()); // prints "Alice"
    person.SetName("Bob");
    console.log("After:", person.GetName()); // prints "Bob"

    const pet: Dog = person.GetPet();
    console.log("pet.GetName()", pet.GetName()); // prints "Fido"

    return module.QRCode().Generate(text, pixelsPerBlock);
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
