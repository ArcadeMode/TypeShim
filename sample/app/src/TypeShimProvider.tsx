import { dotnet } from '_framework/dotnet'
import { ReactNode, useEffect, useState } from 'react';

export interface AppProviderProps {
    children: ReactNode;
}

export function TypeShimProvider({ children }: AppProviderProps) {
  const [runtime, setRuntime] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    async function load() {
      try {
        await createWasmRuntime();
        console.log("WASM Runtime initialized successfully.");
      } catch (err: any) {
        console.error("Error loading WASM runtime:", err);
        if (!cancelled) {
          setError(err);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }
    load();

    return () => { cancelled = true; console.log("CANCEL"); }; // cleanup
  }, []);
    return error 
      ? (<div>Error: {error}</div>) 
      : loading 
        ? (<div>Loading...</div>) 
        : (<>{children}</>);
}


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