import { beforeAll } from 'vitest';
import { dotnet } from '_framework/dotnet';
import { TypeShimInitializer } from 'typeshim';
import { isBrowserMode } from '../../suite';

beforeAll(async () => {
  await initializeWASMRuntime();
});

let runtimeInfo: any = undefined;
async function initializeWASMRuntime(): Promise<void> {
  let builder: any = dotnet;
  if (!isBrowserMode) {
    const { resolve } = await import('node:path');
    const { pathToFileURL } = await import('node:url');
    const wwwrootBase =
      pathToFileURL(resolve(import.meta.dirname, '../../../TypeShim.E2E.Wasm/bin/Debug/net10.0/wwwroot')).href + '/';
    builder = builder.withResourceLoader(
      (_type: string, _name: string, defaultUri: string) =>
        new URL(defaultUri.replace(/^\/+/, ''), wwwrootBase).href
    );
  }
  runtimeInfo = await builder.create();
  await TypeShimInitializer.initialize(runtimeInfo);
  runtimeInfo.runMain();
}
