import { beforeAll } from 'vitest';
import { dotnet } from '_framework/dotnet';
import { TypeShimInitializer } from 'typeshim';

beforeAll(async () => {
  await initializeWASMRuntime();
});

let runtimeInfo: any = undefined;
async function initializeWASMRuntime(): Promise<void> {
  runtimeInfo = await dotnet.create();
  await TypeShimInitializer.initialize(runtimeInfo);
  runtimeInfo.runMain();
}
