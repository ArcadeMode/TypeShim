import { beforeAll, beforeEach } from 'vitest';
import { dotnet } from '@typeshim/e2e-wasm-lib/dotnet';
import { TypeShimInitializer } from '@typeshim/e2e-wasm-lib';
import { isBrowserMode } from '../../suite';

beforeAll(async () => {
  console.log('Initializing WASM Runtime for tests');
  if (!isBrowserMode) {
    console.log('Setting up fetch from disk for Node tests');
    const { serveFetchRequestsFromDisk } = await import('./fetch-from-disk.js');
    serveFetchRequestsFromDisk();
  }
  await initializeWASMRuntime();
  console.log('Initialized WASM Runtime for tests');
});

let runtimeInfo: any = undefined;
async function initializeWASMRuntime(): Promise<void> {
  runtimeInfo = await dotnet.create();
  const config = runtimeInfo.getConfig();
  if (!config?.mainAssemblyName) {
    throw new Error('dotnet runtime config did not provide mainAssemblyName');
  }
  
  (runtimeInfo as any).assemblyExports ??= await runtimeInfo.getAssemblyExports(config.mainAssemblyName);
  
  console.log('Running main assembly:', config.mainAssemblyName);
  runtimeInfo.runMain();
  TypeShimInitializer.initialize(runtimeInfo as any);
}
