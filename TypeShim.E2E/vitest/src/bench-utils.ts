export type MemorySample = {
  rss: number;
  heapUsed: number;
  heapTotal: number;
  external: number;
};

export function sampleMemory(): MemorySample {
  const m = process.memoryUsage();
  return {
    rss: m.rss,
    heapUsed: m.heapUsed,
    heapTotal: m.heapTotal,
    external: m.external,
  };
}

export async function printSample(memSamplePromise: ReturnType<typeof measureMemoryDelta>) {
  const memSample = await memSamplePromise;
  const line = [
    `rss (total): ${formatBytes(memSample.after.rss)} (Δ ${formatBytes(memSample.delta.rss)})`,
    `heap: ${formatBytes(memSample.after.heapUsed)} (Δ ${formatBytes(memSample.delta.heapUsed)})`,
    `external: ${formatBytes(memSample.after.external)} (Δ ${formatBytes(memSample.delta.external)})`,
  ].join(' | ')
  console.log(line);
}

function formatBytes(bytes: number): string {
  const units = ['B', 'KB', 'MB', 'GB'] as const;
  let i = 0;
  let v = Math.abs(bytes);
  while (v >= 1024 && i < units.length - 1) {
    v /= 1024;
    i++;
  }
  v = bytes < 0 ? -v : v;
  return `${v.toFixed(2).padStart(7, ' ')} ${units[i]}`;
}

export async function gcIfAvailable(): Promise<void> {
  // Requires Node run with --expose-gc
  const gc = (globalThis as any).gc as undefined | (() => void);
  if (!gc) return;
  gc();
  // Let finalizers/microtasks settle a bit
  await new Promise((r) => setTimeout(r, 0));
}

export async function measureMemoryDelta(fn: () => void | Promise<void>) {
  await gcIfAvailable();
  const before = sampleMemory();

  await fn();

  await gcIfAvailable();
  const after = sampleMemory();

  return {
    before,
    after,
    delta: {
      rss: after.rss - before.rss,
      heapUsed: after.heapUsed - before.heapUsed,
      heapTotal: after.heapTotal - before.heapTotal,
      external: after.external - before.external,
    },
  };
}