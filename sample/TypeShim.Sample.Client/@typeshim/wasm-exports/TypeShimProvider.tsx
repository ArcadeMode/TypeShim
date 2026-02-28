import { createWasmRuntime, MyApp, TypeShimInitializer } from '@typeshim/wasm-exports';
import { useMemo, ReactNode, useEffect, useState } from 'react';

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
      } catch (err: any) {
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

    return () => { cancelled = true; }; // cleanup
  }, []);
    return error 
      ? (<div>Error: {error}</div>) 
      : loading 
        ? (<div>Loading...</div>) 
        : <>{children}</>;
}