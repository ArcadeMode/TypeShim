import { dotnet } from '/_framework/dotnet.js'

let isStarted = null;

export async function createWasmRuntime(args) {
    if (isStarted) {
        throw new Error("The .NET WebAssembly runtime has already been started.");
    }
    isStarted = true;
    
    const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet.withApplicationArguments(args).create();
    runMain(); // TODO: make configurable.
    
    const config = getConfig();
    return { 
        assemblyExports: await getAssemblyExports(config.mainAssemblyName),
        setModuleImports: setModuleImports
    };
};