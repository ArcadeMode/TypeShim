import { beforeAll, beforeEach } from 'vitest';
import { dotnet } from '@typeshim/e2e-wasm-lib/dotnet';
import { TypeShimInitializer } from '@typeshim/e2e-wasm-lib';
import { isBrowserMode } from '../../suite';

beforeAll(async () => {
  if (!isBrowserMode) {
    const { serveFetchRequestsFromDisk } = await import('./fetch-from-disk.js');
    serveFetchRequestsFromDisk();
  }
  await initializeWASMRuntime();
});

let runtimeInfo: any = undefined;
async function initializeWASMRuntime(): Promise<void> {
  runtimeInfo = await dotnet.create();
  await TypeShimInitializer.initialize(runtimeInfo);
  runtimeInfo.runMain();
}
