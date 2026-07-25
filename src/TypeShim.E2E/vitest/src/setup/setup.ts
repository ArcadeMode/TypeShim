import { beforeAll } from 'vitest';
import { dotnet } from '_framework/dotnet';

beforeAll(async () => {
  await initializeWASMRuntime();
});

async function initializeWASMRuntime(): Promise<void> {
  const runtimeInfo = await dotnet.create();
  runtimeInfo.runMain();
}
