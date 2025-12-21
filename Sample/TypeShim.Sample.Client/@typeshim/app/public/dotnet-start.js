import { dotnet } from './_framework/dotnet.js'

let wasmRuntimeInstance = null;

async function launchWasmRuntime(args) {
    console.info("Creating .NET WebAssembly runtime");
    if (!wasmRuntimeInstance) {
        wasmRuntimeInstance = await dotnet.withApplicationArguments(args).create();
    }
    const { setModuleImports, getAssemblyExports, getConfig, runMain } = wasmRuntimeInstance;
    const config = getConfig();
    const exports = await getAssemblyExports(config.mainAssemblyName);
    console.info("Invoking .NET WebAssembly Main method");
    runMain(); // this will keep the wasm runtime alive, also what allows debugging
    console.info(".NET WebAssembly Module started");
    return exports;
};

// Expose methods JSImport'ed by dotnet wasm module
window.getBaseURI = () => document.baseURI;

window.launchWasmRuntime = launchWasmRuntime;
// TODO: consider if user wants to customize this
// TODO: consider if user wants to run multiple runtimes
// TODO: consider if user wants to delay runtime creation (and accept debugging limitations)