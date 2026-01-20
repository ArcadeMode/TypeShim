import { beforeAll } from 'vitest';
import { dotnet } from '@typeshim/e2e-wasm-lib/dotnet';
import { TypeShimInitializer } from '@typeshim/e2e-wasm-lib';
import { isBrowserMode } from '../../vite.config.js';

beforeAll(async () => {
  if (!isBrowserMode()) {
    const { serveFetchRequestsFromDisk } = await import('./fetch-from-disk.js');
    serveFetchRequestsFromDisk();
  }
  await initializeWASMRuntime();
});

async function initializeWASMRuntime(): Promise<void> {
  const runtimeInfo = await dotnet.create();
  const config = runtimeInfo.getConfig();
  if (!config?.mainAssemblyName) {
    throw new Error('dotnet runtime config did not provide mainAssemblyName');
  }

  (runtimeInfo as any).assemblyExports ??= await runtimeInfo.getAssemblyExports(config.mainAssemblyName);

  TypeShimInitializer.initialize(runtimeInfo as any);
}
