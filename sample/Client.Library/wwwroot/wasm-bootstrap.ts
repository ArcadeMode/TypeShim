import { dotnet } from './_framework/dotnet'

let runtime: any = null;
let runtimePromise: Promise<any> | null = null;
export async function createWasmRuntime(): Promise<any> {
    console.log("Creating WASM runtime...");
    if (runtimePromise) {
        console.warn("WASM runtime is already started. Not creating a new instance.");
        return runtimePromise;
    } else {
        runtimePromise = dotnet.create();
    }
    const runtimeInfo = await runtimePromise;
    console.log("WASM runtime info:", runtimeInfo);
    const { runMain } = runtimeInfo;
    runMain();
    return runtime = runtimeInfo;
};