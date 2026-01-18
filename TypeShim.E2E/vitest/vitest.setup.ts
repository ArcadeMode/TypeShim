import { beforeAll } from 'vitest';
import { dotnet } from '../e2e-wasm-app/wwwroot/_framework/dotnet.js';
import { TypeShimInitializer } from '../e2e-wasm-app/typeshim';
import { e2eConfig } from './e2e.config';


beforeAll(async () => {
  if (!e2eConfig.browserMode) {
    const { installVitestFetchShim } = await import('./vitest.fetchShim');
    installVitestFetchShim();
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
