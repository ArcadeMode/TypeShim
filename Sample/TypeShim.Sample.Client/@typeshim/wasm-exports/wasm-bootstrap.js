import { dotnet } from '/_framework/dotnet.js'

let assemblyExports = null;

export async function launchWasmRuntime(args) {
    if (assemblyExports) {
        return assemblyExports;
    }
    
    console.info("Creating .NET WebAssembly runtime");
    const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet.withApplicationArguments(args).create();
    console.info("Invoking .NET WebAssembly Main method");
    runMain(); // this will keep the wasm runtime alive, also what allows debugging
    console.info(".NET WebAssembly Module started");
    
    const config = getConfig();
    return assemblyExports = await getAssemblyExports(config.mainAssemblyName);
};

// Expose methods JSImport'ed by dotnet wasm module
window.getBaseURI = () => document.baseURI;

// TODO: consider if user wants to customize this
// TODO: consider if user wants to run multiple runtimes
// TODO: consider if user wants to delay runtime creation (and accept debugging limitations)