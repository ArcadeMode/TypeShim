import { beforeAll } from 'vitest';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { dotnet } from '_framework/dotnet';
import { TypeShimInitializer } from 'typeshim';
import { isBrowserMode } from '../../suite';

const wwwrootBase =
  pathToFileURL(resolve(import.meta.dirname, '../../../TypeShim.E2E.Wasm/bin/Debug/net10.0/wwwroot')).href + '/';

beforeAll(async () => {
  await initializeWASMRuntime();
});

let runtimeInfo: any = undefined;
async function initializeWASMRuntime(): Promise<void> {
  let builder: any = dotnet;
  if (!isBrowserMode) {
    builder = builder.withResourceLoader(
      (_type: string, _name: string, defaultUri: string) =>
        new URL(defaultUri.replace(/^\/+/, ''), wwwrootBase).href
    );
  }
  runtimeInfo = await builder.create();
  await TypeShimInitializer.initialize(runtimeInfo);
  runtimeInfo.runMain();
}
