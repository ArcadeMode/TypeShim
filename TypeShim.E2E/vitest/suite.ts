
// Skip tests that marshall invalid values through Promise to .NET
// currently the runtime dies when trying to marshall such values making the tests un-runnable
// https://github.com/dotnet/runtime/pull/123523
export const skipInvalidPromiseResolveValueTests: boolean = true;

export const isBrowserMode: boolean = !!(globalThis as any).__BROWSER_MODE__;

export const isCI: boolean = !!(globalThis as any).__IS_CI__;