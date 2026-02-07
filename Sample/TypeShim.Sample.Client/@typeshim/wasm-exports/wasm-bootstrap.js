import { dotnet } from '/_framework/dotnet.js'

let isStarted = null;

export async function createWasmRuntime(args) {
    if (isStarted) {
        throw new Error("The .NET WebAssembly runtime has already been started.");
    }
    isStarted = true;
    
    const runtimeInfo = await dotnet.withApplicationArguments(args).create();
    const { setModuleImports, getAssemblyExports, getConfig, runMain } = runtimeInfo;
    runMain(); // TODO: make configurable.
    
    const config = getConfig();
    return runtimeInfo; 
};