import { dotnet } from './_framework/dotnet.js'

async function createWasmRuntime(args) {
    console.info("Creating .NET WebAssembly runtime");
    const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet.withApplicationArguments(args).create();

    const config = getConfig();
    const exports = await getAssemblyExports(config.mainAssemblyName);

    console.info("Invoking .NET WebAssembly Main method");
    runMain(); // this will keep the wasm runtime alive, also what allows debugging

    console.info(".NET WebAssembly Module started");
    return exports;
};

window.wasmModuleStarter = {
    exports: (async () =>{
        return await createWasmRuntime("BeepBoop");
    })()
};
// TODO: consider if user wants to customize this
// TODO: consider if user wants to run multiple runtimes
// TODO: consider if user wants to delay runtime creation (and accept debugging limitations)