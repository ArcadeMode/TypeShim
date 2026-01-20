

export function isBrowserMode(): boolean {
  return !!(globalThis as any).__BROWSER_MODE__;
}