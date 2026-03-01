import { createWasmRuntime, TypeShimInitializer } from '@typeshim/wasm-exports';
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
        const runtimeInfo = await createWasmRuntime();
        await TypeShimInitializer.initialize(runtimeInfo);
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