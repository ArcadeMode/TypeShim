import { dotnet } from '_framework/dotnet'
import { ReactNode, useEffect, useState } from 'react';

export interface AppProviderProps {
    children: ReactNode;
}

export function TypeShimProvider({ children }: AppProviderProps) {
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

let runtimePromise: Promise<any> | null = null;
export async function createWasmRuntime(): Promise<void> {
    if (runtimePromise) {
        return runtimePromise;
    } else {
        runtimePromise = dotnet.create();
    }
    const runtimeInfo = await runtimePromise;
    const { runMain } = runtimeInfo;
    runMain();
};