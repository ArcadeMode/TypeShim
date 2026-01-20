

export const isBrowserMode: boolean = !!(globalThis as any).__BROWSER_MODE__;

export const isCI: boolean = !!(globalThis as any).__IS_CI__;